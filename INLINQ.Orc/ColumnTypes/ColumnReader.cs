using INLINQ.Orc.Stripes;
using INLINQ.Orc.Encodings;
using System.Numerics;
using System.Diagnostics;
using INLINQ.Orc.Infrastructure;

namespace INLINQ.Orc.ColumnTypes
{
    public static class ColumnReader
    {   
        //public static long TimeDecompress { get; private set; }
        //public static long TimeReadAll { get; private set; }

        private static StripeStreamReader? GetStripeStream(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind)
        {
            return stripeStreams.FirstOrDefault(s => s.ColumnId == columnId && s.StreamKind == streamKind);
        }

        public static Protocol.ColumnEncodingKind? GetColumnEncodingKind(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind)
        {
            StripeStreamReader? stripeStream = GetStripeStream(stripeStreams, columnId, streamKind);
            return stripeStream == null ? null : stripeStream.ColumnEncodingKind;
        }

        public static void ReadNumericStreamToArray(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind, bool isSigned, long[] data)
        {
            StripeStreamReader? stripeStream = GetStripeStream(stripeStreams, columnId, streamKind);
            if (stripeStream == null)
            {
                throw new ArgumentException($"Stream {streamKind} not found for column {columnId}.");
            }

            if (stripeStream.ColumnEncodingKind != Protocol.ColumnEncodingKind.DirectV2 && stripeStream.ColumnEncodingKind != Protocol.ColumnEncodingKind.DictionaryV2)
            {
                throw new NotImplementedException($"Unimplemented Numeric {nameof(Protocol.ColumnEncodingKind)} {stripeStream.ColumnEncodingKind}");
            }

            //Stopwatch sw = new();
            //sw.Start();
            long lastStop = 0;
            ConcatenatingStream stream = stripeStream.GetDecompressedStream();
            //TimeDecompress -= lastStop - (lastStop = sw.ElapsedMilliseconds);
            IntegerRunLengthEncodingV2Reader.ReadToArray(stream, isSigned, data);
            //TimeReadAll += sw.ElapsedMilliseconds;
        }

        public static uint ReadBooleanStreamToPresentMap(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind, byte[] presentMap)
        {
            StripeStreamReader? stripeStream = GetStripeStream(stripeStreams, columnId, streamKind);
            if (stripeStream == null)
            {
                return 0;
            }

            ConcatenatingStream stream = stripeStream.GetDecompressedStream();
            BitReader reader = new(stream);
            uint length = reader.ReadToArray(presentMap);
            return length;
        }

        public static uint ReadBooleanStreamToArray(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind, bool[] data)
        {
            StripeStreamReader? stripeStream = GetStripeStream(stripeStreams, columnId, streamKind);
            if (stripeStream == null)
            {
                return 0;
            }

            ConcatenatingStream stream = stripeStream.GetDecompressedStream();
            BitReader reader = new(stream);
            uint length = reader.ReadToArray(data);
            return length;
        }

        public static byte[]? ReadBinaryStream(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind)
        {
            StripeStreamReader? stripeStream = GetStripeStream(stripeStreams, columnId, streamKind);
            if (stripeStream == null)
            {
                return null;
            }

            ConcatenatingStream stream = stripeStream.GetDecompressedStream();
            return stream.ReadAll();
        }

        public static void ReadByteStreamToArray(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind, byte[] data)
        {
            Console.WriteLine(Helpers.DebuggerHelper.GetTimeString() + "ColumnReader.ReadByteStream");
            StripeStreamReader? stripeStream = GetStripeStream(stripeStreams, columnId, streamKind);
            if (stripeStream != null)
            {
                ConcatenatingStream stream = stripeStream.GetDecompressedStream();
                ByteRunLengthEncodingReader? reader = new(stream);
                byte[]? array = reader.Read().ToArray();

                uint dataIndex = 0;
                foreach (byte value in array)
                {
                    data[dataIndex++] = value;
                }
            }
        }

        public static BigInteger[]? ReadVarIntStream(StripeStreamReaderCollection stripeStreams, uint columnId, Protocol.StreamKind streamKind)
        {
            Console.WriteLine(Helpers.DebuggerHelper.GetTimeString() + "ColumnReader.ReadVarIntStream");
            StripeStreamReader? stripeStream = GetStripeStream(stripeStreams, columnId, streamKind);
            if (stripeStream == null)
            {
                return null;
            }

            ConcatenatingStream stream = stripeStream.GetDecompressedStream();
            return stream.ReadAllBigVarInt().ToArray(); //TODO: inefficient?
        }

        public static IEnumerable<T?> Read<T>(byte[] presentMaps, T[] column, bool hasPresent) where T : struct
        {
            int count = column.Length;
            if (hasPresent)
            {
                int valueId = 0;
                int mapId = 0;
                byte currentMap = 0;
                for (int i = 0; i < count; i++)
                {
                    if (((byte)i & 7) == 0)
                    {
                        currentMap = presentMaps[mapId++];
                    }

                    var result = currentMap >= 0x80 ? column[valueId++] : (T?)null;
                    currentMap <<= 1;
                    yield return result;
                    
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    yield return column[i];
                }
            }
        }
    }
}