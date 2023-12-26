using INLINQ.Orc.Stripes;

namespace INLINQ.Orc.ColumnTypes
{
    public static class DateReader
    {
        private static readonly DateTime _unixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, DateTime[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            long[] data = new long[column.Length];
            ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Data, true, data);
            int valueId = 0;
            while (valueId < column.Length)
            {
                long value = data[valueId];
                column[valueId++] = _unixEpoch.AddTicks(value * TimeSpan.TicksPerDay);
            }

            return presentLength > 0;
        }

        public static IEnumerable<DateTime?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            DateTime[] column = new DateTime[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            return ColumnReader.Read(presentMaps, column, hasPresentMap);
        }
    }
}
