using INLINQ.Orc.Statistics;

namespace INLINQ.Orc.ColumnTypes
{
    public class StringWriterStatistics : ColumnWriterStatistics, IStatistics
    {
        public string? Min { get; set; }
        public string? Max { get; set; }
        public long Sum { get; set; }
        public ulong NumValues { get; set; }
        public void AddValue(string? value)
        {
            if (value == null)
            {
                HasNull = true;
            }
            else
            {
                if (Min == null || string.Compare(value, Min, StringComparison.Ordinal) < 0)
                {
                    Min = value;
                }

                if (Max == null || string.Compare(value, Max, StringComparison.Ordinal) > 0)
                {
                    Max = value;
                }

                Sum += value.Length;
                NumValues++;
            }
        }

        public void FillColumnStatistics(ColumnStatistics columnStatistics)
        {
            if (columnStatistics.StringStatistics == null)
                {
                columnStatistics.StringStatistics = new StringStatistics { Sum = 0 };
            }

            StringStatistics? ds = columnStatistics.StringStatistics;

            if (Min != null)
            {
                if (ds.Minimum == null || string.Compare(Min, ds.Minimum, StringComparison.Ordinal) < 0)
                {
                    ds.Minimum = Min;
                }
            }

            if (Max != null)
            {
                if (ds.Maximum == null || string.Compare(Max, ds.Maximum, StringComparison.Ordinal) > 0)
                {
                    ds.Maximum = Max;
                }
            }

            columnStatistics.StringStatistics.Sum += Sum;

            columnStatistics.NumberOfValues += NumValues;
            if (HasNull)
            {
                columnStatistics.HasNull = true;
            }
        }
    }
}
