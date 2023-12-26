using INLINQ.Orc.Statistics;

namespace INLINQ.Orc.ColumnTypes
{
    public class DateWriterStatistics : ColumnWriterStatistics, IStatistics
    {
        public int? Min { get; set; }
        public int? Max { get; set; }
        public ulong NumValues { get; set; }
        public void AddValue(int? date)
        {
            if (!date.HasValue)
            {
                HasNull = true;
            }
            else
            {
                if (!Min.HasValue || date.Value < Min.Value)
                {
                    Min = date.Value;
                }

                if (!Max.HasValue || date.Value > Max.Value)
                {
                    Max = date.Value;
                }

                NumValues++;
            }
        }

        public void FillColumnStatistics(ColumnStatistics columnStatistics)
        {
            if (columnStatistics.DateStatistics == null)
            {
                columnStatistics.DateStatistics = new DateStatistics();
            }

            DateStatistics? ds = columnStatistics.DateStatistics;

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

            columnStatistics.NumberOfValues += NumValues;
            if (HasNull)
            {
                columnStatistics.HasNull = true;
            }
        }
    }
}
