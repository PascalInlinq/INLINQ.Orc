using INLINQ.Orc.Encodings;
using System.Numerics;

namespace INLINQ.Orc.Infrastructure
{
    /// <summary>
    /// A read-only Stream that calls out to a provider when data has been exhausted
    /// </summary>
    public class ConcatenatingStream : IDisposable// Stream
    {
        private readonly Func<Stream?> _nextStreamProvider;
        private readonly bool _keepStreamsOpen;
        private Stream? _currentStream;
        private bool _readingHasEnded;

        /// <summary>
        /// Create a Stream that passes data from a series of underlying Streams
        /// </summary>
        /// <param name="nextStreamProvider">A callback for the next underlying Stream.  Return null to indicate the Stream end.</param>
        /// <param name="keepStreamsOpen">Whether to leave underlying streams undisposed, or to dispose each after the last byte is read.</param>
        public ConcatenatingStream(Func<Stream?> nextStreamProvider, bool keepStreamsOpen)
        {
            _nextStreamProvider = nextStreamProvider;
            _keepStreamsOpen = keepStreamsOpen;
        }

        public ConcatenatingStream(Stream stream, bool keepStreamsOpen)
        {
            _nextStreamProvider = () => { var result = stream; stream = null; return result; };
            _keepStreamsOpen = keepStreamsOpen;
        }


        public byte[] ReadAll()
        {
            List<Tuple<byte[], int>> buffers = new List<Tuple<byte[], int>>();
            const int bufferSize = 1024 * 16; //TODO: test impact
            int totalSize = 0;

            while (true)
            {
                if (_readingHasEnded)
                {
                    break;
                }

                if (_currentStream == null)
                {
                    _currentStream = _nextStreamProvider();
                    if (_currentStream == null)
                    {
                        //No additional streams are available. Reading is done.
                        _readingHasEnded = true;
                        break;
                    }
                }

                byte[] buffer = new byte[bufferSize];
                int bytesReadFromCurrentStream = _currentStream.Read(buffer, 0, bufferSize);
                if (bytesReadFromCurrentStream != 0)
                {
                    totalSize += bytesReadFromCurrentStream;
                    buffers.Add(new Tuple<byte[], int>(buffer, bytesReadFromCurrentStream));
                }
                else
                {
                    if (!_keepStreamsOpen)
                    {
                        _currentStream.Dispose();
                    }
                    _currentStream = null;
                }
            }

            byte[] result = new byte[totalSize];
            int offset = 0;
            foreach (var bufferLength in buffers)
            {
                Array.Copy(bufferLength.Item1, 0, result, offset, bufferLength.Item2);
                offset += bufferLength.Item2;
            }

            return result;

        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readingHasEnded = true;
                if (!_keepStreamsOpen && _currentStream != null)
                {
                    _currentStream.Dispose();
                }

                _currentStream = null;
            }
            
        }

        public IEnumerable<BigInteger> ReadAllBigVarInt()
        {
            var stream = ReadAll();
            int streamIndex = 0;
            while(streamIndex < stream.Length)
            {
                BigInteger result = BigInteger.Zero;
                long currentLong = 0;
                long currentByte;
                int bitCount = 0;
                do
                {
                    currentByte = stream[streamIndex++];
                    currentLong |= (currentByte & 0x7f) << (bitCount % 63);
                    bitCount += 7;

                    if (bitCount % 63 == 0)
                    {
                        if (bitCount == 63)
                        {
                            result = new BigInteger(currentLong);
                        }
                        else
                        {
                            result |= new BigInteger(currentLong) << (bitCount - 63);
                        }

                        currentLong = 0;
                    }
                }
                while (currentByte >= 0x80);        //Done when the high bit is not set

                if (currentLong != 0)      //Some bits left to add to result
                {
                    int shift = (bitCount / 63) * 63;
                    result |= new BigInteger(currentLong) << shift;
                }

                //Un zig-zag
                result = (result >> 1) ^ -(result & 1);

                yield return result;
            }

        }

        
    }
}
