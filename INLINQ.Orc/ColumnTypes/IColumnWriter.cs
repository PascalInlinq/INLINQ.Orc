using INLINQ.Orc.Compression;

namespace INLINQ.Orc.ColumnTypes
{
    public interface IColumnWriter
    {
        List<IStatistics> Statistics { get; }
        Protocol.ColumnEncodingKind ColumnEncoding { get; }
        OrcCompressedBuffer[] Buffers { get; }
        long CompressedLength { get; }
        uint ColumnId { get; }
        void FlushBuffers();
        void Reset();
    }
}
