
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class BinaryColumnTest
    {
        [Fact]
        public void RoundTripBinaryColumn()
        {
            RoundTripSingleBinary(70000);
        }

        [Fact]
        public void RoundTripBinaryColumnNullable()
        {
            RoundTripSingleBinaryNullable(70000);
        }

        private static void RoundTripSingleBinary(int numValues)
        {
            List<SingleBinaryPoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                int numBytes = i % 100;
                byte[] bytes = new byte[numBytes];
                random.NextBytes(bytes);
                pocos.Add(new SingleBinaryPoco { Bytes = bytes });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            byte[][] results = INLINQ.Orc.ColumnTypes.BinaryReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.True(pocos[i].Bytes.SequenceEqual(results[i]));
            }
        }

        private static void RoundTripSingleBinaryNullable(int numValues)
        {
            List<SingleBinaryPocoNullable> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                int numBytes = i % 100;
                byte[] bytes = new byte[numBytes];
                random.NextBytes(bytes);
                pocos.Add(new SingleBinaryPocoNullable { Bytes = i==0 ? null : bytes });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            byte[][] results = INLINQ.Orc.ColumnTypes.BinaryReader.Read(stripeStreams, 1).ToArray();

            Assert.Null(results[0]);
            for (int i = 1; i < numValues; i++)
            {
                Assert.True(pocos[i].Bytes.SequenceEqual(results[i]));
            }
        }

        private class SingleBinaryPoco
        {
            public byte[] Bytes { get; set; }
        }

        private class SingleBinaryPocoNullable
        {
            public byte[]? Bytes { get; set; }
        }
    }
}
