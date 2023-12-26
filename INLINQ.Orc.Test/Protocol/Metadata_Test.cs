using INLINQ.Orc.Compression;
using INLINQ.Orc.Protocol;
using ProtoBuf;
using Xunit;

namespace INLINQ.Orc.Test.Protocol
{
    public class MetadataTest
    {
        [Fact]
        public void MetadataShouldMatchExpected()
        {
            ProtocolHelper helper = new("demo-12-zlib.orc");
            int postscriptLength = helper.GetPostscriptLength();
            System.IO.Stream postscriptStream = helper.GetPostscriptStream(postscriptLength);
            PostScript postScript = Serializer.Deserialize<PostScript>(postscriptStream);
            ulong footerLength = postScript.FooterLength;
            ulong metadataLength = postScript.MetadataLength;
            System.IO.Stream metadataStreamCompressed = helper.GetMetadataCompressedStream(postscriptLength, footerLength, metadataLength);
            System.IO.Stream metadataStream = OrcCompression.GetDecompressingStream(metadataStreamCompressed, CompressionKind.Zlib);
            Metadata metadata = Serializer.Deserialize<Metadata>(metadataStream);

            _ = Assert.Single(metadata.StripeStats);
            Assert.Equal(10, metadata.StripeStats[0].ColStats.Count);
        }
    }
}
