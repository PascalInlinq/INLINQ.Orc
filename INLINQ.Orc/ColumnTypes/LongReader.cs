using INLINQ.Orc.Stripes;

namespace INLINQ.Orc.ColumnTypes
{
    public static class LongReader
    {
        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, long[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Data, true, column);
            return presentLength > 0;
        }

        public static IEnumerable<long?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            long[] column = new long[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            return ColumnReader.Read(presentMaps, column, hasPresentMap);
        }
    }
}
