using INLINQ.Orc.Protocol;
using ProtoBuf;
using Xunit;

namespace INLINQ.Orc.Test.Protocol
{
    public class PostScriptTest
    {
        [Fact]
        public void PostScriptShouldMatchExpected()
        {
            ProtocolHelper helper = new("demo-12-zlib.orc");
            int postscriptLength = helper.GetPostscriptLength();
            System.IO.Stream postscriptStream = helper.GetPostscriptStream(postscriptLength);
            PostScript postScript = Serializer.Deserialize<PostScript>(postscriptStream);

            Assert.Equal("ORC", postScript.Magic);
            Assert.Equal(221u, postScript.FooterLength);
            Assert.Equal(CompressionKind.Zlib, postScript.Compression);
            Assert.Equal(262144u, postScript.CompressionBlockSize);
            Assert.Equal(0u, postScript.VersionMajor.Value);
            Assert.Equal(12u, postScript.VersionMinor.Value);
            Assert.Equal(140u, postScript.MetadataLength);
            Assert.Equal(1u, postScript.WriterVersion);
        }
    }
}
