using INLINQ.Orc.Compression;
using INLINQ.Orc.Infrastructure;
using INLINQ.Orc.Protocol;
using ProtoBuf;
using Xunit;

namespace INLINQ.Orc.Test.Protocol
{
    public class RowIndexTest
    {
        [Fact]
        public void RowIndexShouldMatchExpected()
        {
            ProtocolHelper helper = new("demo-12-zlib.orc");
            int postscriptLength = helper.GetPostscriptLength();
            System.IO.Stream postscriptStream = helper.GetPostscriptStream(postscriptLength);
            PostScript postScript = Serializer.Deserialize<PostScript>(postscriptStream);
            ulong footerLength = postScript.FooterLength;
            System.IO.Stream footerStreamCompressed = helper.GetFooterCompressedStream(postscriptLength, footerLength);
            ConcatenatingStream footerStream = OrcCompression.GetDecompressingStream(footerStreamCompressed, CompressionKind.Zlib);
            Footer footer = Serializer.Deserialize<Footer>(footerStream.ReadAll().AsSpan());

            StripeInformation stripeDetails = footer.Stripes[0];
            System.IO.Stream streamFooterStreamCompressed = helper.GetStripeFooterCompressedStream(stripeDetails.Offset, stripeDetails.IndexLength, stripeDetails.DataLength, stripeDetails.FooterLength);
            ConcatenatingStream stripeFooterStream = OrcCompression.GetDecompressingStream(streamFooterStreamCompressed, CompressionKind.Zlib);
            StripeFooter stripeFooter = Serializer.Deserialize<StripeFooter>(stripeFooterStream.ReadAll().AsSpan());

            ulong offset = stripeDetails.Offset;
            foreach (INLINQ.Orc.Protocol.Stream stream in stripeFooter.Streams)
            {
                if (stream.Kind == StreamKind.RowIndex)
                {
                    System.IO.Stream rowIndexStreamCompressed = helper.GetRowIndexCompressedStream(offset, stream.Length);
                    ConcatenatingStream rowIndexStream = OrcCompression.GetDecompressingStream(rowIndexStreamCompressed, CompressionKind.Zlib);
                    RowIndex rowIndex = Serializer.Deserialize<RowIndex>(rowIndexStream.ReadAll().AsSpan());
                }

                offset += stream.Length;
            }
        }
    }
}
