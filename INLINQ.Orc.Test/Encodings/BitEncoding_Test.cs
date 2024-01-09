using INLINQ.Orc.Encodings;
using INLINQ.Orc.Infrastructure;
using Xunit;

namespace INLINQ.Orc.Test.Encodings
{
    public class BitEncodingTest
    {
        [Fact]
        public void ReadWrite()
        {
            bool[] bools = new bool[] { true, false, false, false, false, false, false, false };
            byte[] bytes = new byte[] { 0xff, 0x80 };
            TestRead(bools, bytes);
            TestWrite(bytes, bools);
        }

        [Fact]
        public void RoundTrip1()
        {
            bool[] bools0 = new bool[] { false };
            bool[] bools1 = new bool[] { true };

            TestRoundTrip(bools0);
            TestRoundTrip(bools1);
        }

        [Fact]
        public void RoundTrip2()
        {
            bool[] bools0 = new bool[] { false, false };
            bool[] bools1 = new bool[] { false, true };
            bool[] bools2 = new bool[] { true, false };
            bool[] bools3 = new bool[] { true, true };

            TestRoundTrip(bools0);
            TestRoundTrip(bools1);
            TestRoundTrip(bools2);
            TestRoundTrip(bools3);
        }

        [Fact]
        public void RoundTripRandom()
        {
            List<bool> bools = new();
            Random random = new(123);
            for (int i = 0; i < 10000; i++)
            {
                bools.Add((random.Next() & 1) == 0);
            }

            TestRoundTrip(bools.ToArray());
        }

        private static void TestRead(bool[] expected, byte[] input)
        {
            MemoryStream stream = new(input);
            BitReader reader = new(new ConcatenatingStream(stream, true));
            bool[] actual = reader.Read().ToArray();
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        private static void TestWrite(byte[] expected, bool[] input)
        {
            MemoryStream stream = new();
            BitWriter writer = new(stream);
            writer.Write(input);
            byte[] actual = stream.ToArray();
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }

        private static void TestRoundTrip(bool[] values, int? expectedEncodeLength = null)
        {
            MemoryStream stream = new();
            BitWriter writer = new(stream);
            writer.Write(values);
            writer.Flush();

            //If we know the encode length, make sure it's correct
            if (expectedEncodeLength.HasValue)
            {
                Assert.Equal(expectedEncodeLength.Value, stream.Length);
            }

            _ = stream.Seek(0, SeekOrigin.Begin);

            BitReader reader = new(new ConcatenatingStream(stream, true));
            bool[] result = reader.Read().ToArray();

            //Make sure all bytes in the written stream were consumed
            Assert.Equal(stream.Length, stream.Position);

            //Check the actual values
            Assert.InRange(result.Length, values.Length, values.Length + 7);        //We may end up with up to 7 extra bits--ignore these
            for (int i = 0; i < values.Length; i++)
            {
                Assert.Equal(values[i], result[i]);
            }
        }
    }
}
