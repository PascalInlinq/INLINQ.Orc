using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;

namespace INLINQ.Orc.ColumnTypes
{
    public class BooleanWriter : NullableWriter, IColumnWriter
    {
        private readonly BitWriter _bitWriter;
        
        public BooleanWriter(bool isNullable, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base (isNullable, bufferFactory, columnId, ColumnEncodingKind.Direct)
        {
            _bitWriter = new BitWriter(_dataBuffer);
        }

        public new void FlushBuffers()
        {
            _bitWriter.Flush();
            base.FlushBuffers();
            
            
        }

        public int AddBlock(ReadOnlyMemory<bool> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            BooleanWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0, bitsToConsume: 0);

            bool[] valList = new bool[presentValueCount];
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                bool value = valuesSpan[index];
                valList[index] = value; //because cast is necessary
                stats.AddValue(value);
            }

            _bitWriter.Write(valList);
            return presentValueCount;
        }
    }
}
