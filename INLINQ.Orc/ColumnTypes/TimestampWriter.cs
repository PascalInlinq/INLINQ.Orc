using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;

namespace INLINQ.Orc.ColumnTypes
{
    public class TimestampWriter : NullableWriter, IColumnWriter
    {
        private readonly static DateTime _orcEpoch = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly static DateTime _unixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly bool _shouldAlignEncodedValues;
        private readonly OrcCompressedBuffer _secondaryBuffer;

        public TimestampWriter(bool isNullable, bool shouldAlignEncodedValues, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base(isNullable, bufferFactory, columnId, ColumnEncodingKind.DirectV2, StreamKind.Secondary)
        {
             _shouldAlignEncodedValues = shouldAlignEncodedValues;
            _secondaryBuffer = base.Buffers[base.Buffers.Length-1];
        }


        private static long GetValues(DateTime dateTime, out long millisecondsSinceUnixEpoch, out long fraction)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
            {
                throw new NotSupportedException("Only UTC DateTimes are supported in Timestamp columns");
            }

            long ticks = (dateTime - _orcEpoch).Ticks;
            long seconds = ticks / TimeSpan.TicksPerSecond;
            millisecondsSinceUnixEpoch = (dateTime - _unixEpoch).Ticks / TimeSpan.TicksPerMillisecond;
            int remainderTicks = (int)(ticks - (seconds * TimeSpan.TicksPerSecond));
            int nanoseconds = Math.Abs(remainderTicks) * 100;
            byte scale = RemoveZeros(nanoseconds, out int scaledNanoseconds);
            fraction = (scaledNanoseconds << 3) | scale;
            return seconds;
        }

        private static byte RemoveZeros(int nanoseconds, out int scaledNanoseconds)
        {
            if (nanoseconds >= 1 * 1000 * 1000 * 1000)
            {
                throw new ArgumentException("Nanoseconds must be less than a single second");
            }

            scaledNanoseconds = nanoseconds / (100 * 1000 * 1000);
            if (scaledNanoseconds * 100 * 1000 * 1000 == nanoseconds)
            {
                return 7;
            }

            scaledNanoseconds = nanoseconds / (10 * 1000 * 1000);
            if (scaledNanoseconds * 10 * 1000 * 1000 == nanoseconds)
            {
                return 6;
            }

            scaledNanoseconds = nanoseconds / (1 * 1000 * 1000);
            if (scaledNanoseconds * 1 * 1000 * 1000 == nanoseconds)
            {
                return 5;
            }

            scaledNanoseconds = nanoseconds / (100 * 1000);
            if (scaledNanoseconds * 100 * 1000 == nanoseconds)
            {
                return 4;
            }

            scaledNanoseconds = nanoseconds / (10 * 1000);
            if (scaledNanoseconds * 10 * 1000 == nanoseconds)
            {
                return 3;
            }

            scaledNanoseconds = nanoseconds / (1 * 1000);
            if (scaledNanoseconds * 1 * 1000 == nanoseconds)
            {
                return 2;
            }

            scaledNanoseconds = nanoseconds / (100);
            if (scaledNanoseconds * 100 == nanoseconds)
            {
                return 1;
            }

            scaledNanoseconds = nanoseconds;
            return 0;
        }

        public int AddBlock(ReadOnlyMemory<DateTime> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            TimestampWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);

            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            _secondaryBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);

            long[] secondsList = new long[presentValueCount];
            long[] fractionsList = new long[presentValueCount];
            int secondsId = 0;
            int fractionsId = 0;

            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                DateTime value = valuesSpan[index];
                long seconds = GetValues(value, out long millisecondsSinceUnixEpoch, out long fraction);
                stats.AddValue(millisecondsSinceUnixEpoch);
                secondsList[secondsId++] = seconds;
                fractionsList[fractionsId++] = fraction;
            }

            //IntegerRunLengthEncodingV2Writer? secondsEncoder = new(_dataBuffer);
            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, new ReadOnlySpan<long>(secondsList, 0, secondsId), true, _shouldAlignEncodedValues);

            //IntegerRunLengthEncodingV2Writer? fractionsEncoder = new(_secondaryBuffer);
            IntegerRunLengthEncodingV2Writer.Write(_secondaryBuffer, new ReadOnlySpan<long>(fractionsList, 0, fractionsId), false, _shouldAlignEncodedValues);
            return presentValueCount;
        }

    }
}