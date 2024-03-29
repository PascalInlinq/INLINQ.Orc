﻿using Xunit;

namespace INLINQ.Orc.Test.Compression
{
    public class ZLibStreamTest
    {
        [Fact]
        public void ReadCompressedDataLast4()
        {
            byte[] data = GetCompressedData();
            MemoryStream stream = new(data);
            INLINQ.Orc.Compression.ZLibStream decompressingStream = new(stream);
            Assert.Equal(0x10D8, ReadBytes(decompressingStream, 0x10D8));
            Assert.Equal(0x4, ReadBytes(decompressingStream, 0x4));
        }

        [Fact]
        public void ReadCompressedDataLast2()
        {
            byte[] data = GetCompressedData();
            MemoryStream stream = new(data);
            INLINQ.Orc.Compression.ZLibStream decompressingStream = new(stream);
            Assert.Equal(0x10D8, ReadBytes(decompressingStream, 0x10D8));
            Assert.Equal(0x2, ReadBytes(decompressingStream, 0x2));
            Assert.Equal(0x2, ReadBytes(decompressingStream, 0x2));
        }

        [Fact]
        public void ReadCompressedDataLast1()
        {
            byte[] data = GetCompressedData();
            MemoryStream stream = new(data);
            INLINQ.Orc.Compression.ZLibStream decompressingStream = new(stream);
            Assert.Equal(0x10D8, ReadBytes(decompressingStream, 0x10D8));
            Assert.Equal(0x3, ReadBytes(decompressingStream, 0x3));
            Assert.Equal(0x1, ReadBytes(decompressingStream, 0x1));
        }

        private static int ReadBytes(Stream stream, int numBytes)
        {
            byte[] tempBuffer = new byte[numBytes];
            return stream.Read(tempBuffer, 0, numBytes);
        }

        private static byte[] GetCompressedData()
        {
            return new byte[] {
                0xED, 0xD0, 0x41, 0x0D, 0x00, 0x30, 0x08, 0x00,
                0x31, 0x04, 0xCC, 0x03, 0x72, 0xA6, 0x7B, 0x7F,
                0x3C, 0x0D, 0x13, 0x7C, 0x48, 0x7A, 0x0A, 0x2E,
                0xAD, 0xDA, 0xD9, 0xFD, 0x3B, 0xBF, 0xE7, 0xAE,
                0x09, 0x10, 0x20, 0x40, 0x80, 0x00, 0x01, 0x02,
                0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x00, 0x01,
                0x02, 0x2F, 0x4F, 0x34
            };
        }
    }
}
