using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Compression;

namespace INLINQ.Orc.Encodings
{
    public class BitWriter
    {
        private readonly ByteRunLengthEncodingWriter _byteWriter;
        private byte[] _bitWriterBuffer = new byte[0];
        private int lastByteBitCount = 0; //0..7

        public BitWriter(Stream outputStream)
        {
            _byteWriter = new ByteRunLengthEncodingWriter(outputStream);
        }

        public void Flush()
        {
            if(_bitWriterBuffer.Length > 0)
            {
                _byteWriter.Write(_bitWriterBuffer, _bitWriterBuffer.Length);
            }
        }

        public void Write(ReadOnlySpan<byte> bits)
        {
            _byteWriter.Write(bits, bits.Length);
        }

        public void Write(ReadOnlySpan<bool> values)
        {
            //flush buffer if necessary:
            byte b = 0;
            int length = _bitWriterBuffer.Length;
            if (length > 0)
            {
                if (lastByteBitCount > 0)
                {
                    length--;
                    b = _bitWriterBuffer[length];
                }
                if (length > 0)
                {
                    _byteWriter.Write(_bitWriterBuffer, length);
                }
            }

            //add to new buffer:
            byte currentBit = (byte)(0x80 >> lastByteBitCount);
            int numBytes = (values.Length + lastByteBitCount + 7) / 8;
            _bitWriterBuffer = new byte[numBytes];
            int byteIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i])
                {
                    b += currentBit;
                }

                if ((currentBit >>= 1) == 0)
                {
                    _bitWriterBuffer[byteIndex++] = b;
                    b = 0;
                    currentBit = 0x80;
                }
            }

            if (currentBit != 0x80)
            {
                _bitWriterBuffer[byteIndex] = b;
            }
            lastByteBitCount = (lastByteBitCount + values.Length) % 8;
            if(_bitWriterBuffer.Length > 0 && lastByteBitCount == 0)
            {
                //flush here already:
                _byteWriter.Write(_bitWriterBuffer, _bitWriterBuffer.Length);
                _bitWriterBuffer = Array.Empty<byte>();
            }

        }
    }
}
