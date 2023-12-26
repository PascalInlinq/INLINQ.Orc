using INLINQ.Orc.Protocol;
using System.Text;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class StringColumnTest
    {
        [Fact]
        public void RoundTripStringColumnDirect()
        {
            RoundTripSingleValue_Direct(70000);
        }

        [Fact]
        public void RoundTripStringColumnNullable()
        {
            RoundTripSingleValue_Nullable(70000);
        }

        [Fact]
        public void RoundTripStringColumnDictionary()
        {
            RoundTripSingleValue_Dictionary(70000);
        }

        [Fact]
        public void RoundTripStringColumnDictionaryVaryDictionarySize()
        {
            RoundTripSingleValue_Dictionary_VaryDictionarySize(70000);
        }

        [Fact]
        public void RoundTripStringColumnDictionaryWithNulls()
        {
            RoundTripSingleValue_Dictionary_WithNulls(70000);
        }

        private static void RoundTripSingleValue_Direct(int numValues)
        {
            Random random = new(123);
            List<SingleValuePoco> pocos = GenerateRandomStrings(random, numValues, numValues).Select(s => new SingleValuePoco { Value = s }).ToList();
            string[] results = RoundTripSingleValue(pocos);
            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValue_Nullable(int numValues)
        {
            Random random = new(123);
            List<SingleValuePoco> pocos = GenerateRandomStrings(random, numValues, numValues).Select(s => new SingleValuePoco { Value = s }).ToList();
            pocos[0].Value = null;
            string[] results = RoundTripSingleValue(pocos);
            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValue_Dictionary(int numValues)
        {
            Random random = new(123);
            List<SingleValuePoco> pocos = GenerateRandomStrings(random, numValues, 100).Select(s => new SingleValuePoco { Value = s }).ToList();
            string[] results = RoundTripSingleValue(pocos);

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValue_Dictionary_VaryDictionarySize(int numValues)
        {
            Random random = new(123);
            List<SingleValuePoco> pocos = GenerateRandomStrings(random, numValues / 10, 10)
                .Concat(GenerateRandomStrings(random, numValues / 10, 20))
                .Concat(GenerateRandomStrings(random, numValues / 10, 30))
                .Concat(GenerateRandomStrings(random, numValues / 10, 40))
                .Concat(GenerateRandomStrings(random, numValues / 10, 50))
                .Concat(GenerateRandomStrings(random, numValues / 10, 50))
                .Concat(GenerateRandomStrings(random, numValues / 10, 40))
                .Concat(GenerateRandomStrings(random, numValues / 10, 30))
                .Concat(GenerateRandomStrings(random, numValues / 10, 20))
                .Concat(GenerateRandomStrings(random, numValues / 10, 10))
                .Select(s => new SingleValuePoco { Value = s }).ToList();
            string[] results = RoundTripSingleValue(pocos);

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static void RoundTripSingleValue_Dictionary_WithNulls(int numValues)
        {
            Random random = new(123);
            List<SingleValuePoco> pocos = GenerateRandomStrings(random, numValues, 100, includeNulls: true).Select(s => new SingleValuePoco { Value = s }).ToList();
            string[] results = RoundTripSingleValue(pocos);

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Value, results[i]);
            }
        }

        private static IEnumerable<string> GenerateRandomStrings(Random rnd, int count, int uniqueCount, bool includeNulls = false)
        {
            List<string> strings = new(uniqueCount);
            for (int i = 0; i < uniqueCount; i++)
            {
                strings.Add(GenerateRandomString(rnd));
            }

            for (int i = 0; i < count; i++)
            {
                int id = rnd.Next() % uniqueCount;
                yield return includeNulls && id == 0 ? null : strings[id];
            }
        }

        private static string GenerateRandomString(Random rnd)
        {
            int minimumLength = 0;
            int maximumLength = 25;
            int minimumAscii = 0x20;
            int maximumAscii = 0x7e;
            int length = (rnd.Next() % (maximumLength - minimumLength + 1)) + minimumLength;
            StringBuilder sb = new();
            for (int i = 0; i < length; i++)
            {
                _ = sb.Append((char)(byte)((rnd.Next() % (maximumAscii - minimumAscii + 1)) + minimumAscii));
            }

            return sb.ToString();
        }

        private static string[] RoundTripSingleValue(IEnumerable<SingleValuePoco> pocos)
        {
            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            return INLINQ.Orc.ColumnTypes.StringReader.Read(stripeStreams, 1).ToArray();
        }

        private class SingleValuePoco
        {
            public string Value { get; set; }
        }
    }
}
