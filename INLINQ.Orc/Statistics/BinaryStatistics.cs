using ProtoBuf;

namespace INLINQ.Orc.Statistics
{
    [ProtoContract]
    public class BinaryStatistics : IBinaryStatistics
    {
        [ProtoMember(1, DataFormat = DataFormat.ZigZag)] public long? Sum { get; set; }
    }
}
