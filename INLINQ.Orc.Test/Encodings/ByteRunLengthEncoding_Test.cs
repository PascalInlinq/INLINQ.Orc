using INLINQ.Orc.Encodings;
using Xunit;

namespace INLINQ.Orc.Test.Encodings
{
    public class ByteRunLengthEncodingTest
    {
        [Fact]
        public void ReadWriteRepeated()
        {
            byte[] values = new byte[100];
            byte[] encoded = new byte[] { 0x61, 0x00 };
            TestRead(values, encoded);
            TestWrite(encoded, values);
        }

        [Fact]
        public void ReadWriteLiterals()
        {
            byte[] values = new byte[] { 0x44, 0x45 };
            byte[] encoded = new byte[] { 0xfe, 0x44, 0x45 };
            TestRead(values, encoded);
            TestWrite(encoded, values);
        }

        [Fact]
        public void ReadWriteSetOfRepeats()
        {
            byte[] values = new byte[] { 0x1, 0x1, 0x1, 0x2, 0x2, 0x2, 0x3, 0x3, 0x3 };
            byte[] encoded = new byte[] { 0x0, 0x1, 0x0, 0x2, 0x0, 0x3 };
            TestRead(values, encoded);
            TestWrite(encoded, values);
        }

        [Fact]
        public void ReadWriteRepeatsLiteralRepeats()
        {
            byte[] values = new byte[] { 0x1, 0x1, 0x1, 0x2, 0x3, 0x4, 0x5, 0x5, 0x5 };
            byte[] encoded = new byte[] { 0x0, 0x1, 0xfd, 0x2, 0x3, 0x4, 0x0, 0x5 };
            TestRead(values, encoded);
            TestWrite(encoded, values);
        }

        [Fact]
        public void RoundTripInterspersedRepeats()
        {
            byte[] values = new byte[] { 0x1, 0x2, 0x2, 0x2, 0x3, 0x3, 0x4, 0x4, 0x4, 0x5 };
            TestRoundTrip(values, 2 + 2 + 3 + 2 + 2);
        }

        [Fact]
        public void RoundTrip130Repeats()
        {
            List<byte> values = new();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 130; j++)
                {
                    values.Add(0x1);
                }
            }

            TestRoundTrip(values.ToArray(), 5 * 2);
        }

        [Fact]
        public void RoundTrip128Literals()
        {
            List<byte> values = new();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    values.Add((byte)j);
                }
            }

            TestRoundTrip(values.ToArray(), 5 * (128 + 1));
        }

        private static void TestRead(byte[] expected, byte[] input)
        {
            MemoryStream stream = new(input);
            ByteRunLengthEncodingReader reader = new(stream);
            byte[] actual = reader.Read().ToArray();
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        private static void TestWrite(byte[] expected, byte[] input)
        {
            MemoryStream stream = new();
            ByteRunLengthEncodingWriter writer = new(stream);
            writer.Write(input);
            byte[] actual = stream.ToArray();
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        private static void TestRoundTrip(byte[] values, int? expectedEncodeLength = null)
        {
            MemoryStream stream = new();
            ByteRunLengthEncodingWriter writer = new(stream);
            writer.Write(values);

            //If we know the encode length, make sure it's correct
            if (expectedEncodeLength.HasValue)
            {
                Assert.Equal(expectedEncodeLength.Value, stream.Length);
            }

            _ = stream.Seek(0, SeekOrigin.Begin);

            ByteRunLengthEncodingReader reader = new(stream);
            byte[] result = reader.Read().ToArray();

            //Make sure all bytes in the written stream were consumed
            Assert.Equal(stream.Length, stream.Position);

            //Check the actual values
            Assert.Equal(values.Length, result.Length);
            for (int i = 0; i < values.Length; i++)
            {
                Assert.Equal(values[i], result[i]);
            }
        }
    }
}
