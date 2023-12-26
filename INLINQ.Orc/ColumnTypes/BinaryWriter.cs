using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;

namespace INLINQ.Orc.ColumnTypes
{
    public class BinaryWriter : NullableWriter, IColumnWriter
    {
        private readonly bool _shouldAlignLengths;
        private readonly OrcCompressedBuffer _lengthBuffer;

        public BinaryWriter(bool shouldAlignLengths, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base (true, bufferFactory, columnId, ColumnEncodingKind.DirectV2, StreamKind.Length)
        {
            _shouldAlignLengths = shouldAlignLengths;
            _lengthBuffer = Buffers[Buffers.Length - 1];
        }

        public int AddBlock(ReadOnlyMemory<byte[]> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            BinaryWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats);
            _lengthBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);

            var valuesSpan = values.Span;
            List<byte[]?>? bytesList = new(presentValueCount);
            long[] lengthList = new long[presentValueCount];
            for (int index = 0; index < presentValueCount; index++)
            {
                //is present:
                byte[] bytes = valuesSpan[index];
                stats.AddValue(bytes);
                bytesList.Add(bytes);
                lengthList[index] = bytes.Length;
            }

            foreach (byte[]? bytes in bytesList)
            {
                if (bytes != null)
                {
                    _dataBuffer.Write(bytes, 0, bytes.Length);
                }
            }

            IntegerRunLengthEncodingV2Writer.Write(_lengthBuffer, lengthList, false, _shouldAlignLengths);
            return presentValueCount;
        }
    }
}