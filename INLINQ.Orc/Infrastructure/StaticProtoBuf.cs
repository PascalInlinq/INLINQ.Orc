using ProtoBuf.Meta;

namespace INLINQ.Orc.Infrastructure
{
    public static class StaticProtoBuf
    {
		static StaticProtoBuf()
		{
			Serializer = RuntimeTypeModel.Create();
			Serializer.UseImplicitZeroDefaults = false;
		}

		public static ProtoBuf.Meta.RuntimeTypeModel Serializer { get; }
    }
}
