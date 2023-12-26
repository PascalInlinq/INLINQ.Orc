using INLINQ.Orc.Compression;
using INLINQ.Orc.Protocol;
using System.Diagnostics;

namespace INLINQ.Orc.ColumnTypes
{
    public class StructWriter : IColumnWriter
    {
        //Assume all root values are present
        //public static long WriteBlockMilliseconds { get; private set; }
        public StructWriter(OrcCompressedBufferFactory bufferFactory, uint columnId)
        {
            ColumnId = columnId;
        }

        public List<IStatistics> Statistics { get; } = new List<IStatistics>();
        public long CompressedLength => 0;
        public uint ColumnId { get; }
        public OrcCompressedBuffer[] Buffers => Array.Empty<OrcCompressedBuffer>();
        public ColumnEncodingKind ColumnEncoding => ColumnEncodingKind.Direct;

        public void FlushBuffers()
        {
        }

        public void Reset()
        {
            Statistics.Clear();
        }

        public int AddBlock(IList<object> values)
        {
            BooleanWriterStatistics? stats = new();
            Statistics.Add(stats);
            foreach (OrcCompressedBuffer? buffer in Buffers)
            {
                buffer.AnnotatePosition(stats, rleValuesToConsume: 0, bitsToConsume: 0);
            }

            stats.NumValues += (uint)values.Count;
            return (int)values.Count;
        }

        public int AddBlock(/* object[] values,*/  int valueCount)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            BooleanWriterStatistics? stats = new();
            Statistics.Add(stats);
            foreach (OrcCompressedBuffer? buffer in Buffers)
            {
                buffer.AnnotatePosition(stats, rleValuesToConsume: 0, bitsToConsume: 0);
            }

            stats.NumValues += (uint)valueCount;
            //WriteBlockMilliseconds += sw.ElapsedMilliseconds;
            return valueCount;
        }

        public int AddBlockCast(object uncastValues, int valueCount)
        {
            throw new NotImplementedException();
        }
    }
}