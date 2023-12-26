using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
	public class Metadata
	{
		[ProtoMember(1)]
		public List<StripeStatistics> StripeStats { get; set; } = new List<StripeStatistics>();
    }
}
