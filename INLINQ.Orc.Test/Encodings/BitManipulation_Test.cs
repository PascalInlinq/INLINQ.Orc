using System.Numerics;
using Xunit;
using INLINQ.Orc.Encodings;

namespace INLINQ.Orc.Test.Encodings
{
    public class BitManipulationTest
    {
        //[Fact]
        //public void CheckedReadByteDataIsAvailableShouldReturnAByte()
        //{
        //    byte[] data = new byte[] { 0x11 };
        //    MemoryStream stream = new(data);
        //    byte result = stream.CheckedReadByte();
        //    Assert.Equal(0x11, result);
        //}

        //[Fact]
        //public void CheckedReadByteDataNotAvailableShouldThrow()
        //{
        //    byte[] data = new byte[] { 0x11 };
        //    MemoryStream stream = new(data);
        //    byte read1 = stream.CheckedReadByte();
        //    try
        //    {
        //        byte read2 = stream.CheckedReadByte();
        //        Assert.True(false);
        //    }
        //    catch (InvalidOperationException)
        //    { }
        //}

        [Fact]
        public void ReadLongBEVariousByteLengthsShouldWork()
        {
            Dictionary<long, byte[]> data = new()
            {
                { 0x11, new byte[] { 0x11 } },
                { 0x1122, new byte[] { 0x11, 0x22 } },
                { 0x112233, new byte[] { 0x11, 0x22, 0x33 } },
                { 0x11223344, new byte[] { 0x11, 0x22, 0x33, 0x44 } },
                { 0x1122334455, new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55 } },
                { 0x112233445566, new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 } },
                { 0x11223344556677, new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77 } },
                { 0x1122334455667788, new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 } }
            };

            foreach (KeyValuePair<long, byte[]> keyval in data)
            {
                MemoryStream stream = new(keyval.Value);
                byte[] buffer = stream.ToArray();
                uint position = 0;
                long expected = keyval.Key;
                long actual = buffer.ReadLongBE(keyval.Value.Length, ref position);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void ZigzagDecodeShouldZigZag()
        {
            Dictionary<long, long> data = new()
            {
                { 0, 0 },
                { -1, 1 },
                { 1, 2 },
                { -2, 3 },
                { 2, 4 }
            };

            foreach (KeyValuePair<long, long> keyval in data)
            {
                long expected = keyval.Key;
                long actual = keyval.Value.ZigzagDecode();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void ZigzagEncodeShouldRoundTrip()
        {
            List<long> data = new() { 0, 1, 2, 3, 4 };
            foreach (long value in data)
            {
                long expected = value;
                long encoded = value.ZigzagEncode();
                long actual = encoded.ZigzagDecode();
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void ReadBitpackedIntegersVariousBitWidthsShouldWork()
        {
            CheckBitpackedIntegersFromString(
                new long[] { 1, 0, 1, 1, 0 },
                "1 0 1 1 0",
                1);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1, 2, 3, 0, 1, 2, 3 },
                "00 01 10 11 00 01 10 11",
                2);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1, 2, 3, 4, 5, 6, 7 },
                "000 001 010 011 100 101 110 111",
                3);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1, 2, 4, 8, 15 },
                "0000 0001 0010 0100 1000 1111",
                4);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1, 2, 4, 8, 16, 31 },
                "00000 00001 00010 00100 01000 10000 11111",
                5);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1, 2, 4, 8, 16, 32, 63 },
                "000000 000001 000010 000100 001000 010000 100000 111111",
                6);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1, 2, 4, 8, 16, 32, 64, 127 },
                "0000000 0000001 0000010 0000100 0001000 0010000 0100000 1000000 1111111",
                7);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1, 2, 4, 8, 16, 32, 64, 128, 255 },
                "00000000 00000001 00000010 00000100 00001000 00010000 00100000 01000000 10000000 11111111",
                8);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 511, 0 },
                "000000000 111111111 000000000",
                9);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 1023, 0 },
                "0000000000 1111111111 0000000000",
                10);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 131071, 0 },
                "00000000000000000 11111111111111111 00000000000000000",
                17);

            CheckBitpackedIntegersFromString(
                new long[] { 0, 8589934591, 0 },
                "000000000000000000000000000000000 111111111111111111111111111111111 000000000000000000000000000000000",
                33);

            CheckBitpackedIntegersFromString(
                new long[] { 0, -1, 0 },
                "0000000000000000000000000000000000000000000000000000000000000000 " +
                "1111111111111111111111111111111111111111111111111111111111111111 " +
                "0000000000000000000000000000000000000000000000000000000000000000",
                64);
        }

        private static void CheckBitpackedIntegersFromString(long[] expected, string bits, int bitWidth)
        {
            byte[] bytesExpected = BitStringToByteArray(bits);
            MemoryStream readStream = new(bytesExpected);
            byte[] readStreamBuffer = readStream.ToArray();
            long[] readActual = readStreamBuffer.ReadBitpackedIntegers(bitWidth, expected.Length).ToArray();
            Assert.Equal(expected.Length, readActual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], readActual[i]);
            }

            MemoryStream writeStream = new();
            writeStream.WriteBitpackedIntegers(expected, bitWidth);
            byte[] writeBytesActual = writeStream.ToArray();
            Assert.Equal(bytesExpected.Length, writeBytesActual.Length);
            for (int i = 0; i < bytesExpected.Length; i++)
            {
                Assert.Equal(bytesExpected[i], writeBytesActual[i]);
            }
        }

        private static byte[] BitStringToByteArray(string bits)
        {
            List<byte> result = new();
            int bitCount = 0;
            byte currentByte = 0;
            foreach (char c in bits)
            {
                if (c != '1' && c != '0')
                {
                    continue;
                }

                currentByte <<= 1;
                if (c == '1')
                {
                    currentByte |= 1;
                }

                if (++bitCount > 7)
                {
                    result.Add(currentByte);
                    bitCount = 0;
                }
            }
            if (bitCount > 0)
            {
                currentByte <<= 8 - bitCount;     //Shift in zeros to fill out the rest of this byte
                result.Add(currentByte);
            }

            return result.ToArray();
        }

        [Fact]
        public void ReadVarIntSignedShouldMatchExpected()
        {
            Dictionary<long, byte[]> data = new()
            {
                { 0, new byte[] { 0x00 } },
                { 1, new byte[] { 0x01 } },
                { 127, new byte[] { 0x7f } },
                { 128, new byte[] { 0x80, 0x01 } },
                { 129, new byte[] { 0x81, 0x01 } },
                { 16383, new byte[] { 0xff, 0x7f } },
                { 16384, new byte[] { 0x80, 0x80, 0x01 } },
                { 16385, new byte[] { 0x81, 0x80, 0x01 } }
            };

            foreach (KeyValuePair<long, byte[]> keyval in data)
            {
                MemoryStream stream = new(keyval.Value);
                byte[] streamBuffer = stream.ToArray();
                long expected = keyval.Key;
                uint position = 0;
                long actual = streamBuffer.ReadVarIntUnsigned(ref position);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void RoundTripVarIntSigned()
        {
            long[] longs = new long[] { 0, 1000, -1000, 10000, -10000, 100000, -100000, int.MaxValue, int.MinValue };
            foreach (long expected in longs)
            {
                using MemoryStream stream = new();
                stream.WriteVarIntSigned(expected);
                _ = stream.Seek(0, SeekOrigin.Begin);
                byte[] streamBuffer = stream.ToArray();
                uint position = 0;
                long actual = streamBuffer.ReadVarIntSigned(ref position);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void RoundTripVarIntSignedExtents()
        {
            long[] longs = new long[] { long.MaxValue, long.MinValue };
            foreach (long expected in longs)
            {
                using MemoryStream stream = new();
                stream.WriteVarIntSigned(expected);
                _ = stream.Seek(0, SeekOrigin.Begin);
                byte[] streamBuffer = stream.ToArray();
                uint position = 0;
                long actual = streamBuffer.ReadVarIntSigned(ref position);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void RoundTripVarIntUnsigned()
        {
            long[] longs = new long[] { 0, 1000, 10000, 100000, uint.MaxValue };
            foreach (long expected in longs)
            {
                using MemoryStream stream = new();
                stream.WriteVarIntUnsigned(expected);
                _ = stream.Seek(0, SeekOrigin.Begin);
                byte[] streamBuffer = stream.ToArray();
                uint position = 0;
                long actual = streamBuffer.ReadVarIntUnsigned(ref position);
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void RoundTripVarIntUnsignedExtents()
        {
            using MemoryStream stream = new();
            ulong expected = 0xffffffffffffffff;
            stream.WriteVarIntUnsigned((long)expected);
            _ = stream.Seek(0, SeekOrigin.Begin);
            byte[] streamBuffer = stream.ToArray();
            uint position = 0;
            long actual = streamBuffer.ReadVarIntUnsigned(ref position);
            Assert.Equal(expected, (ulong)actual);
        }

        [Fact]
        public void RoundTripVarIntBigIntDebuggingSequences()
        {
            List<Tuple<uint, uint, uint, bool>> sequences = new()
            {
                Tuple.Create(0xffffffffu, 0xffffffffu, 0xffffffffu, false),
                Tuple.Create(0xffffffffu, 0xffffffffu, 0xffffffffu, true),
                Tuple.Create(0xf80fe03fu, 0x3f80fe03u, 0x3f80fe0u, false),
                Tuple.Create(0xf80fe03fu, 0x3f80fe03u, 0x3f80fe0u, true),
                Tuple.Create(~0xf80fe03fu, ~0x3f80fe03u, ~0x3f80fe0u, false),
                Tuple.Create(~0xf80fe03fu, ~0x3f80fe03u, ~0x3f80fe0u, true),
                Tuple.Create(0x870e1c38u, 0x3870e1c3u, 0xc3870e1cu, true),
                Tuple.Create(0x870e1c38u, 0x3870e1c3u, 0xc3870e1cu, false),
                Tuple.Create(~0x870e1c38u, ~0x3870e1c3u, ~0xc3870e1cu, true),
                Tuple.Create(~0x870e1c38u, ~0x3870e1c3u, ~0xc3870e1cu, false),
            };

            CheckBigIntVarInt(sequences);
        }

        [Fact]
        public void RoundTripVarIntBigIntRandom()
        {
            List<Tuple<uint, uint, uint, bool>> values = new();

            Random random = new(123);
            for (int i = 0; i < 1000; i++)
            {
                byte[] buffer = new byte[(4 * 3) + 1];
                random.NextBytes(buffer);

                values.Add(Tuple.Create(BitConverter.ToUInt32(buffer, 8), BitConverter.ToUInt32(buffer, 4), BitConverter.ToUInt32(buffer, 0), buffer[12] % 2 == 0));
            }

            CheckBigIntVarInt(values);
        }

        [Fact]
        public void RoundTripVarIntBigIntDebugging()
        {
            Tuple<uint, uint, uint, bool>[] values = new[] { Tuple.Create(0xffffffffu, 0xffffffffu, 0xffffffffu, true) };
            CheckBigIntVarInt(values);
        }

        private static void CheckBigIntVarInt(IEnumerable<Tuple<uint, uint, uint, bool>> values)
        {
            //TODO
            //foreach (Tuple<uint, uint, uint, bool> tuple in values)
            //{
            //    uint low = tuple.Item3;
            //    uint mid = tuple.Item2;
            //    uint high = tuple.Item1;
            //    bool isNegative = tuple.Item4;

            //    BigInteger expected = new BigInteger(low) | (new BigInteger(mid) << 32) | (new BigInteger(high) << 64);
            //    if (isNegative)
            //    {
            //        expected = -(expected + 1);
            //    }

            //    using MemoryStream stream = new();
            //    stream.WriteVarIntSigned(low, mid, high, isNegative);
            //    _ = stream.Seek(0, SeekOrigin.Begin);
            //    BigInteger? actual = stream.ReadBigVarInt();
            //    Assert.Equal(expected, actual);
            //}
        }
    }
}
