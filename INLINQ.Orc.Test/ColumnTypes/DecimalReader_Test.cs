using INLINQ.Orc.Test.TestHelpers;
using INLINQ.Orc.Stripes;
using Xunit;
using INLINQ.Orc.ColumnTypes;
using System.Globalization;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class DecimalReaderTest
    {
        private static StripeStreamReaderCollection GetStripeStreamCollection()
        {
            DataFileHelper dataFile = new("decimal.orc");
            System.IO.Stream stream = dataFile.GetStream();
            FileTail fileTail = new(stream);
            StripeReaderCollection stripes = fileTail.Stripes;
            _ = Assert.Single(stripes);
            return stripes[0].GetStripeStreamCollection();
        }

        [Fact]
        public void ReadColumn1ShouldProduceExpectedResults()
        {
            StripeStreamReaderCollection stripeStreams = GetStripeStreamCollection();
            decimal?[] results = DecimalReader.Read(stripeStreams, 1).ToArray();

            Assert.Equal(6000, results.Length);
            for (int i = 0; i < results.Length; i++)
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
                Assert.Equal(expected, results[i]);
            }
        }

    }
}
