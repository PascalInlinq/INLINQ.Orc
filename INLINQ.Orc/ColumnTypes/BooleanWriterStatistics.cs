using INLINQ.Orc.Statistics;

namespace INLINQ.Orc.ColumnTypes
{
    public class BooleanWriterStatistics : ColumnWriterStatistics, IStatistics
    {
        public ulong FalseCount { get; set; }
        public ulong TrueCount { get; set; }
        public ulong NumValues { get; set; }
        public void AddNull()
        {
            HasNull = true;
        }

        public void AddValue(bool value)
        {
            if (value)
            {
                TrueCount++;
            }
            else
            {
                FalseCount++;
            }

            NumValues++;
        }

        public void FillColumnStatistics(ColumnStatistics columnStatistics)
        {
            if (columnStatistics.BucketStatistics == null)
            {
                columnStatistics.BucketStatistics = new BucketStatistics
                {
                    Count = new List<ulong>
                    {
                        0,
                        0
                    }
                };
            }

            columnStatistics.BucketStatistics.Count[0] += FalseCount;
            columnStatistics.BucketStatistics.Count[1] += TrueCount;
            columnStatistics.NumberOfValues += NumValues;
            if (HasNull)
            {
                columnStatistics.HasNull = true;
            }
        }
    }
}
