using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class LongColumnTest
    {
        [Fact]
        public void RoundTripLongColumn()
        {
            RoundTripSingleInt(70000);
        }

        [Fact]
        public void RoundTripLongColumnRecord()
        {
            RoundTripSingleIntRecord(70000);
        }


        [Fact]
        public void RoundTripLongColumnNullable()
        {
            RoundTripSingleIntNullable(70000, 1);
            RoundTripSingleIntNullable(70000, 10);
            RoundTripSingleIntNullable(70000, 70001);
        }

        private static void RoundTripSingleInt(int numValues)
        {
            List<SingleIntPoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new SingleIntPoco { Int = random.Next() });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            long?[] results = LongReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Int, results[i]);
            }
        }

        private static void RoundTripSingleIntRecord(int numValues)
        {
            List<SingleIntRecord> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new SingleIntRecord { Int = random.Next() });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            long?[] results = LongReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Int, results[i]);
            }
        }

        private static void RoundTripSingleIntNullable(int numValues, int nullableFraction)
        {
            List<SingleIntPocoNullable> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new SingleIntPocoNullable { Int = i % nullableFraction == nullableFraction - 1
                    ? default(int?)
                    : random.Next() });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            long?[] results = LongReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Int, results[i]);
            }
        }

        private class SingleIntPoco
        {
            public int Int { get; set; }
        }

        private class SingleIntPocoNullable
        {
            public int? Int { get; set; }
        }

        private record class SingleIntRecord
        {
            public int Int { get; set; }
        }
    }
}
