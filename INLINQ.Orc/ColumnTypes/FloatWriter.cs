using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;

namespace INLINQ.Orc.ColumnTypes
{
    public class FloatWriter : NullableWriter, IColumnWriter
    {
        public FloatWriter(bool isNullable, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base(isNullable, bufferFactory, columnId, ColumnEncodingKind.Direct)
        {   
        }

        public int AddBlock(ReadOnlyMemory<float> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            DoubleWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats);

            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                float value = valuesSpan[index];
                stats.AddValue(value);
                _dataBuffer.WriteFloat(value);
            }
            return presentValueCount;
        }
    }
}