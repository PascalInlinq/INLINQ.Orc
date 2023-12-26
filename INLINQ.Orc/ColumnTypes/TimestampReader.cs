using INLINQ.Orc.Stripes;

namespace INLINQ.Orc.ColumnTypes
{
    public static class TimestampReader // : ColumnReader
    {
        private static readonly DateTime _orcEpoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long EncodedNanosToTicks(long encodedNanos)
        {
            int scale = (int)(encodedNanos & 0x7);
            long nanos = encodedNanos >> 3;

            if (scale == 0)
            {
                return nanos;
            }

            while (scale-- >= 0)
            {
                nanos *= 10;
            }

            return nanos / 100;     //100 nanoseconds per tick
        }

        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, DateTime[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            long[] data = new long[stripeStreams.NumRows];
            long[] secondary = new long[stripeStreams.NumRows];
            ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Data, true, data);
            ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Secondary, false, secondary);
            int valueId = 0;
            while (valueId < column.Length)
            {
                long seconds = data[valueId];
                long nanosecondTicks = EncodedNanosToTicks(secondary[valueId]);
                long totalTicks = seconds * TimeSpan.TicksPerSecond + (seconds >= 0 ? nanosecondTicks : -nanosecondTicks);
                column[valueId++] = _orcEpoch.AddTicks(totalTicks);
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
