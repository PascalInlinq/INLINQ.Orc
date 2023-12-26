using INLINQ.Orc.Infrastructure;
using Xunit;

namespace INLINQ.Orc.Test.Infrastructure
{
    public class StreamSegmentTest
    {
        [Fact]
        public void ReadZeroLengthShouldReturn0()
        {
            MemoryStream stream = new();    //Empty
            StreamSegment segment = new(stream, 0, false);
            int result = ReadBytes(segment, 100);
            Assert.Equal(0, result);
        }

        [Fact]
        public void ReadOverrunShouldReturn0()
        {
            MemoryStream stream = new(new byte[] { 0x01, 0x02, 0x03 });
            StreamSegment segment = new(stream, 2, false);
            int successfulRead = ReadBytes(segment, 2);
            Assert.Equal(2, successfulRead);
            int unsuccessfulRead = ReadBytes(segment, 1);
            Assert.Equal(0, unsuccessfulRead);
        }

        [Fact]
        public void ReadOneByteAtATimeShouldReturnCorrectResult()
        {
            MemoryStream stream = new(new byte[] { 0x01, 0x02, 0x03 });
            StreamSegment segment = new(stream, 3, false);
            Assert.Equal(0x01, segment.ReadByte());
            Assert.Equal(0x02, segment.ReadByte());
            Assert.Equal(0x03, segment.ReadByte());
        }

        private static int ReadBytes(Stream stream, int numBytes)
        {
            byte[] tempBuffer = new byte[numBytes];
            return stream.Read(tempBuffer, 0, numBytes);
        }
    }
}
