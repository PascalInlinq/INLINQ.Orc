using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class FloatColumnTest
    {
        [Fact]
        public void RoundTripDoubleColumn()
        {
            RoundTripSingleValue(70000);
        }

        private static void RoundTripSingleValue(int numValues)
        {
            List<SingleValuePoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new SingleValuePoco { Value = random.Next() / (float)random.Next() });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            float?[] results = FloatReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private class SingleValuePoco
        {
            public float Value { get; set; }
        }
    }
}
