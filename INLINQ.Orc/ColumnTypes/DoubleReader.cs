using INLINQ.Orc.Encodings;
using INLINQ.Orc.Stripes;
using System.Runtime.InteropServices;

namespace INLINQ.Orc.ColumnTypes
{
    public static class DoubleReader
    {
        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, double[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            byte[]? data = ColumnReader.ReadBinaryStream(stripeStreams, columnId, Protocol.StreamKind.Data);
            Span<byte> dataSpan = new Span<byte>(data);
            Span<double> columnSpan = new Span<double>(column);
            Span<byte> columnBytes = MemoryMarshal.Cast<double, byte>(columnSpan);
            dataSpan.CopyTo(columnBytes);
            return presentLength > 0;
        }

        public static IEnumerable<double?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            double[] column = new double[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            return ColumnReader.Read(presentMaps, column, hasPresentMap);
        }

    }
}
