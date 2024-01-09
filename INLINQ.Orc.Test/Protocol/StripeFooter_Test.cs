using INLINQ.Orc.Compression;
using INLINQ.Orc.Protocol;
using ProtoBuf;
using Xunit;

namespace INLINQ.Orc.Test.Protocol
{
    public class StripeFooterTest
    {
        [Fact]
        private void StripeFooter_ShouldMatchExpected()
        {
            ProtocolHelper helper = new("demo-12-zlib.orc");
            int postscriptLength = helper.GetPostscriptLength();
            System.IO.Stream postscriptStream = helper.GetPostscriptStream(postscriptLength);
            PostScript postScript = Serializer.Deserialize<PostScript>(postscriptStream);
            ulong footerLength = postScript.FooterLength;
            System.IO.Stream footerStreamCompressed = helper.GetFooterCompressedStream(postscriptLength, footerLength);
            var footerStream = OrcCompression.GetDecompressingStream(footerStreamCompressed, CompressionKind.Zlib);
            Footer footer = Serializer.Deserialize<Footer>(footerStream.ReadAll().AsSpan());

            StripeInformation stripeDetails = footer.Stripes[0];
            System.IO.Stream streamFooterStreamCompressed = helper.GetStripeFooterCompressedStream(stripeDetails.Offset, stripeDetails.IndexLength, stripeDetails.DataLength, stripeDetails.FooterLength);
            var stripeFooterStream = OrcCompression.GetDecompressingStream(streamFooterStreamCompressed, CompressionKind.Zlib);
            StripeFooter stripeFooter = Serializer.Deserialize<StripeFooter>(stripeFooterStream.ReadAll().AsSpan());

            Assert.Equal(10, stripeFooter.Columns.Count);
            Assert.Equal(27, stripeFooter.Streams.Count);
        }
    }
}
