using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class DoubleColumnTest
    {
        [Fact]
        public void RoundTripDoubleColumn()
        {
            RoundTripSingleValue(70000);
        }

        [Fact]
        public void RoundTripDoubleColumnNullable()
        {
            RoundTripSingleNullableValue(70000);
        }

        private static void RoundTripSingleValue(int numValues)
        {
            List<SingleValuePoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new SingleValuePoco { Value = random.Next() / (double)random.Next() });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            //DoubleReader reader = new(stripeStreams, 1);
            double?[] results = DoubleReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleNullableValue(int numValues)
        {
            List<SingleValueNullablePoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                var val = new SingleValueNullablePoco { Value = (i == 0) ? null : random.Next() / (double)random.Next() };
                pocos.Add(val);
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            //DoubleReader reader = new(stripeStreams, 1);
            double?[] results = DoubleReader.Read(stripeStreams, 1).ToArray();

            Assert.Null(results[0]);
            for (int i = 1; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private class SingleValuePoco
        {
            public double Value { get; set; }
        }

        private class SingleValueNullablePoco
        {
            public double? Value { get; set; }
        }
    }
}
