using INLINQ.Orc.Statistics;

namespace INLINQ.Orc.ColumnTypes
{
    public class BinaryWriterStatistics : ColumnWriterStatistics, IStatistics
    {
        public long? Sum { get; set; } = 0;
        public ulong NumValues { get; set; }

        public void AddValue(byte[]? data)
        {
            if (data == null)
            {
                HasNull = true;
            }
            else
            {
                Sum = CheckedAdd(Sum, data.Length);
                NumValues++;
            }
        }

        public void FillColumnStatistics(ColumnStatistics columnStatistics)
        {
            if (columnStatistics.BinaryStatistics == null)
            {
                columnStatistics.BinaryStatistics = new BinaryStatistics { Sum = 0 };
            }

            BinaryStatistics? ds = columnStatistics.BinaryStatistics;

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
                    return left.Value + right.Value;
                }
            }
            catch (OverflowException)
            {
                return null;
            }
        }
    }
}
