using INLINQ.Orc.Compression;
using INLINQ.Orc.Infrastructure;
using ProtoBuf;

namespace INLINQ.Orc.Stripes
{
    public class StripeReader
    {
        private readonly Stream _inputStream;
        private readonly ulong _indexOffset;
        private readonly ulong _indexLength;
        private readonly ulong _dataOffset;
        private readonly ulong _dataLength;
        private readonly ulong _footerOffset;
        private readonly ulong _footerLength;
        private readonly Protocol.CompressionKind _compressionKind;

        internal StripeReader(Stream inputStream, ulong indexOffset, ulong indexLength, ulong dataOffset, ulong dataLength, ulong footerOffset, ulong footerLength, ulong numRows, Protocol.CompressionKind compressionKind)
        {
            _inputStream = inputStream;
            _indexOffset = indexOffset;
            _indexLength = indexLength;
            _dataOffset = dataOffset;
            _dataLength = dataLength;
            _footerOffset = footerOffset;
            _footerLength = footerLength;
            NumRows = numRows;
            _compressionKind = compressionKind;
        }

        public ulong NumRows { get; }

        private Protocol.StripeFooter GetStripeFooter()
        {
            _ = _inputStream.Seek((long)_footerOffset, SeekOrigin.Begin);
            StreamSegment? segment = new(_inputStream, (long)_footerLength, true);
            Stream stream = OrcCompression.GetDecompressingStream(segment, _compressionKind);
            return Serializer.Deserialize<Protocol.StripeFooter>(stream);
        }


        public StripeStreamReaderCollection GetStripeStreamCollection()
        {
            Protocol.StripeFooter? footer = GetStripeFooter();
            return new StripeStreamReaderCollection(_inputStream, footer, (long)_indexOffset, _compressionKind, NumRows);
        }
    }
}
