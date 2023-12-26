using INLINQ.Orc.Statistics;
using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
    public class StripeStatistics
    {
		[ProtoMember(1)]
		public List<ColumnStatistics> ColStats { get; } = new List<ColumnStatistics>();
    }
}
