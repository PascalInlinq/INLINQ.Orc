using INLINQ.Orc.Compression;

namespace INLINQ.Orc.Infrastructure
{
    public static class OrcCompressedBufferFactoryExtensions
    {
		public static void SerializeAndCompressTo(this OrcCompressedBufferFactory bufferFactory, Stream outputStream, object instance, out long length)
		{
            OrcCompressedBuffer? buffer = bufferFactory.CreateBuffer();
			StaticProtoBuf.Serializer.Serialize(buffer, instance);
			buffer.CopyTo(outputStream);
			length = buffer.Length;
		}
	}
}
