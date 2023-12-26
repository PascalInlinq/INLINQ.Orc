using INLINQ.Orc.Test.TestHelpers;
using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Stripes;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class LongReaderTest
    {
        private static StripeStreamReaderCollection GetStripeStreamCollection()
        {
            DataFileHelper dataFile = new("demo-12-zlib.orc");
            System.IO.Stream stream = dataFile.GetStream();
            INLINQ.Orc.FileTail fileTail = new(stream);
            StripeReaderCollection stripes = fileTail.Stripes;
            _ = Assert.Single(stripes);
            return stripes[0].GetStripeStreamCollection();
        }

        [Fact]
        public void ReadColumn1ShouldProduceExpectedResults()
        {
            StripeStreamReaderCollection stripeStreams = GetStripeStreamCollection();
            long?[] results = LongReader.Read(stripeStreams, 1).ToArray();

            Assert.Equal(1920800, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                int expected = i + 1;
                Assert.True(results[i].HasValue);
                Assert.Equal(expected, results[i].Value);
            }
        }

        [Fact]
        public void ReadColumn5ShouldProduceExpectedResults()
        {
            StripeStreamReaderCollection stripeStreams = GetStripeStreamCollection();
            long?[] results = LongReader.Read(stripeStreams, 5).ToArray();

            Assert.Equal(1920800, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                int expected = i / 70 * 500 % 10000 + 500;
                Assert.True(results[i].HasValue);
                Assert.Equal(expected, results[i].Value);
            }
        }

        [Fact]
        public void ReadColumn7ShouldProduceExpectedResults()
        {
            StripeStreamReaderCollection stripeStreams = GetStripeStreamCollection();
            long?[] results = LongReader.Read(stripeStreams, 7).ToArray();

            Assert.Equal(1920800, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                int expected = i / 5600 % 7;
                Assert.True(results[i].HasValue);
                Assert.Equal(expected, results[i].Value);
            }
        }

        [Fact]
        public void ReadColumn8ShouldProduceExpectedResults()
        {
            StripeStreamReaderCollection stripeStreams = GetStripeStreamCollection();
            long?[] results = LongReader.Read(stripeStreams, 8).ToArray();

            Assert.Equal(1920800, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                int expected = i / 39200 % 7;
                Assert.True(results[i].HasValue);
                Assert.Equal(expected, results[i].Value);
            }
        }

        [Fact]
        public void ReadColumn9ShouldProduceExpectedResults()
        {
            StripeStreamReaderCollection stripeStreams = GetStripeStreamCollection();
            long?[] results = LongReader.Read(stripeStreams, 9).ToArray();

            Assert.Equal(1920800, results.Length);
            for (int i = 0; i < results.Length; i++)
            {
                int expected = i / 274400;
                Assert.True(results[i].HasValue);
                Assert.Equal(expected, results[i].Value);
            }
        }
    }
}
