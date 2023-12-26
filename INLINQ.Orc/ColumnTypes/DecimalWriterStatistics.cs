using INLINQ.Orc.Statistics;
using System.Globalization;

namespace INLINQ.Orc.ColumnTypes
{
    public class DecimalWriterStatistics : ColumnWriterStatistics, IStatistics
    {
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public decimal? Sum { get; set; } = 0;
        public ulong NumValues { get; set; }
        public void AddValue(decimal? value)
        {
            if (!value.HasValue)
            {
                HasNull = true;
            }
            else
            {
                if (!Min.HasValue || value.Value < Min.Value)
                {
                    Min = value.Value;
                }

                if (!Max.HasValue || value.Value > Max.Value)
                {
                    Max = value.Value;
                }

                Sum = CheckedAdd(Sum, value.Value);
                NumValues++;
            }
        }

        public void FillColumnStatistics(ColumnStatistics columnStatistics)
        {
            if (columnStatistics.DecimalStatistics == null)
            {
                columnStatistics.DecimalStatistics = new DecimalStatistics() { Sum = "0" };     //null means overflow so start with zero
            }

            DecimalStatistics? ds = columnStatistics.DecimalStatistics;

            if (Min.HasValue)
            {
                if (string.IsNullOrEmpty(ds.Minimum) || Min.Value < Decimal.Parse(ds.Minimum, CultureInfo.InvariantCulture.NumberFormat))
                {
                    ds.Minimum = Min.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            if (Max.HasValue)
            {
                if (string.IsNullOrEmpty(ds.Maximum) || Max.Value > Decimal.Parse(ds.Maximum, CultureInfo.InvariantCulture.NumberFormat))
                {
                    ds.Maximum = Max.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
                }
            }

            if (!string.IsNullOrEmpty(ds.Sum))
            {
                decimal? result = CheckedAdd(decimal.Parse(ds.Sum, CultureInfo.InvariantCulture.NumberFormat), Sum);
                if (!result.HasValue)
                {
                    throw new NotImplementedException();
                }
                ds.Sum = result.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);
            }

            columnStatistics.NumberOfValues += NumValues;
            if (HasNull)
            {
                columnStatistics.HasNull = true;
            }
        }

        private static decimal? CheckedAdd(decimal? left, decimal? right)
        {
            if (!left.HasValue)
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
