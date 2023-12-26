using INLINQ.Orc.ColumnTypes;
using INLINQ.Orc.Protocol;
using Xunit;

namespace INLINQ.Orc.Test.ColumnTypes
{
    public class ByteColumnTest
    {
        [Fact]
        public void RoundTripByteColumn()
        {
            RoundTripSingleByte(70000);
        }

        [Fact]
        public void RoundTripByteColumnNullable()
        {
            RoundTripSingleByteNullable(70000);
        }

        private static void RoundTripSingleByte(int numValues)
        {
            List<SingleBytePoco> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new SingleBytePoco { Byte = (byte)random.Next() });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            byte?[] results = ByteReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Byte, results[i]);
            }
        }

        private static void RoundTripSingleByteNullable(int numValues)
        {
            List<SingleBytePocoNullable> pocos = new();
            Random random = new(123);
            for (int i = 0; i < numValues; i++)
            {
                pocos.Add(new SingleBytePocoNullable { Byte = i == 0 ? null : (byte)random.Next() });
            }

            MemoryStream stream = new();
            StripeStreamHelper.Write(stream, pocos, out Footer footer);
            INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
            //ByteReader boolReader = new(stripeStreams, 1);
            byte?[] results = ByteReader.Read(stripeStreams, 1).ToArray();

            for (int i = 0; i < numValues; i++)
            {
                Assert.Equal(pocos[i].Byte, results[i]);
            }
        }

        //private static void RoundTripSingleSByte(int numValues)
        //{
        //    List<SingleSBytePoco> pocos = new();
        //    Random random = new(123);
        //    for (int i = 0; i < numValues; i++)
        //    {
        //        pocos.Add(new SingleSBytePoco { Byte = (sbyte)random.Next() });
        //    }

        //    MemoryStream stream = new();
        //    StripeStreamHelper.Write(stream, pocos, out Footer footer);
        //    INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
        //    //ByteReader boolReader = new(stripeStreams, 1);
        //    sbyte?[] results = ByteReader.Read(stripeStreams, 1).ToArray();

        //    for (int i = 0; i < numValues; i++)
        //    {
        //        Assert.Equal(pocos[i].Byte, results[i]);
        //    }
        //}

        //private static void RoundTripSingleSByteNullable(int numValues)
        //{
        //    List<SingleSBytePocoNullable> pocos = new();
        //    Random random = new(123);
        //    for (int i = 0; i < numValues; i++)
        //    {
        //        pocos.Add(new SingleSBytePocoNullable { Byte = i == 0 ? null : (sbyte)random.Next() });
        //    }

        //    MemoryStream stream = new();
        //    StripeStreamHelper.Write(stream, pocos, out Footer footer);
        //    INLINQ.Orc.Stripes.StripeStreamReaderCollection stripeStreams = StripeStreamHelper.GetStripeStreams(stream, footer);
        //    //ByteReader boolReader = new(stripeStreams, 1);
        //    byte?[] results = ByteReader.Read(stripeStreams, 1).ToArray();

        //    for (int i = 0; i < numValues; i++)
        //    {
        //        Assert.Equal(pocos[i].Byte, results[i]);
        //    }
        //}

        private class SingleBytePoco
        {
            public byte Byte { get; set; }
        }

        private class SingleBytePocoNullable
        {
            public byte? Byte { get; set; }
        }

        private class SingleSBytePoco
        {
            public sbyte Byte { get; set; }
        }

        private class SingleSBytePocoNullable
        {
            public sbyte? Byte { get; set; }
        }
    }
}
