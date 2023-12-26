using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class TimestampColumnTest
    {
        [Fact]
        public void RoundTripTimestampColumn()
        {
            RoundTripSingleValue(70000);
        }

        [Fact]
        public void RoundTripTimestampColumnNullable()
        {
            RoundTripSingleValueNullable(70000, 10);
        }

        [Fact]
        public void RoundTripTimestampColumnNullableNone()
        {
            RoundTripSingleValueNullable(70000, 70001);
        }

        [Fact]
        public void RoundTripTimestampColumnNullableAll()
        {
            RoundTripSingleValueNullable(70000, 1);
        }

        //[Fact]
        //public void RoundTripTimestampColumn2()
        //{
        //    RoundTripSingleValue2(70000);
        //}

        //[Fact]
        //public void RoundTripTimestampColumnNullable2()
        //{
        //    RoundTripSingleValueNullable2(70000, 10);
        //}

        //[Fact]
        //public void RoundTripTimestampColumnNullableNone2()
        //{
        //    RoundTripSingleValueNullable2(70000, 70001);
        //}

        //[Fact]
        //public void RoundTripTimestampColumnNullableAll2()
        //{
        //    RoundTripSingleValueNullable2(70000, 1);
        //}

        private static void RoundTripSingleValue(int numValues)
        {
            Random random = new(123);
            List<SingleValuePoco> pocos = GenerateRandomTimestamps(random, numValues).Select(t => new SingleValuePoco { Value = t }).ToList();

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            DateTime?[] results = TimestampReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValueNullable(int numValues, int nullableFraction)
        {
            Random random = new(123);
            List<SingleValuePocoNullable> pocos = GenerateRandomTimestampsNullable(random, numValues, nullableFraction).Select(t => new SingleValuePocoNullable { Value = t }).ToList();

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            DateTime?[] results = TimestampReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValue2(int numValues)
        {
            Random random = new(123);
            List<SingleValuePoco2> pocos = GenerateRandomTimestamps(random, numValues).Select(t => new SingleValuePoco2 { Value = t }).ToList();

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            DateTimeOffset?[] results = TimestampReader.Read(stripeStreams, 1).Select(t => (DateTimeOffset?)t ).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValueNullable2(int numValues, int nullableFraction)
        {
            Random random = new(123);
            List<SingleValuePocoNullable2> pocos = GenerateRandomTimestampsNullable(random, numValues, nullableFraction).Select(t => new SingleValuePocoNullable2 { Value = t }).ToList();

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            DateTime?[] results = TimestampReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private enum Precision { Nanos, Micros, Millis, Seconds }

        private static DateTime RandomTimestamp(Random rnd)
        {
            DateTime baseTime = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            Precision p = (Precision)(rnd.Next() % 4);
            switch (p)
            {
                case Precision.Seconds: return baseTime.AddSeconds(GetSeconds(rnd));
                case Precision.Millis: return baseTime.AddSeconds(GetSeconds(rnd)).AddTicks(GetMillisecondTicks(rnd));
                case Precision.Micros: return baseTime.AddSeconds(GetSeconds(rnd)).AddTicks(GetMicrosecondTicks(rnd));
                case Precision.Nanos: return baseTime.AddSeconds(GetSeconds(rnd)).AddTicks(GetNanosecondTicks(rnd));
            }
            return default;
        }

        private static IEnumerable<DateTime> GenerateRandomTimestamps(Random rnd, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return RandomTimestamp(rnd);
            }
        }

        private static IEnumerable<DateTimeOffset> GenerateRandomTimestamps2(Random rnd, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return RandomTimestamp(rnd);
            }
        }

        private static IEnumerable<DateTime?> GenerateRandomTimestampsNullable(Random rnd, int count, int nullableFraction)
        {
            for (int i = 0; i < count; i++)
            {
                yield return i % nullableFraction == nullableFraction - 1
                    ? default(DateTime?)
                    : RandomTimestamp(rnd);
            }
        }

        private static IEnumerable<DateTimeOffset?> GenerateRandomTimestampsNullable2(Random rnd, int count, int nullableFraction)
        {
            for (int i = 0; i < count; i++)
            {
                yield return i % nullableFraction == nullableFraction - 1
                    ? default(DateTimeOffset?)
                    : RandomTimestamp(rnd);
            }
        }

        private static int GetSeconds(Random rnd)
        {
            int seconds = 10 * 365 * 24 * 60 * 60;  //10 years
            return (rnd.Next() % seconds) - (seconds / 2);    //A positive or negative random number
        }

        private static long GetMillisecondTicks(Random rnd)
        {
            return rnd.Next() % 1000 * TimeSpan.TicksPerMillisecond;
        }

        private static long GetMicrosecondTicks(Random rnd)
        {
            return rnd.Next() % (1000 * 1000) * (TimeSpan.TicksPerMillisecond / 1000);
        }

        private static long GetNanosecondTicks(Random rnd)
        {
            return rnd.Next() % (1000 * 1000 * 1000 / 100);
        }

        private class SingleValuePoco
        {
            public DateTime Value { get; set; }
        }

        private class SingleValuePocoNullable
        {
            public DateTime? Value { get; set; }
        }

        private class SingleValuePoco2
        {
            public DateTimeOffset Value { get; set; }
        }

        private class SingleValuePocoNullable2
        {
            public DateTimeOffset? Value { get; set; }
        }
    }
}
