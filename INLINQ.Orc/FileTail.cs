using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Infrastructure;
using INLINQ.Orc.Stripes;
using ProtoBuf;

namespace INLINQ.Orc
{
    public class FileTail
    {
        private readonly Stream _inputStream;

        public Protocol.PostScript PostScript { get; }
        public Protocol.Footer Footer { get; }
        public StripeReaderCollection Stripes { get; }

        public FileTail(Stream inputStream)
        {
            _inputStream = inputStream;

            PostScript = ReadPostScript(out byte postScriptLength);
            Footer = ReadFooter(PostScript, postScriptLength);
            Stripes = new StripeReaderCollection(_inputStream, Footer, PostScript.Compression);
        }

        private Protocol.PostScript ReadPostScript(out byte postScriptLength)
        {
            _ = _inputStream.Seek(-1, SeekOrigin.End);
            postScriptLength = _inputStream.CheckedReadByte();
            _ = _inputStream.Seek(-1 - postScriptLength, SeekOrigin.End);
            //byte[] buffer = new byte[postScriptLength];
            //ReadOnlyMemory<byte> source = new ReadOnlyMemory<byte>(buffer);
            //int bytesRead = _inputStream.Read(buffer, 0, postScriptLength);
            //Protocol.PostScript postScript = Serializer.Deserialize(source, new Protocol.PostScript());
            //Protocol.PostScript postScript = Serializer.Deserialize(stream, new Protocol.PostScript(), postScriptLength);
            Protocol.PostScript postScript = StreamSegment.ReadObject<Protocol.PostScript>(_inputStream, postScriptLength);

            if (postScript.Magic != "ORC")
            {
                throw new InvalidDataException("Postscript didn't contain magic bytes");
            }

            return postScript;
        }

        private Protocol.Footer ReadFooter(Protocol.PostScript postScript, byte postScriptLength)
        {
            _ = _inputStream.Seek(-1 - postScriptLength - (long)postScript.FooterLength, SeekOrigin.End);
            StreamSegment? compressedStream = new(_inputStream, (long)postScript.FooterLength, true);
            ConcatenatingStream footerStream = OrcCompression.GetDecompressingStream(compressedStream, postScript.Compression);
            return Serializer.Deserialize<Protocol.Footer>(footerStream.ReadAll().AsSpan());

        }
    }
}
