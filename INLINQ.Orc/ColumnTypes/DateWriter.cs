using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;

namespace INLINQ.Orc.ColumnTypes
{
    public class DateWriter : NullableWriter, IColumnWriter
    {
        private static readonly DateTime _unixEpoch = new(1970, 1, 1, 0, 0, 0);        //Here we'll use a Kind=Unspecified DateTime to avoid muddling the subtraction below

        private readonly bool _shouldAlignEncodedValues;

        public DateWriter(bool isNullable, bool shouldAlignEncodedValues, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base(isNullable, bufferFactory, columnId, ColumnEncodingKind.DirectV2)
        {
            _shouldAlignEncodedValues = shouldAlignEncodedValues;
        }

        public int AddBlock(ReadOnlyMemory<DateTime> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            DateWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            long[] datesList = new long[presentValueCount];
            int valueId = 0;
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                int daysSinceEpoch = (int)(valuesSpan[valueId] - _unixEpoch).TotalDays;
                stats.AddValue(daysSinceEpoch);
                datesList[valueId++] = daysSinceEpoch;
            }

            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, datesList, true, _shouldAlignEncodedValues);
            return presentValueCount;
        }


    }
}