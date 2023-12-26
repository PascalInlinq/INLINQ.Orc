using ProtoBuf;

namespace INLINQ.Orc.Protocol
{
    [ProtoContract]
    public class UserMetadataItem
    {
        [ProtoMember(1)] public string Name { get; set; }
        [ProtoMember(2)] public byte[] Value { get; set; }

        public UserMetadataItem(string name, byte[] value)
        {
            Name = name;
            Value = value;
        }
    }
}
