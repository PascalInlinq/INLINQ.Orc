using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.FluentSerialization;
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class DateColumnTest
    {
        [Fact]
        public void RoundTripDateColumn()
        {
            RoundTripSingleValue(70000, false);
        }

        [Fact]
        public void RoundTripDateColumnNull()
        {
            RoundTripSingleValue(70000, true);
        }

        [Fact]
        public void RoundTripDateColumnNullable()
        {
            RoundTripSingleValueNullable(70000, false);
        }


        private static void RoundTripSingleValue(int numValues, bool makeNullStruct)
        {
            Random random = new(123);
            List<SingleValuePoco> pocos = GenerateRandomDates(random, numValues).Select(t => new SingleValuePoco { Value = t }).ToList();
            if(makeNullStruct)
            {
                pocos[0] = null;
            }


            SerializationConfiguration configuration = new SerializationConfiguration()
                                .ConfigureType<SingleValuePoco>()
                                    .ConfigureProperty(x => x.Value, x => x.SerializeAsDate = true)
                                    .Build();

            MemoryStream stream = new();
            Exception writeException = null;
            Footer footer = null;
            try
            {
                StripeStreamHelper.Write(stream, pocos, out footer, configuration);
            }
            catch (Exception ex)
            {
                writeException = ex;
            }

            if (writeException == null)
            {
                INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
                //DateReader reader = new(stripeStreams, 1);
                DateTime?[] results = DateReader.Read(stripeStreams, 1).ToArray();

                for (int i = 0; i < numValues; i++)
                {
                    if (pocos[i] == null)
                    {
                        Assert.Null(results[i]);
                    }
                    Assert.Equal(pocos[i].Value, results[i]);
                }
            }
            else
            {
                Assert.True(makeNullStruct);
                Assert.Equal("Value cannot be null. (Parameter 'element in rows')", writeException.Message);
            }
        }

        private static void RoundTripSingleValueNullable(int numValues, bool makeNullStruct)
        {
            Random random = new(123);
            List<SingleValuePocoNullable> pocos = GenerateRandomDates(random, numValues).Select(t => new SingleValuePocoNullable { Value = t }).ToList();
            pocos[0].Value = null;
            if (makeNullStruct)
            {
                pocos[0] = null;
            }

            SerializationConfiguration configuration = new SerializationConfiguration()
                                .ConfigureType<SingleValuePocoNullable>()
                                    .ConfigureProperty(x => x.Value, x => x.SerializeAsDate = true)
                                    .Build();

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer, configuration);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            //DateReader reader = new(stripeStreams, 1);
            DateTime?[] results = DateReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                if (pocos[i] == null)
                {
                    Assert.Null(results[i]);
                }
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValueStructNull(int numValues)
        {
            Random random = new(123);
            List<SingleValuePocoNullable> pocos = GenerateRandomDates(random, numValues).Select(t => new SingleValuePocoNullable { Value = t }).ToList();
            pocos[0] = null;

            SerializationConfiguration configuration = new SerializationConfiguration()
                                .ConfigureType<SingleValuePocoNullable>()
                                    .ConfigureProperty(x => x.Value, x => x.SerializeAsDate = true)
                                    .Build();

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer, configuration);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            //DateReader reader = new(stripeStreams, 1);
            DateTime?[] results = DateReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static IEnumerable<DateTime> GenerateRandomDates(Random rnd, int count)
        {
            DateTime baseTime = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            for (int i = 0; i < count; i++)
            {
                yield return baseTime.AddDays((rnd.Next() % (360 * 200)) - (360 * 100));
            }
        }

        private class SingleValuePoco
        {
            public DateTime Value { get; set; }
        }

        private class SingleValuePocoNullable
        {
            public DateTime? Value { get; set; }
        }
    }
}
