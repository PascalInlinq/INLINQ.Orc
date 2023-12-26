using INLINQ.Orc.Compression;
using INLINQ.Orc.Infrastructure;

namespace INLINQ.Orc.ColumnTypes
{
    public static class ColumnExtensions
    {
        public static void WriteToBuffer(this IEnumerable<IStatistics> statistics, Stream outputStream, Func<int, bool> bufferIndexMustBeIncluded)
        {
            Protocol.RowIndex? indexes = new();
            foreach (IStatistics? stats in statistics)
            {
                Protocol.RowIndexEntry? indexEntry = new();
                stats.FillPositionList(indexEntry.Positions, bufferIndexMustBeIncluded);
                stats.FillColumnStatistics(indexEntry.Statistics);
                indexes.Entry.Add(indexEntry);
            }

            _ = StaticProtoBuf.Serializer.Serialize(outputStream, indexes);
        }

        public static void AddDataStream(this Protocol.StripeFooter footer, uint columnId, OrcCompressedBuffer buffer)
        {
            Protocol.Stream? stream = new()
            {
                Column = columnId,
                Kind = buffer.StreamKind,
                Length = (ulong)buffer.Length
            };
            footer.Streams.Add(stream);
        }

        public static void AddColumn(this Protocol.StripeFooter footer, Protocol.ColumnEncodingKind columnEncodingKind, uint dictionarySize = 0)
        {
            Protocol.ColumnEncoding? columnEncoding = new()
            {
                Kind = columnEncodingKind,
                DictionarySize = dictionarySize
            };
            footer.Columns.Add(columnEncoding);
        }
    }
}
