using INLINQ.Orc.Infrastructure;

namespace INLINQ.Orc.Encodings
{
    public class ByteRunLengthEncodingWriter
    {
        private readonly Stream _outputStream;

        public ByteRunLengthEncodingWriter(Stream outputStream)
        {
            _outputStream = outputStream;
        }

        public void Write(byte[] values)
        {
            Write(values, values.Length);
        }

        public void Write(ReadOnlySpan<byte> values, int valueCount)
        {
            int position = 0;
            while (position < valueCount)
            {
                int startPosition = position;
                ReadOnlySpan<byte> window = values.Slice(position, values.Length - position);
                //Check for repeats
                int repeatingValueCount = FindRepeatedValues(window, out byte repeatingValue);
                if (repeatingValueCount >= 3)
                {
                    EncodeRepeat(repeatingValueCount, repeatingValue);
                    position += repeatingValueCount;
                    continue;   //Search again for new repeating values
                }

                //Check for future repeats
                int repeatLocation = FindNonRepeatingValues(window);
                ReadOnlySpan<byte> literalWindow = values.Slice(startPosition, repeatLocation);
                EncodeLiterals(literalWindow);
                position += repeatLocation;
            }
        }

        private void EncodeRepeat(int repeatingValueCount, byte repeatingValue)
        {
            byte byte1 = (byte)(repeatingValueCount - 3);

            _outputStream.WriteByte(byte1);
            _outputStream.WriteByte(repeatingValue);
        }

        private void EncodeLiterals(ReadOnlySpan<byte> values)
        {
            byte byte1 = (byte)-values.Length;

            _outputStream.WriteByte(byte1);
            foreach (byte curByte in values)
            {
                _outputStream.WriteByte(curByte);
            }
        }

        private static int FindNonRepeatingValues(ReadOnlySpan<byte> values)
        {
            if (values.Length < 3)
            {
                return values.Length;
            }

            int result = 0;
            while (result < values.Length - 2 && result < 128 - 2)
            {
                byte val0 = values[result + 0];
                byte val1 = values[result + 1];
                byte val2 = values[result + 2];
                if (val0 == val1 && val0 == val2)
                {
                    return result;       //End of the non-repeating section
                }

                result++;
            }

            return result + 2;          //No repeats found including the last two values
        }

        private static int FindRepeatedValues(ReadOnlySpan<byte> values, out byte repeatingValue)
        {
            int result = 0;
            repeatingValue = values[0];
            while (result < values.Length && result < 127 + 3)
            {
                if (values[result] != repeatingValue)
                {
                    break;
                }

                result++;
            }

            return result;
        }
    }
}
