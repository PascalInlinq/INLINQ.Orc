namespace INLINQ.Orc.Infrastructure
{
    /// <summary>
    /// A read-only Stream that calls out to a provider when data has been exhausted
    /// </summary>
    public class ConcatenatingStream : Stream
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_readingHasEnded)
            {
                return 0;
            }

            if (_currentStream == null)
            {
                _currentStream = _nextStreamProvider();
                if (_currentStream == null)
                {
                    //No additional streams are available. Reading is done.
                    _readingHasEnded = true;
                    return 0;
                }
            }

            //int bytesReadFromCurrentStream = _currentStream.Read(buffer, offset, count);
            //while (bytesReadFromCurrentStream < count)
            //{
            //    if (!_keepStreamsOpen)
            //    {
            //        _currentStream.Dispose();
            //    }

            //    _currentStream = _nextStreamProvider();
            //    if (_currentStream == null)
            //    {
            //        //No additional streams are available. Reading is done.
            //        _readingHasEnded = true;
            //        return 0;
            //    }

            //    bytesReadFromCurrentStream += _currentStream.Read(buffer, offset + bytesReadFromCurrentStream, count - bytesReadFromCurrentStream);
            //}

            //return bytesReadFromCurrentStream;

            int bytesReadFromCurrentStream = _currentStream.Read(buffer, offset, count);
            if (bytesReadFromCurrentStream == 0)
            {
                if (!_keepStreamsOpen)
                {
                    _currentStream.Dispose();
                }

                _currentStream = null;
                return Read(buffer, offset, count);     //Recurse, loading a new stream
            }
            else
            {
                return bytesReadFromCurrentStream;
            }
        }

        //public byte[] ReadNext()
        //{
        //    if (_readingHasEnded)
        //    {
        //        return new byte[0];
        //    }

        //    if (_currentStream == null)
        //    {
        //        _currentStream = _nextStreamProvider();
        //        if (_currentStream == null)
        //        {
        //            //No additional streams are available. Reading is done.
        //            _readingHasEnded = true;
        //            return new byte[0];
        //        }
        //    }

        //    byte[] result = new byte[_currentStream.Length];
        //    int bytesReadFromCurrentStream = _currentStream.Read(result, 0, (int)_currentStream.Length);
        //    if (bytesReadFromCurrentStream == 0)
        //    {
        //        if (!_keepStreamsOpen)
        //        {
        //            _currentStream.Dispose();
        //        }

        //        _currentStream = null;
        //        return ReadNext();     //Recurse, loading a new stream
        //    }
        //    else
        //    {
        //        return result;
        //    }
        //}

        public byte[] ReadAll()
        {
            List<Tuple<byte[], int>> buffers = new List<Tuple<byte[], int>>();
            const int bufferSize = 1024 * 16;
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

        protected override void Dispose(bool disposing)
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
            base.Dispose(disposing);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length
        {
            get
            {
                throw new NotImplementedException();    //We can't determine the final length because we don't have access to future streams
            }
        }
        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
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
    }
}
