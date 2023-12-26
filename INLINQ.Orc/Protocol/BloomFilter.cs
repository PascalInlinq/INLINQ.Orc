using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
    public class BloomFilter
    {
        [ProtoMember(1)] public uint NumHashFunctions { get; }
        [ProtoMember(2, DataFormat = DataFormat.FixedSize)]
        public List<ulong> Bitset { get; } = new List<ulong>();
        [ProtoMember(3)] public byte[] Utf8Bitset { get; set; }

        public BloomFilter(byte[] utf8Bitset)
        {
            Utf8Bitset = utf8Bitset;
        }
    }
}
