using INLINQ.Orc.Infrastructure;
using System.Diagnostics;

namespace INLINQ.Orc.Encodings
{
    public class IntegerRunLengthEncodingV2Reader
    {
        private enum EncodingType { ShortRepeat, Direct, PatchedBase, Delta }

        public static long timeIntegerStreamReading { get; private set; }
        public static long countInteger0 { get; private set; }
        public static long countInteger1 { get; private set; }
        public static long countInteger2 { get; private set; }
        public static long countInteger3 { get; private set; }
        public static long countDeltaFixed { get; private set; }
        public static long countDeltaDyn { get; private set; }

        public static void ReadToArray(ConcatenatingStream inputStream, bool isSigned, long[] data)
        {
            //Stopwatch sw = new();
            //sw.Start();
            byte[] inputStreamBuffer = inputStream.ReadAll();
            uint streamIndex = 0;
            //timeIntegerStreamReading += sw.ElapsedMilliseconds;

            int dataIndex = 0;
            while (streamIndex < inputStreamBuffer.Length)
            {
                int firstByte = inputStreamBuffer[streamIndex++];

                int encodingType = (firstByte >> 6) & 0x3;
                switch (encodingType)
                {
                    case (int)EncodingType.ShortRepeat:
                        countInteger0++;
                        ReadShortRepeatValues(inputStreamBuffer, ref streamIndex, isSigned, firstByte, data, ref dataIndex);
                        break;
                    case (int)EncodingType.Direct:
                        countInteger1++;
                        ReadDirectValues(inputStreamBuffer, ref streamIndex, isSigned, firstByte, data, ref dataIndex);
                        break;
                    case (int)EncodingType.PatchedBase:
                        countInteger2++;
                        ReadPatchedBaseValues(inputStreamBuffer, ref streamIndex, firstByte, data, ref dataIndex);
                        break;
                    case (int)EncodingType.Delta:
                        countInteger3++;
                        ReadDeltaValues(inputStreamBuffer, ref streamIndex, isSigned, firstByte, data, ref dataIndex);
                        break;
                }
            }
        }

        private static void ReadShortRepeatValues(byte[] inputStreamBuffer, ref uint streamIndex, bool isSigned, int firstByte, long[] data, ref int dataIndex)
        {
            int width = ((firstByte >> 3) & 0x7) + 1;
            int repeatCount = (firstByte & 0x7) + 3;
            long value = inputStreamBuffer.ReadLongBE(width, ref streamIndex);
            if (isSigned)
            {
                value = value.ZigzagDecode();
            }

            for (int i = 0; i < repeatCount; i++)
            {
                data[dataIndex++] = value;
            }
        }

        private static void ReadDirectValues(byte[] inputStreamBuffer, ref uint streamIndex, bool isSigned, int firstByte, long[] data, ref int dataIndex)
        {
            int encodedWidth = (firstByte >> 1) & 0x1f;
            int width = encodedWidth.DecodeDirectWidth();
            int length = (firstByte & 0x1) << 8;
            length |= inputStreamBuffer[streamIndex++];
            length += 1;
            BitManipulation.StreamIndex index = new BitManipulation.StreamIndex { theStreamIndex = streamIndex };
            foreach (long value in inputStreamBuffer.ReadBitpackedIntegers(index, width, length))
            {
                if (isSigned)
                {
                    data[dataIndex++] = value.ZigzagDecode();
                }
                else
                {
                    data[dataIndex++] = value;
                }
            }

            streamIndex = index.theStreamIndex;
        }

        private static void ReadPatchedBaseValues(byte[] inputStreamBuffer, ref uint streamIndex, int firstByte, long[] data, ref int dataIndex)
        {
            int encodedWidth = (firstByte >> 1) & 0x1f;
            int width = encodedWidth.DecodeDirectWidth();
            int length = (firstByte & 0x1) << 8;
            length |= inputStreamBuffer[streamIndex++];
            length += 1;

            byte thirdByte = inputStreamBuffer[streamIndex++];
            int baseValueWidth = ((thirdByte >> 5) & 0x7) + 1;
            int encodedPatchWidth = thirdByte & 0x1f;
            int patchWidth = encodedPatchWidth.DecodeDirectWidth();

            byte fourthByte = inputStreamBuffer[streamIndex++];
            int patchGapWidth = ((fourthByte >> 5) & 0x7) + 1;
            int patchListLength = fourthByte & 0x1f;

            long baseValue = inputStreamBuffer.ReadLongBE(baseValueWidth, ref streamIndex);
            long msbMask = (1L << ((baseValueWidth * 8) - 1));
            if ((baseValue & msbMask) != 0)
            {
                baseValue = baseValue & ~msbMask;
                baseValue = -baseValue;
            }

            //Buffer all the values so we can patch them
            BitManipulation.StreamIndex index = new BitManipulation.StreamIndex() { theStreamIndex = streamIndex };
            long[]? dataValues = inputStreamBuffer.ReadBitpackedIntegers(index, width, length).ToArray();

            if (patchGapWidth + patchWidth > 64)
            {
                throw new InvalidDataException($"{nameof(patchGapWidth)} ({patchGapWidth}) + {nameof(patchWidth)} ({patchWidth}) > 64");
            }

            int patchListWidth = BitManipulation.FindNearestDirectWidth(patchWidth + patchGapWidth);
            long[]? patchListValues = inputStreamBuffer.ReadBitpackedIntegers(index, patchListWidth, patchListLength).ToArray();
            streamIndex = index.theStreamIndex;

            int patchIndex = 0;
            long gap = 0;
            GetNextPatch(patchListValues, ref patchIndex, ref gap, out long patch, patchWidth, (1L << patchWidth) - 1);

            for (int i = 0; i < length; i++)
            {
                if (i == gap)
                {
                    long patchedValue = dataValues[i] | (patch << width);
                    data[dataIndex++] = baseValue + patchedValue;

                    if (patchIndex < patchListLength)
                    {
                        GetNextPatch(patchListValues, ref patchIndex, ref gap, out patch, patchWidth, (1L << patchWidth) - 1);
                    }
                }
                else
                {
                    data[dataIndex++] = baseValue + dataValues[i];
                }
            }
        }

        private static void GetNextPatch(long[] patchListValues, ref int patchIndex, ref long gap, out long patch, int patchWidth, long patchMask)
        {
            long raw = patchListValues[patchIndex];
            patchIndex++;
            long curGap = (long)((ulong)raw >> patchWidth);
            patch = raw & patchMask;
            while (curGap == 255 && patch == 0)
            {
                gap += 255;
                raw = patchListValues[patchIndex];
                patchIndex++;
                curGap = (long)((ulong)raw >> patchWidth);
                patch = raw & patchMask;
            }
            gap += curGap;
        }

        private static void ReadDeltaValues(byte[] inputStreamBuffer, ref uint streamIndex, bool isSigned, int firstByte, long[] data, ref int dataIndex)
        {
            int encodedWidth = (firstByte >> 1) & 0x1f;
            int width = 0;
            if (encodedWidth != 0)      //EncodedWidth 0 means Width 0 for Delta
            {
                width = encodedWidth.DecodeDirectWidth();
            }

            int length = (firstByte & 0x1) << 8;
            length |= inputStreamBuffer[streamIndex++];
            //Delta lengths start at 0

            long currentValue;
            if (isSigned)
            {
                currentValue = inputStreamBuffer.ReadVarIntSigned(ref streamIndex);
            }
            else
            {
                currentValue = inputStreamBuffer.ReadVarIntUnsigned(ref streamIndex);
            }

            data[dataIndex++] = currentValue;

            long deltaBase = inputStreamBuffer.ReadVarIntSigned(ref streamIndex);
            if (width == 0)
            {
                //Uses a fixed delta base for every value
                for (int i = 0; i < length; i++)
                {
                    countDeltaFixed++;
                    currentValue += deltaBase;
                    data[dataIndex++] = currentValue;
                }
            }
            else
            {
                currentValue += deltaBase;
                data[dataIndex++] = currentValue;

                BitManipulation.StreamIndex index = new() { theStreamIndex = streamIndex };
                IEnumerable<long>? deltaValues = inputStreamBuffer.ReadBitpackedIntegers(index, width, length - 1);
                if (deltaBase > 0)
                {
                    foreach (long deltaValue in deltaValues)
                    {
                        countDeltaDyn++;
                        currentValue += deltaValue;
                        data[dataIndex++] = currentValue;
                    }
                }
                else
                {
                    foreach (long deltaValue in deltaValues)
                    {
                        countDeltaDyn++;
                        currentValue -= deltaValue;
                        data[dataIndex++] = currentValue;
                    }
                }

                streamIndex = index.theStreamIndex;
            }
        }
    }
}
