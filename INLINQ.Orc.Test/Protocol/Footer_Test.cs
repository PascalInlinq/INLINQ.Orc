using INLINQ.Orc.Compression;
using INLINQ.Orc.Protocol;
using ProtoBuf;
using Xunit;

namespace INLINQ.Orc.Test.Protocol
{
    public class FooterTest
    {
        [Fact]
        public void FooterShouldMatchExpected()
        {
            ProtocolHelper helper = new("demo-12-zlib.orc");
            int postscriptLength = helper.GetPostscriptLength();
            System.IO.Stream postscriptStream = helper.GetPostscriptStream(postscriptLength);
            PostScript postScript = Serializer.Deserialize<PostScript>(postscriptStream);
            ulong footerLength = postScript.FooterLength;
            System.IO.Stream footerStreamCompressed = helper.GetFooterCompressedStream(postscriptLength, footerLength);
            System.IO.Stream footerStream = OrcCompression.GetDecompressingStream(footerStreamCompressed, CompressionKind.Zlib);
            Footer footer = Serializer.Deserialize<Footer>(footerStream);

            Assert.Equal(1920800ul, footer.NumberOfRows);
            _ = Assert.Single(footer.Stripes);
            Assert.Equal(45592ul, footer.ContentLength);
            Assert.Equal(10000u, footer.RowIndexStride);

            Assert.Equal(1920800ul, footer.Stripes[0].NumberOfRows);
            Assert.Equal(3ul, footer.Stripes[0].Offset);
            Assert.Equal(14035ul, footer.Stripes[0].IndexLength);
            Assert.Equal(31388ul, footer.Stripes[0].DataLength);
            Assert.Equal(166ul, footer.Stripes[0].FooterLength);
        }
    }
}
