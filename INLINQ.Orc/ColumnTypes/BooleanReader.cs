using INLINQ.Orc.Stripes;

namespace INLINQ.Orc.ColumnTypes
{
    public static class BooleanReader
    {
        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, bool[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            uint count = ColumnReader.ReadBooleanStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Data, column);
            return presentLength > 0;
        }

        public static IEnumerable<bool?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            bool[] column = new bool[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            return ColumnReader.Read(presentMaps, column, hasPresentMap);
        }

        
    }
}
