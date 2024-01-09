using ProtoBuf;

namespace INLINQ.Orc.Infrastructure
{
    //TODO: dont use this anymore
    public class StreamSegment : Stream
    {
        private readonly Stream _underlyingStream;
        private readonly long _lengthToExpose;
        private readonly bool _keepUnderlyingStreamOpen;
        private long _bytesRead;

        public static T ReadObject<T>(Stream underlyingStream, long lengthToExpose) where T : new()
        {
            byte[] buffer = new byte[lengthToExpose];
            Span<byte> span = buffer;
            _ = underlyingStream.Read(span);
            ReadOnlyMemory<byte> source = new ReadOnlyMemory<byte>(buffer);
            T obj = Serializer.Deserialize(source, new T());
            return obj;
        }

        public StreamSegment(Stream underlyingStream, long lengthToExpose, bool keepUnderlyingStreamOpen)
        {
            _underlyingStream = underlyingStream;
            _lengthToExpose = lengthToExpose;
            _keepUnderlyingStreamOpen = keepUnderlyingStreamOpen;

            if (!_underlyingStream.CanRead)
            {
                throw new InvalidOperationException($"{nameof(StreamSegment)} requires a readable underlying stream");
            }
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _lengthToExpose;     //What if the underlying Stream has less bytes available than this?
        public override long Position
        {
            get => _bytesRead;
            set => throw new NotImplementedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_bytesRead >= _lengthToExpose)
            {
                return 0;   //No more bytes
            }

            long remainingBytes = _lengthToExpose - _bytesRead;
            int bytesToRead = (int)Math.Min(count, remainingBytes);   //Safe to cast to an int here because count can never exceed Int32.MaxValue
            int bytesRead = _underlyingStream.Read(buffer, offset, bytesToRead);
            _bytesRead += bytesRead;

            return bytesRead;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !_keepUnderlyingStreamOpen)
            {
                _underlyingStream.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
