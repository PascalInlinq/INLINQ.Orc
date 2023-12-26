using INLINQ.Orc.Statistics;

namespace INLINQ.Orc.ColumnTypes
{
    public class LongWriterStatistics : ColumnWriterStatistics, IStatistics
    {
        public long? Min { get; set; }
        public long? Max { get; set; }
        public long? Sum { get; set; } = 0;
        public ulong NumValues { get; set; }
        
        public void AddValue(long value)
        {
            if (!Min.HasValue || value < Min.Value)
            {
                Min = value;
            }

            if (!Max.HasValue || value > Max.Value)
            {
                Max = value;
            }

            Sum = CheckedAdd(Sum, value);
            NumValues++;
        }

        public void FillColumnStatistics(ColumnStatistics columnStatistics)
        {
            if (columnStatistics.IntStatistics == null)
            {
                columnStatistics.IntStatistics = new IntegerStatistics { Sum = 0 };
            }

            IntegerStatistics? ds = columnStatistics.IntStatistics;

            if (Min.HasValue)
            {
                if (!ds.Minimum.HasValue || Min.Value < ds.Minimum.Value)
                {
                    ds.Minimum = Min.Value;
                }
            }

            if (Max.HasValue)
            {
                if (!ds.Maximum.HasValue || Max.Value > ds.Maximum.Value)
                {
                    ds.Maximum = Max.Value;
                }
            }

            ds.Sum = CheckedAdd(ds.Sum, Sum);

            columnStatistics.NumberOfValues += NumValues;
            if (HasNull)
            {
                columnStatistics.HasNull = true;
            }
        }

        private static long? CheckedAdd(long? left, long? right)
        {
            if (!left.HasValue || !right.HasValue)
            {
                return null;
            }

            try
            {
                checked
                {
                    return left.Value + right;
                }
            }
            catch (OverflowException)
            {
                return null;
            }
        }
    }
}
