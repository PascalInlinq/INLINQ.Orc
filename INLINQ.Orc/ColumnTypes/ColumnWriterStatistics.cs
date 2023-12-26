namespace INLINQ.Orc.ColumnTypes
{
    public class ColumnWriterStatistics
    {
        public bool HasNull { get; set; }

        public List<List<ulong>> PositionTuples { get; } = new();

        public void AnnotatePosition(long storedBufferOffset, long? decompressedOffset = null, int? rleValuesToConsume = null, int? bitsToConsume = null)
        {
            List<ulong>? newTuple = new() { (ulong)storedBufferOffset };
            if (decompressedOffset.HasValue)
            {
                newTuple.Add((ulong)decompressedOffset.Value);
            }

            if (rleValuesToConsume.HasValue)
            {
                newTuple.Add((ulong)rleValuesToConsume.Value);
            }

            if (bitsToConsume.HasValue)
            {
                newTuple.Add((ulong)bitsToConsume.Value);
            }

            PositionTuples.Add(newTuple);
        }

        public void FillPositionList(List<ulong> positions, Func<int, bool> bufferIndexMustBeIncluded)
        {
            for (int i = 0; i < PositionTuples.Count; i++)
            {
                if (!bufferIndexMustBeIncluded(i))
                {
                    continue;
                }

                positions.AddRange(PositionTuples[i]);
            }
        }
    }
}
