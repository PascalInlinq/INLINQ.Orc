using INLINQ.Orc.Compression;
using INLINQ.Orc.FluentSerialization;
using INLINQ.Orc.Infrastructure;
using INLINQ.Orc.Stripes;

namespace INLINQ.Orc
{
    public class OrcWriter<T> : IDisposable
    {
        private readonly Stream _outputStream;
        private readonly OrcCompressedBufferFactory _bufferFactory;
        private readonly StripeWriter<T> _stripeWriter;
        private static readonly List<uint> _version = new() { 0, 12 };
        private static readonly uint _writerVersion = 5;
        private static readonly string _magic = "ORC";

        public OrcWriter(Stream outputStream, WriterConfiguration? configuration= null) 
            : this(outputStream, configuration==null ? new WriterConfiguration(): configuration, null)
        {
        }

        public OrcWriter(Stream outputStream, WriterConfiguration configuration, SerializationConfiguration? serializationConfiguration = null)
        {
            _outputStream = outputStream;
            _bufferFactory = new OrcCompressedBufferFactory(configuration);
            _stripeWriter = new StripeWriter<T>(
                //typeof(T),
                outputStream,
                configuration.EncodingStrategy == EncodingStrategy.Speed,
                configuration.DictionaryKeySizeThreshold,
                configuration.DefaultDecimalPrecision,
                configuration.DefaultDecimalScale,
                _bufferFactory,
                configuration.RowIndexStride,
                configuration.StripeSize,
                serializationConfiguration
                );

            WriteHeader();
        }

        public void AddRow(T row)
        {
            _stripeWriter.AddRows(new[] { row });
        }

        public void AddRow(object row)
        {
            throw new NotImplementedException();
        }

        public void AddRows(IEnumerable<T> rows)
        {
            _stripeWriter.AddRows(rows);
        }

        public void AddUserMetadata(string key, byte[] value)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _stripeWriter.RowAddingCompleted();
            ulong numberOfRows =WriteTail();
            GC.SuppressFinalize(this);
        }

        public ulong DisposeAndGetRowCount()
        {
            _stripeWriter.RowAddingCompleted();
            ulong numberOfRows = WriteTail();
            GC.SuppressFinalize(this);
            return numberOfRows; ;
        }

        private ulong WriteTail()
        {
            Protocol.Metadata? metadata = _stripeWriter.GetMetadata();
            Protocol.Footer? footer = _stripeWriter.GetFooter();
            footer.HeaderLength = (ulong)_magic.Length;

            _bufferFactory.SerializeAndCompressTo(_outputStream, metadata, out long metadataLength);
            _bufferFactory.SerializeAndCompressTo(_outputStream, footer, out long footerLength);

            Protocol.PostScript? postScript = GetPostscript(_bufferFactory, _stripeWriter, (ulong)footerLength, (ulong)metadataLength);
            MemoryStream? postScriptStream = new();
            _ = StaticProtoBuf.Serializer.Serialize(postScriptStream, postScript);
            _ = postScriptStream.Seek(0, SeekOrigin.Begin);
            postScriptStream.CopyTo(_outputStream);

            if (postScriptStream.Length > 255)
            {
                throw new InvalidDataException("Invalid Postscript length");
            }

            _outputStream.WriteByte((byte)postScriptStream.Length);

            return footer.NumberOfRows;
        }

        private static Protocol.PostScript GetPostscript(OrcCompressedBufferFactory bufferFactory, StripeWriter<T> stripeWriter,
            ulong footerLength, ulong metadataLength)
        {
            return new Protocol.PostScript
            {
                FooterLength = footerLength,
                Compression = bufferFactory.CompressionKind,
                CompressionBlockSize = (ulong)bufferFactory.CompressionBlockSize,
                Version = _version,
                MetadataLength = metadataLength,
                WriterVersion = _writerVersion,
                Magic = _magic
            };
        }

        private void WriteHeader()
        {
            byte[]? magic = new byte[] { (byte)'O', (byte)'R', (byte)'C' };
            _outputStream.Write(magic, 0, magic.Length);
        }
    }

}
