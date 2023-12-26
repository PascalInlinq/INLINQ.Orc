using INLINQ.Orc;
using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.FluentSerialization;
using INLINQ.Orc.Protocol;
using INLINQ.Orc.Stripes;
using System.Globalization;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class DecimalColumnTest
    {
        [Fact]
        public void RoundTripDecimalColumn()
        {
            RoundTripSingleValue(70000);
        }

        [Fact]
        public void RoundTripDecimalColumnNone()
        {
            RoundTripSingleValue(0);
        }


        [Fact]
        public void RoundTripDecimalAllNulls()
        {
            RoundTripNulls(70000);
        }

        [Fact]
        public void RoundTripCustom()
        {
            int numValues = 6000;
            int start = 0;
            List<NullableSingleValuePoco> pocos = new();
            for (int i = start; i < numValues; i++)
            {
                decimal? expected;
                if (i < 2000)
                {
                    int decimalPortion = 5 + i;
                    int wholePortion = -1000 + i;
                    expected = decimal.Parse($"{wholePortion}.{decimalPortion}", CultureInfo.InvariantCulture);
                }
                else if (i < 4000)
                {
                    expected = null;
                }
                else
                {
                    int decimalPortion = i - 4000 + 1;
                    int wholePortion = i - 4000;
                    expected = decimal.Parse($"{wholePortion}.{decimalPortion}", CultureInfo.InvariantCulture);
                }
                pocos.Add(new NullableSingleValuePoco() { Value = expected });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            //DecimalReader reader = new(stripeStreams, 1);
            var x = DecimalReader.Read(stripeStreams, 1);
            decimal?[] results = x.ToArray();

            for (int i = start; i < numValues; i++)
            {
                Assert.Equal(pocos[i-start].Value, results[i-start]);
            }
        }

        [Fact]
        public void TooMuchPrecisionThrows()
        {
            SerializationConfiguration serializationConfiguration = new SerializationConfiguration()
                .ConfigureType<SingleValuePoco>()
                    .ConfigureProperty(x => x.Value, x => { x.DecimalPrecision = 14; x.DecimalScale = 9; })
                    .Build();

            MemoryStream stream = new();
            OrcWriter<SingleValuePoco> goodWriter = new(stream, new WriterConfiguration(), serializationConfiguration);
            goodWriter.AddRow(new SingleValuePoco { Value = 12345.678901234m });
            goodWriter.Dispose();

            OrcWriter<SingleValuePoco> badWriter = new(stream, new WriterConfiguration(), serializationConfiguration);
            _ = Assert.Throws<OverflowException>(() =>
              {
                  badWriter.AddRow(new SingleValuePoco { Value = 123456.789012345m });
                  badWriter.Dispose();
              });
        }

        private static void RoundTripSingleValue(int numValues)
        {
            List<SingleValuePoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                decimal wholePortion = random.Next() % 99999;       //14-9
                decimal decimalPortion = random.Next() % 999999999; //9
                decimal value = wholePortion + decimalPortion / 1000000000m;
                pocos.Add(new SingleValuePoco { Value = value });
            }

            SerializationConfiguration configuration = new SerializationConfiguration()
                    .ConfigureType<SingleValuePoco>()
                        .ConfigureProperty(x => x.Value, x => { x.DecimalPrecision = 14; x.DecimalScale = 9; })
                        .Build();

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer, configuration);
            if (numValues == 0)
            {
                StripeStreamHelper.AssertNoStripeStream(stream, footer);
            }
            else
            {
                INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
                var results = DecimalReader.Read(stripeStreams, 1).ToArray();

                for (int i = 0; i < numValues; i++)
                {
                    Assert.Equal(pocos[i].Value, results[i]);
                }
            }
        }

        private class SingleValuePoco
        {
            public decimal Value { get; set; }
        }

        private static void RoundTripNulls(int numValues)
        {
            List<NullableSingleValuePoco> pocos = new();
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new NullableSingleValuePoco());
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            //DecimalReader reader = new(stripeStreams, 1);
            decimal?[] results = DecimalReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private class NullableSingleValuePoco
        {
            public decimal? Value { get; set; }
        }
    }
}
