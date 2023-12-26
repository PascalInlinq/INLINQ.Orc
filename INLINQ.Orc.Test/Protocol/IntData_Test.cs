
namespace INLINQ.Orc.Test.Protocol
{
    public class IntDataTest
    {
        //[Fact]
        //public void ReadIntData()
        //{
        //    ProtocolHelper helper = new("demo-12-zlib.orc");
        //    int postscriptLength = helper.GetPostscriptLength();
        //    System.IO.Stream postscriptStream = helper.GetPostscriptStream(postscriptLength);
        //    PostScript postScript = Serializer.Deserialize<PostScript>(postscriptStream);
        //    ulong footerLength = postScript.FooterLength;
        //    System.IO.Stream footerStreamCompressed = helper.GetFooterCompressedStream(postscriptLength, footerLength);
        //    System.IO.Stream footerStream = OrcCompressedStream.GetDecompressingStream(footerStreamCompressed, CompressionKind.Zlib);
        //    Footer footer = Serializer.Deserialize<Footer>(footerStream);

        //    StripeInformation stripeDetails = footer.Stripes[0];
        //    System.IO.Stream streamFooterStreamCompressed = helper.GetStripeFooterCompressedStream(stripeDetails.Offset, stripeDetails.IndexLength, stripeDetails.DataLength, stripeDetails.FooterLength);
        //    System.IO.Stream stripeFooterStream = OrcCompressedStream.GetDecompressingStream(streamFooterStreamCompressed, CompressionKind.Zlib);
        //    StripeFooter stripeFooter = Serializer.Deserialize<StripeFooter>(stripeFooterStream);

        //    ulong offset = stripeDetails.Offset;
        //    foreach (INLINQ.Orc.Protocol.Stream stream in stripeFooter.Streams)
        //    {
        //        ColumnType columnInFooter = footer.Types[(int)stream.Column];
        //        ColumnEncoding columnInStripe = stripeFooter.Columns[(int)stream.Column];
        //        if (columnInFooter.Kind == ColumnTypeKind.Int)
        //        {
        //            if (stream.Kind == StreamKind.Data)
        //            {
        //                Assert.Equal(ColumnEncodingKind.DirectV2, columnInStripe.Kind);

        //                System.IO.Stream dataStreamCompressed = helper.GetDataCompressedStream(offset, stream.Length);
        //                System.IO.Stream dataStream = OrcCompressedStream.GetDecompressingStream(dataStreamCompressed, CompressionKind.Zlib);
        //                IntegerRunLengthEncodingV2Reader reader = new(dataStream, true);
        //                long[] result = reader.Read().ToArray();

        //                for (int i = 0; i < result.Length; i++)
        //                {
        //                    if (stream.Column == 1)
        //                    {
        //                        int expected = i + 1;
        //                        Assert.Equal(expected, result[i]);
        //                    }
        //                    else if (stream.Column == 5)
        //                    {
        //                        int expected = i / 70 * 500 % 10000 + 500;
        //                        Assert.Equal(expected, result[i]);
        //                    }
        //                    else if (stream.Column == 7)
        //                    {
        //                        int expected = i / 5600 % 7;
        //                        Assert.Equal(expected, result[i]);
        //                    }
        //                    else if (stream.Column == 8)
        //                    {
        //                        int expected = i / 39200 % 7;
        //                        Assert.Equal(expected, result[i]);
        //                    }
        //                    else if (stream.Column == 9)
        //                    {
        //                        int expected = i / 274400;
        //                        Assert.Equal(expected, result[i]);
        //                    }
        //                    else
        //                    {
        //                        Assert.True(false, "Unexpected column");
        //                    }
        //                }
        //            }
        //        }

        //        offset += stream.Length;
        //    }
        //}
    }
}
