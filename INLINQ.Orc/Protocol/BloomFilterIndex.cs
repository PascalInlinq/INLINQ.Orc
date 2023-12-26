using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
    public class BloomFilterIndex
    {
		[ProtoMember(1)]
		public List<BloomFilter> BloomFilter { get; } = new List<Protocol.BloomFilter>();
    }
}
