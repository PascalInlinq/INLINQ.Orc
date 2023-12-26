using INLINQ.Orc.Stripes;

namespace INLINQ.Orc.ColumnTypes
{
    public static class ByteReader
    {
        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, byte[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            ColumnReader.ReadByteStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Data, column);
            return presentLength > 0;
        }

        public static IEnumerable<byte?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            byte[] column = new byte[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            return ColumnReader.Read(presentMaps, column, hasPresentMap);
        }
    }
}
