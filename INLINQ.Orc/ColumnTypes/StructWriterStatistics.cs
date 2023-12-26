using INLINQ.Orc.Statistics;

namespace INLINQ.Orc.ColumnTypes
{
    public class StructWriterStatistics : ColumnWriterStatistics, IStatistics
	{
		public ulong NumValues { get; set; }

		public void FillColumnStatistics(ColumnStatistics columnStatistics)
		{
			columnStatistics.NumberOfValues += NumValues;
		}
	}
}
