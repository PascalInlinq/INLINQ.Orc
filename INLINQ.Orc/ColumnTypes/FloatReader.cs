using INLINQ.Orc.Encodings;
using INLINQ.Orc.Stripes;
using System.Runtime.InteropServices;

namespace INLINQ.Orc.ColumnTypes
{
    public static class FloatReader
    {
        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, float[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            byte[]? data = ColumnReader.ReadBinaryStream(stripeStreams, columnId, Protocol.StreamKind.Data);
            Span<byte> dataSpan = new Span<byte>(data);
            Span<float> columnSpan = new Span<float>(column);
            Span<byte> columnBytes = MemoryMarshal.Cast<float, byte>(columnSpan);
            dataSpan.CopyTo(columnBytes);
            return presentLength > 0;
        }

        public static IEnumerable<float?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            float[] column = new float[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            return ColumnReader.Read(presentMaps, column, hasPresentMap);
        }
    }
}
