using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;
using System;
using System.Diagnostics;

namespace INLINQ.Orc.ColumnTypes
{
    public class LongWriter : NullableWriter, IColumnWriter
    {
        private readonly bool _shouldAlignEncodedValues;

        public LongWriter(bool isNullable, bool shouldAlignEncodedValues, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base (isNullable, bufferFactory, columnId, ColumnEncodingKind.DirectV2)
        {
            _shouldAlignEncodedValues = shouldAlignEncodedValues;
        }

        public int AddBlock(ReadOnlyMemory<int> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            long[] valList = new long[presentValueCount];
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                int value = valuesSpan[index];
                valList[index] = value; //because cast is necessary
                stats.AddValue(value);
            }

            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, new ReadOnlySpan<long>(valList, 0, presentValueCount), true, _shouldAlignEncodedValues);
            return presentValueCount;
        }

        public int AddBlock(ReadOnlyMemory<long> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                long value = valuesSpan[index];
                index++;
                stats.AddValue(value);
            }

            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, values.Slice(0, presentValueCount).Span, true, _shouldAlignEncodedValues);
            return presentValueCount;
        }

        public int AddBlock(ReadOnlyMemory<short> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            long[] valList = new long[presentValueCount];
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                long value = valuesSpan[index];
                valList[index] = value;
                stats.AddValue(value);
            }

            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, new ReadOnlySpan<long>(valList, 0, presentValueCount), true, _shouldAlignEncodedValues);
            return presentValueCount;
        }

        public int AddBlock(ReadOnlyMemory<uint> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            long[] valList = new long[presentValueCount];
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                long value = valuesSpan[index];
                valList[index] = value;
                stats.AddValue(value);
            }

            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, new ReadOnlySpan<long>(valList, 0, presentValueCount), true, _shouldAlignEncodedValues);
            return presentValueCount;
        }

        public int AddBlock(ReadOnlyMemory<ulong> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            long[] valList = new long[presentValueCount];
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                long value = (long)valuesSpan[index];
                valList[index] = value;
                stats.AddValue(value);
            }

            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, new ReadOnlySpan<long>(valList, 0, presentValueCount), true, _shouldAlignEncodedValues);
            return presentValueCount;
        }

        public int AddBlock(ReadOnlyMemory<ushort> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            LongWriterStatistics stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
            long[] valList = new long[presentValueCount];
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                long value = valuesSpan[index];
                valList[index] = value;
                stats.AddValue(value);
            }

            IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, new ReadOnlySpan<long>(valList, 0, presentValueCount), true, _shouldAlignEncodedValues);
            return presentValueCount;
        }
    }
}