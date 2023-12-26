using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;
using System.Diagnostics;

namespace INLINQ.Orc.ColumnTypes
{
    public abstract class NullableWriter //: IColumnWriter
    {
        protected readonly OrcCompressedBuffer _dataBuffer;
        public OrcCompressedBuffer[] Buffers { get; }

        private OrcCompressedBuffer? PresentBuffer { get; }

        public List<IStatistics> Statistics { get; } = new List<IStatistics>();
        public long CompressedLength => Buffers.Sum(s => s.Length);
        public uint ColumnId { get; }
        
        public ColumnEncodingKind ColumnEncoding { get; set; }

        public NullableWriter(bool isNullable, OrcCompressedBufferFactory bufferFactory, uint columnId, ColumnEncodingKind columnEncoding, params StreamKind[] extraStreams)
        {
            _dataBuffer = bufferFactory.CreateBuffer(StreamKind.Data);
            if (isNullable)
            {
                PresentBuffer = bufferFactory.CreateBuffer(StreamKind.Present);
                PresentBuffer.MustBeIncluded = false;           //If we never have nulls, we won't write this stream
            }

            Buffers = new OrcCompressedBuffer[extraStreams.Length + (PresentBuffer == null ? 1 : 2)];
            int bufferId = 0;
            if (PresentBuffer != null)
            {
                Buffers[bufferId++] = PresentBuffer;
            }
            Buffers[bufferId++] = _dataBuffer;
            int extraStreamIndex = 0;
            while(bufferId < Buffers.Length)
            {
                Buffers[bufferId++] = bufferFactory.CreateBuffer(extraStreams[extraStreamIndex++]); 
            }

            ColumnId = columnId;
            ColumnEncoding = columnEncoding;
        }

        public void FlushBuffers()
        {
            foreach (OrcCompressedBuffer? buffer in Buffers)
            {
                buffer.Flush();
            }
        }

        public void Reset()
        {
            foreach (OrcCompressedBuffer? buffer in Buffers)
            {
                buffer.Reset();
            }

            if (PresentBuffer != null)
            {
                PresentBuffer.MustBeIncluded = false;
            }

            Statistics.Clear();
        }


        public int WritePresentMaps(IStatistics stats, object presentMapsObj, int valueCount)
        {
            var presentMaps = ((ReadOnlyMemory<ulong>)presentMapsObj).Span;
            if (PresentBuffer != null)
            {
                PresentBuffer.AnnotatePosition(stats, rleValuesToConsume: 0, bitsToConsume: 0);
            }

            int presentValueCount;
            if (PresentBuffer != null)
            {
                int presentIndex = 0;
                ulong presentMap = 0;
                int presentByteIndex = 0;
                byte[] presentByteMap = new byte[(valueCount + 7) / 8];
                int mapCount = (valueCount + 63) / 64;
                presentValueCount = 0;
                for (int i = 0; i < mapCount;)
                {
                    i++;
                    presentMap = presentMaps[presentIndex++];
                    presentValueCount += System.Numerics.BitOperations.PopCount(presentMap);
                    int byteCount = (i == mapCount) ? presentByteMap.Length - presentByteIndex : 8;
                    for (int j = 0; j < byteCount; j++)
                    {
                        byte b = (byte)(presentMap >> 56);
                        presentMap <<= 8;
                        presentByteMap[presentByteIndex++] = b;
                    }
                }

                BitWriter? presentEncoder = new(PresentBuffer);
                presentEncoder.Write(presentByteMap);
                if (presentValueCount < valueCount)
                {
                    PresentBuffer.MustBeIncluded = true;     //A null occurred.  Make sure to write this stream
                }
                if (presentValueCount < valueCount)
                {
                    stats.HasNull = true;
                }
            }
            else
            {
                presentValueCount = valueCount;
                if (presentMaps.Length > 0)
                {
                    throw new NotSupportedException("Mixing nullables");
                }
            }

            return presentValueCount;
        }
    }
}