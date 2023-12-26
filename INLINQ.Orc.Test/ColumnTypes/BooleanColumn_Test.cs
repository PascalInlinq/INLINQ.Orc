using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{   
    public class BooleanColumn_Test
    {
        [Fact]
        public void RoundTrip_BooleanColumn()
        {
            RoundTripSingleBool(8, 8);
            RoundTripSingleBool(18, 8);
            RoundTripSingleBool(70000, 8);
            RoundTripSingleBool(9999, 8000);

        }

        [Fact]
        public void RoundTrip_NullableBooleanColumn()
        {
            RoundTripSingleNullableBool(10050, 10000);
            RoundTripSingleNullableBool(9, 8, true);
            RoundTripSingleNullableBool(9,8);
        }

        static void RoundTripSingleBool(int numValues, int rowIndexStride)
        {
            List<SingleBoolPoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                bool b = random.Next() % 2 == 0;
                pocos.Add(new SingleBoolPoco { Bool = b });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer, null, rowIndexStride);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            bool?[] results = BooleanReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Bool, results[i]);
            }
        }

        class SingleBoolPoco
        {
            public bool Bool { get; set; }
        }

        static void RoundTripSingleNullableBool(int numValues, int rowIndexStride, bool? forceValue = null)
        {
            List<SingleNullableBoolPoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                bool b = forceValue.HasValue ? forceValue.Value : (random.Next() % 2 == 0);
                bool isNull = (i == 0);
                pocos.Add(new SingleNullableBoolPoco { Bool = isNull ? null : true });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer, null, rowIndexStride);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            bool?[] results = BooleanReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Bool, results[i]);
            }
        }

        class SingleNullableBoolPoco
        {
            public bool? Bool { get; set; }
        }
    }
}
