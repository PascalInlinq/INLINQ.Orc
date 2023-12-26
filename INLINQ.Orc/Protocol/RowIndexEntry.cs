using INLINQ.Orc.Statistics;
using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
    public class RowIndexEntry
    {
        [ProtoMember(1, IsPacked = true)]
        public List<ulong> Positions { get; } = new List<ulong>();
        [ProtoMember(2)]
        public ColumnStatistics Statistics { get; } = new ColumnStatistics();
    }
}
