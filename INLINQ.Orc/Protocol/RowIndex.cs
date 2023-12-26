using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
    public class RowIndex
    {
		[ProtoMember(1)]
		public List<RowIndexEntry> Entry { get; } = new List<RowIndexEntry>();
    }
}
