using INLINQ.Orc.Infrastructure;

namespace INLINQ.Orc.Encodings
{
    public class ByteRunLengthEncodingReader
    {
        private readonly byte[] _inputStreamBuffer;

        public ByteRunLengthEncodingReader(ConcatenatingStream inputStream)
        {
            _inputStreamBuffer = inputStream.ReadAll();
        }

        public IEnumerable<byte> Read()
        {
            uint streamIndex = 0;
            while (streamIndex < _inputStreamBuffer.Length)
            {
                int firstByte = _inputStreamBuffer[streamIndex++];
                if (firstByte < 0x80)    //A run
                {
                    int numBytes = firstByte + 3;
                    byte repeatedByte = _inputStreamBuffer[streamIndex++];
                    for (int i = 0; i < numBytes; i++)
                    {
                        yield return repeatedByte;
                    }
                }
                else  //Literals
                {
                    int numBytes = 0x100 - firstByte;
                    for (int i = 0; i < numBytes; i++)
                    {
                        yield return _inputStreamBuffer[streamIndex++];
                    }
                }
            }
        }

        public IEnumerable<byte[]> ReadSmart()
        {
            uint streamIndex = 0;
            while (streamIndex < _inputStreamBuffer.Length)
            {
                int firstByte = _inputStreamBuffer[streamIndex++];
                if (firstByte < 0x80)    //A run
                {
                    int numBytes = firstByte + 3;
                    byte repeatedByte = _inputStreamBuffer[streamIndex++];
                    byte[] result = new byte[numBytes];
                    Array.Fill(result, repeatedByte);
                    yield return result;
                }
                else  //Literals
                {
                    int numBytes = 0x100 - firstByte;
                    byte[] result = new byte[numBytes];
                    Array.Copy(_inputStreamBuffer, streamIndex, result, 0, numBytes);
                    yield return result;
                    streamIndex += (uint)numBytes;
                }
            }
        }
    }
}
