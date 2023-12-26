using INLINQ.Orc;
using INLINQ.Orc.Compression;
using INLINQ.Orc.FluentSerialization;
using INLINQ.Orc.Protocol;
using INLINQ.Orc.Stripes;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public static class StripeStreamHelper
    {
        public static void Write<T>(System.IO.Stream outputStream, IEnumerable<T> values, out Footer footer, SerializationConfiguration serializationConfiguration = null, int rowIndexStride= 10240) where T : class
        {
            OrcCompressedBufferFactory bufferFactory = new(256 * 1024, CompressionKind.Zlib, CompressionStrategy.Size);
            StripeWriter<T> stripeWriter = new(outputStream, false, 0.8, 18, 6, bufferFactory, rowIndexStride, 512 * 1024 * 1024, serializationConfiguration);
            stripeWriter.AddRows(values);
            stripeWriter.RowAddingCompleted();
            footer = stripeWriter.GetFooter();

            _ = outputStream.Seek(0, SeekOrigin.Begin);
        }

        public static StripeStreamReaderCollection GetStripeStreams(System.IO.Stream inputStream, Footer footer)
        {
            StripeReaderCollection stripes = new(inputStream, footer, CompressionKind.Zlib);
            _ = Assert.Single(stripes);
            return stripes[0].GetStripeStreamCollection();
        }

        public static void AssertNoStripeStream(System.IO.Stream inputStream, Footer footer)
        {
            StripeReaderCollection stripes = new(inputStream, footer, CompressionKind.Zlib);
            Assert.Empty(stripes);
        }
    }
}
