using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;

namespace INLINQ.Orc.ColumnTypes
{
    public class ByteWriter : NullableWriter, IColumnWriter
    {   
        public ByteWriter(bool isNullable, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base(isNullable, bufferFactory, columnId, ColumnEncodingKind.Direct)
        {   
        }

        public int AddBlock(ReadOnlyMemory<byte> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            ByteRunLengthEncodingWriter? valEncoder = new(_dataBuffer);
            valEncoder.Write(values.Span.Slice(0, presentValueCount), presentValueCount);
            return presentValueCount;
        }

        public int AddBlock(ReadOnlyMemory<sbyte> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);

            byte[] valList = new byte[presentValueCount];
            int valueId = 0;
            var valuesSpan = values.Span;
            for (; valueId < presentValueCount; valueId++)
            {
                sbyte value = valuesSpan[valueId];
                valList[valueId] = (byte)value;
            }
            ByteRunLengthEncodingWriter? valEncoder = new(_dataBuffer);
            valEncoder.Write(valList, valueId);
            return presentValueCount;
        }

    }
}