using INLINQ.Orc.Compression;
using INLINQ.Orc.Infrastructure;
using System.Diagnostics;

namespace INLINQ.Orc.Stripes
{
    public class StripeStreamReader
    {
        private readonly Stream _inputStream;
        private readonly long _inputStreamOffset;
        private readonly ulong _compressedLength;
        private readonly Protocol.CompressionKind _compressionKind;

        internal StripeStreamReader(Stream inputStream, uint columnId, Protocol.StreamKind streamKind, Protocol.ColumnEncodingKind encodingKind, long inputStreamOffset, ulong compressedLength, Protocol.CompressionKind compressionKind)
        {
            _inputStream = inputStream;
            ColumnId = columnId;
            StreamKind = streamKind;
            ColumnEncodingKind = encodingKind;
            _inputStreamOffset = inputStreamOffset;
            _compressedLength = compressedLength;
            _compressionKind = compressionKind;
        }

        public uint ColumnId { get; }
        public Protocol.StreamKind StreamKind { get; }
        public Protocol.ColumnEncodingKind ColumnEncodingKind { get; }

        public ConcatenatingStream GetDecompressedStream()
        {
            //TODO move from using Streams to using MemoryMapped files or another data type that decouples the Stream Position from the Read call, allowing re-entrancy
            _ = _inputStream.Seek(_inputStreamOffset, System.IO.SeekOrigin.Begin);
            StreamSegment? segment = new(_inputStream, (long)_compressedLength, true);
            var result = OrcCompression.GetDecompressingStream(segment, _compressionKind);
            return result;
        }
    }
}
