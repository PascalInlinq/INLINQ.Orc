using ProtoBuf;
using System.Globalization;

namespace INLINQ.Orc.Statistics
{
    [ProtoContract]
    public class DecimalStatistics : IDecimalStatistics
    {
        [ProtoMember(1)] public string? Minimum { get; set; }
        decimal? IDecimalStatistics.Minimum => string.IsNullOrEmpty(Minimum) ? null : decimal.Parse(Minimum, CultureInfo.InvariantCulture.NumberFormat);
        [ProtoMember(2)] public string? Maximum { get; set; }
        decimal? IDecimalStatistics.Maximum => string.IsNullOrEmpty(Maximum) ? null : decimal.Parse(Maximum, CultureInfo.InvariantCulture.NumberFormat);
        [ProtoMember(3)] public string? Sum { get; set; }
        decimal? IDecimalStatistics.Sum => string.IsNullOrEmpty(Sum) ? null : decimal.Parse(Sum, CultureInfo.InvariantCulture.NumberFormat);
    }
}
