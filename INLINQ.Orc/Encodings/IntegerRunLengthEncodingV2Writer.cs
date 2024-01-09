using INLINQ.Orc.Infrastructure;
using System;
using System.Diagnostics;

namespace INLINQ.Orc.Encodings
{
    public static class IntegerRunLengthEncodingV2Writer
    {
        private const int WINDOW_SIZE = 512;

        public static void Write(Stream outputStream, ReadOnlySpan<long> values, bool areSigned, bool aligned)
        {
            int valueCount = values.Length;
            int position = 0;
            int maxWindowsBuffer = WINDOW_SIZE * (sizeof(long) + 1); /* gross overestimation of maximum buffer required */
            byte[] streamBuffer = new byte[200 * maxWindowsBuffer];
            int streamPosition = 0;
            while (position < valueCount)
            {
                int streamIndex = 0;
                ReadOnlySpan<long> window = values.Slice(position, position + WINDOW_SIZE <= valueCount ? WINDOW_SIZE : valueCount - position);
                var positionedBuffer = new Span<byte>(streamBuffer, streamPosition, maxWindowsBuffer);
                int numValuesEncoded = EncodeValues(positionedBuffer, ref streamIndex, window, areSigned, aligned);
                position += numValuesEncoded;
                streamPosition += streamIndex;
                if (streamPosition + maxWindowsBuffer > streamBuffer.Length || position == valueCount)
                {
                    //flush necessary:
                    outputStream.Write(streamBuffer, 0, streamPosition);
                    streamPosition = 0;
                }

            }
        }

        private static int EncodeValues(Span<byte> stream, ref int streamIndex, ReadOnlySpan<long> values, bool areSigned, bool aligned)
        {
            //Eventually:
            //Find the longest monotonically increasing or decreasing (or constant) segment of data in the next 1024 samples
            //If the length is less than 10 and is constant, use SHORT_REPEAT
            //For data before and after segment, consider using PATCHED_BASE.  Otherwise fall back on DIRECT.

            //For now, match the Java implementation
            //Count how many values repeat in the next 512 samples
            //If it's less than 10 and more than 3, use SHORT_REPEAT
            //Otherwise, try to use DELTA
            //If values aren't monotonically increasing or decreasing, check if PATCHED_BASE makes sense (90% of the values are one bit-width less than the 100% number)
            //If all else fails, use DIRECT
            int? fixedBitWidth = null;
            ReadOnlySpan<long> zigZaggedValues;
            if (SequenceIsTooShort(values))
            {
                zigZaggedValues = values;
                if (areSigned)
                {
                    long[] zigs = new long[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        zigs[i] = values[i].ZigzagEncode();
                    }
                    //copyCount += values.Length;
                    zigZaggedValues = new ReadOnlySpan<long>(zigs);
                }
                DirectEncode(stream, ref streamIndex, zigZaggedValues, values.Length, aligned, fixedBitWidth);
                return values.Length;
            }

            FindRepeatedValues(values, out long repeatingValue, out int length);
            if (length >= 3 && length <= 10)
            {
                ShortRepeatEncode(stream, ref streamIndex, areSigned ? repeatingValue.ZigzagEncode() : repeatingValue, length);
                return length;
            }

            DeltaEncodingResult result = TryDeltaEncoding(stream, ref streamIndex, values, areSigned, aligned, out length, out long minValue);
            if (result == DeltaEncodingResult.Success)
            {
                return length;
            }
            else if (result == DeltaEncodingResult.Overflow)
            {
                zigZaggedValues = values;
                if (areSigned)
                {
                    long[] zigs = new long[values.Length];
                    for (int i = 0; i < values.Length; i++)
                    {
                        zigs[i] = values[i].ZigzagEncode();
                    }
                    //copyCount += values.Length;
                    zigZaggedValues = new ReadOnlySpan<long>(zigs);
                }
                DirectEncode(stream, ref streamIndex, zigZaggedValues, values.Length, aligned, fixedBitWidth);
                return values.Length;
            }

            //At this point we must zigzag
            zigZaggedValues = values;
            if (areSigned)
            {
                long[] zigs = new long[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    zigs[i] = values[i].ZigzagEncode();
                }
                //copyCount += values.Length;
                zigZaggedValues = new ReadOnlySpan<long>(zigs);
            }

            if (TryPatchEncoding(stream, ref streamIndex, zigZaggedValues, values, minValue, ref fixedBitWidth, out length))
            {
                return length;
            }

            //If all else fails, DIRECT encode
            DirectEncode(stream, ref streamIndex, zigZaggedValues, values.Length, aligned, fixedBitWidth);
            return values.Length;
        }

        private static void FindRepeatedValues(ReadOnlySpan<long> values, out long repeatingValue, out int length)
        {
            length = 0;
            repeatingValue = 0;

            bool isFirst = true;
            foreach (long value in values)
            {
                if (isFirst)
                {
                    repeatingValue = value;
                    isFirst = false;
                }
                else if (repeatingValue != value)
                {
                    break;
                }

                length++;
            }
        }

        private enum DeltaEncodingResult { Success, Overflow, NonMonotonic }

        private static DeltaEncodingResult TryDeltaEncoding(Span<byte> outputStream, ref int streamIndex, ReadOnlySpan<long> values, bool areSigned, bool aligned, out int length, out long minValue)
        {
            long[]? deltas = new long[values.Length - 1];
            long initialValue = values[0];
            minValue = initialValue;                        //This gets saved for the patch base if things don't work out here
            long maxValue = initialValue;
            long initialDelta = values[1] - initialValue;
            long curDelta = initialDelta;
            long deltaMax = 0;      //This is different from the java implementation.  I believe their implementation may be a bug.
                                    //The first delta value is not considered for the delta bit width, so don't include it in the max value calculation
            bool isIncreasing = initialDelta > 0;
            bool isDecreasing = initialDelta < 0;
            bool isConstantDelta = true;

            long previousValue = values[1];
            if (values[1] < minValue)
            {
                minValue = values[1];
            }

            if (values[1] > maxValue)
            {
                maxValue = values[1];
            }

            deltas[0] = initialDelta;

            int i = 2;
            while (i < values.Length)  //The first value is initialValue. The second value is initialDelta, already loaded. Start with the third value
            {
                long value = values[i];
                curDelta = value - previousValue;
                if (value < minValue)
                {
                    minValue = value;
                }

                if (value > maxValue)
                {
                    maxValue = value;
                }

                if (value < previousValue)
                {
                    isIncreasing = false;
                }

                if (value > previousValue)
                {
                    isDecreasing = false;
                }

                if (curDelta != initialDelta)
                {
                    isConstantDelta = false;
                }

                long absCurrDelta = Math.Abs(curDelta);
                deltas[i - 1] = absCurrDelta;
                if (absCurrDelta > deltaMax)
                {
                    deltaMax = absCurrDelta;
                }

                i++;
                previousValue = value;
            }

            if (BitManipulation.SubtractionWouldOverflow(maxValue, minValue))
            {
                length = 0;
                return DeltaEncodingResult.Overflow;
            }

            if (maxValue == minValue)   //All values after the first were identical
            {
                DeltaEncode(outputStream, ref streamIndex, minValue, areSigned, values.Length);
                length = values.Length;
                return DeltaEncodingResult.Success;
            }

            if (isConstantDelta) //All values changed by set amount
            {
                DeltaEncode(outputStream, ref streamIndex, initialValue, areSigned, curDelta, values.Length);
                length = values.Length;
                return DeltaEncodingResult.Success;
            }

            if (isIncreasing || isDecreasing)
            {
                int deltaBits = BitManipulation.NumBits((ulong)deltaMax);
                if (aligned)
                {
                    deltaBits = BitManipulation.FindNearestAlignedDirectWidth(deltaBits);
                }
                else
                {
                    deltaBits = BitManipulation.FindNearestDirectWidth(deltaBits);
                }

                DeltaEncode(outputStream, ref streamIndex, initialValue, areSigned, values.Length, deltas, deltaBits);
                length = values.Length;
                return DeltaEncodingResult.Success;
            }

            length = 0;
            return DeltaEncodingResult.NonMonotonic;
        }

        private static bool TryPatchEncoding(Span<byte> outputStream, ref int streamIndex, ReadOnlySpan<long> zigZagValues, ReadOnlySpan<long> values, long minValue, ref int? fixedBitWidth, out int length)
        {
            Tuple<int, int[]>? zigZagValuesHistogram = zigZagValues.GenerateHistogramOfBitWidths();
            int zigZagHundredthBits = BitManipulation.GetBitsRequiredForPercentile(zigZagValuesHistogram, 1.0);
            fixedBitWidth = zigZagHundredthBits;            //We'll use this later if if end up DIRECT encoding
            int zigZagNinetiethBits = BitManipulation.GetBitsRequiredForPercentile(zigZagValuesHistogram, 0.9);
            if (zigZagHundredthBits - zigZagNinetiethBits == 0)
            {
                //Requires as many bits even if we eliminate 10% of the most difficult values
                length = 0;
                return false;
            }

            long[]? baseReducedValues = new long[values.Length];
            int i = 0;
            foreach (long value in values)
            {
                baseReducedValues[i++] = value - minValue;
            }

            Tuple<int, int[]>? baseReducedValuesHistogram = baseReducedValues.GenerateHistogramOfBitWidths();
            int baseReducedHundredthBits = BitManipulation.GetBitsRequiredForPercentile(baseReducedValuesHistogram, 1.0);
            int baseReducedNinetyfifthBits = BitManipulation.GetBitsRequiredForPercentile(baseReducedValuesHistogram, 0.95);
            if (baseReducedHundredthBits - baseReducedNinetyfifthBits == 0)
            {
                //In the end, no benefit could be realized from patching
                length = 0;
                return false;
            }

            PatchEncode(outputStream, ref streamIndex, minValue, baseReducedValues, baseReducedHundredthBits, baseReducedNinetyfifthBits);
            length = values.Length;
            return true;
        }

        private static bool SequenceIsTooShort(ReadOnlySpan<long> values)
        {
            if (values.Length <= 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void DirectEncode(Span<byte> outputStream, ref int streamIndex, ReadOnlySpan<long> values, int numValues, bool aligned, int? precalculatedFixedBitWidth)
        {
            int fixedBitWidth;
            if (precalculatedFixedBitWidth.HasValue)
            {
                fixedBitWidth = precalculatedFixedBitWidth.Value;
            }
            else
            {
                Tuple<int, int[]>? histogram = values.GenerateHistogramOfBitWidths();
                fixedBitWidth = BitManipulation.GetBitsRequiredForPercentile(histogram, 1.0);
            }

            if (aligned)
            {
                fixedBitWidth = BitManipulation.FindNearestAlignedDirectWidth(fixedBitWidth);
            }
            else
            {
                fixedBitWidth = BitManipulation.FindNearestDirectWidth(fixedBitWidth);
            }

            int encodedFixedBitWidth = fixedBitWidth.EncodeDirectWidth();

            int byte1 = 0;
            byte1 |= 0x1 << 6;                              //7..6 Encoding Type
            byte1 |= (encodedFixedBitWidth & 0x1f) << 1;    //5..1 Fixed Width
            byte1 |= (numValues - 1) >> 8;                  //0    MSB of length
            int byte2 = (numValues - 1) & 0xff;             //7..0 LSBs of length

            outputStream[streamIndex++] = (byte)byte1;
            outputStream[streamIndex++] = (byte)byte2;
            outputStream.WriteBitpackedIntegersSpan(ref streamIndex, values, fixedBitWidth);
        }

        private static void ShortRepeatEncode(Span<byte> outputStream, ref int streamIndex, long value, int repeatCount)
        {
            int bits = BitManipulation.FindNearestDirectWidth(BitManipulation.NumBits((ulong)value));
            int width = bits / 8;
            if (bits % 8 != 0)
            {
                width++;      //Some remainder
            }

            int byte1 = 0;
            byte1 |= 0x0 << 6;
            byte1 |= (width - 1) << 3;
            byte1 |= repeatCount - 3;

            outputStream[streamIndex++] = (byte)byte1;
            outputStream.WriteLongBE(ref streamIndex, width, value);
        }

        private static void DeltaEncode(Span<byte> outputStream, ref int streamIndex, long initialValue, bool areSigned, int repeatCount)
        {
            DeltaEncode(outputStream, ref streamIndex, initialValue, areSigned, 0, repeatCount);
        }

        private static void DeltaEncode(Span<byte> outputStream, ref int streamIndex, long initialValue, bool areSigned, long constantOffset, int repeatCount)
        {
            DeltaEncode(outputStream, ref streamIndex, initialValue, areSigned, repeatCount, new[] { constantOffset }, 0);
        }

        private static void DeltaEncode(Span<byte> outputStream, ref int streamIndex, long initialValue, bool areSigned, int numValues, long[] deltas, int deltaBitWidth)
        {
            if (deltaBitWidth == 1)
            {
                deltaBitWidth = 2;      //encodedBitWidth of zero is reserved for constant runlengths. Allocate an extra bit to avoid triggering that logic.
            }

            int encodedBitWidth = deltaBitWidth > 1 ? deltaBitWidth.EncodeDirectWidth() : 0;

            int byte1 = 0;
            byte1 |= 0x3 << 6;                              //7..6 Encoding Type
            byte1 |= (encodedBitWidth & 0x1f) << 1;         //5..1 Delta Bit Width
            byte1 |= (numValues - 1) >> 8;                  //0    MSB of length
            int byte2 = (numValues - 1) & 0xff;             //7..0 LSBs of length

            outputStream[streamIndex++] = (byte)byte1;
            outputStream[streamIndex++] = (byte)byte2;
            if (areSigned)
            {
                outputStream.WriteVarIntSigned(ref streamIndex, initialValue);                          //Base Value
            }
            else
            {
                outputStream.WriteVarIntUnsigned(ref streamIndex, initialValue);
            }

            outputStream.WriteVarIntSigned(ref streamIndex, deltas[0]);                                 //Delta Base
            if (deltas.Length > 1)
            {
                outputStream.WriteBitpackedIntegers(ref streamIndex, deltas.Skip(1), deltaBitWidth);    //Delta Values
            }
        }

        private static void PatchEncode(Span<byte> outputStream, ref int streamIndex, long baseValue, long[] baseReducedValues, int originalBitWidth, int reducedBitWidth)
        {
            bool baseIsNegative = baseValue < 0;
            if (baseIsNegative)
            {
                baseValue = -baseValue;
            }

            int numBitsBaseValue = BitManipulation.NumBits((ulong)baseValue) + 1;   //Need one additional bit for the sign
            int numBytesBaseValue = numBitsBaseValue / 8;
            if (numBitsBaseValue % 8 != 0)
            {
                numBytesBaseValue++;      //Some remainder
            }

            if (baseIsNegative)
            {
                baseValue |= 1L << ((numBytesBaseValue * 8) - 1);   //Set the MSB to 1 to mark the sign
            }

            int patchBitWidth = BitManipulation.FindNearestDirectWidth(originalBitWidth - reducedBitWidth);
            if (patchBitWidth == 64)
            {
                patchBitWidth = 56;
                reducedBitWidth = 8;
            }
            int encodedPatchBitWidth = patchBitWidth.EncodeDirectWidth();
            int valueBitWidth = BitManipulation.FindNearestDirectWidth(reducedBitWidth);
            int encodedValueBitWidth = valueBitWidth.EncodeDirectWidth();

            long[]? patchGapList = GeneratePatchList(baseReducedValues, patchBitWidth, reducedBitWidth, out int gapBitWidth);
            int patchListBitWidth = BitManipulation.FindNearestDirectWidth(gapBitWidth + patchBitWidth);


            int byte1 = 0, byte2 = 0, byte3 = 0, byte4 = 0;
            byte1 |= 0x2 << 6;                                  //7..6 Encoding Type
            byte1 |= (encodedValueBitWidth & 0x1f) << 1;        //5..1 Value Bit Width
            byte1 |= (baseReducedValues.Length - 1) >> 8;       //0    MSB of length
            byte2 |= (baseReducedValues.Length - 1) & 0xff;     //7..0 LSBs of length
            byte3 |= (numBytesBaseValue - 1) << 5;              //7..5 Base Value Byte Width
            byte3 |= encodedPatchBitWidth & 0x1f;               //4..0 Encoded Patch Bit Width
            byte4 |= (gapBitWidth - 1) << 5;                    //7..5 Gap Bit Width
            byte4 |= patchGapList.Length & 0x1f;                //4..0 Patch/Gap List Length

            outputStream[streamIndex++] = (byte)byte1;
            outputStream[streamIndex++] = (byte)byte2;
            outputStream[streamIndex++] = (byte)byte3;
            outputStream[streamIndex++] = (byte)byte4;
            outputStream.WriteLongBE(ref streamIndex, numBytesBaseValue, baseValue);
            outputStream.WriteBitpackedIntegersSpan(ref streamIndex, baseReducedValues, valueBitWidth);
            outputStream.WriteBitpackedIntegersSpan(ref streamIndex, patchGapList, patchListBitWidth);
        }

        private static long[] GeneratePatchList(long[] baseReducedValues, int patchBitWidth, int reducedBitWidth, out int gapBitWidth)
        {
            int prevIndex = 0;
            int maxGap = 0;

            long mask = (1L << reducedBitWidth) - 1;

            int estimatedPatchCount = (int)(baseReducedValues.Length * 0.05 + .5);      //We're patching 5% of the values (round up)
            List<Tuple<int, long>>? patchGapList = new(estimatedPatchCount);

            for (int i = 0; i < baseReducedValues.Length; i++)
            {
                if (baseReducedValues[i] > mask)
                {
                    int gap = i - prevIndex;
                    if (gap > maxGap)
                    {
                        maxGap = gap;
                    }

                    long patch = (long)((ulong)baseReducedValues[i] >> reducedBitWidth);
                    patchGapList.Add(Tuple.Create(gap, patch));

                    baseReducedValues[i] &= mask;
                    prevIndex = i;
                }
            }

            int actualLength = patchGapList.Count;

            if (maxGap == 0 && patchGapList.Count != 0)
            {
                gapBitWidth = 1;
            }
            else
            {
                gapBitWidth = BitManipulation.FindNearestDirectWidth(BitManipulation.NumBits((ulong)maxGap));
            }

            if (gapBitWidth > 8)
            {
                //Prepare for the special case of 511 and 256
                gapBitWidth = 8;
                if (maxGap == 511)
                {
                    actualLength += 2;
                }
                else
                {
                    actualLength += 1;
                }
            }

            int resultIndex = 0;
            long[]? result = new long[actualLength];
            foreach (Tuple<int, long>? patchGap in patchGapList)
            {
                long gap = patchGap.Item1;
                long patch = patchGap.Item2;
                while (gap > 255)
                {
                    result[resultIndex++] = 255L << patchBitWidth;
                    gap -= 255;
                }
                result[resultIndex++] = gap << patchBitWidth | patch;
            }

            return result;
        }
    }
}
