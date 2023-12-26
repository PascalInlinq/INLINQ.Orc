using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Infrastructure;
using INLINQ.Orc.Protocol;

namespace INLINQ.Orc.ColumnTypes
{
    public class DecimalWriter : NullableWriter, IColumnWriter
    {
        private readonly bool _shouldAlignEncodedValues;
        private readonly int _precision;
        private readonly int _scale;
        private readonly OrcCompressedBuffer _secondaryBuffer;

        public DecimalWriter(bool isNullable, bool shouldAlignEncodedValues, int precision, int scale, OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base(isNullable, bufferFactory, columnId, ColumnEncodingKind.DirectV2, StreamKind.Secondary)
        {
            _shouldAlignEncodedValues = shouldAlignEncodedValues;
            _precision = precision;
            _scale = scale;
            _secondaryBuffer = Buffers[Buffers.Length - 1];

            if (_precision > 18)
            {
                throw new NotSupportedException("This implementation of DecimalWriter does not support precision greater than 18 digits (2^63)");
            }
        }


        public int AddBlock(ReadOnlyMemory<decimal> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            DecimalWriterStatistics? stats = new();
            Statistics.Add(stats);
            int presentValueCount = WritePresentMaps(stats, presentMaps, valueCount);
            _dataBuffer.AnnotatePosition(stats);
            _secondaryBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);

            List<long> wholePartsList = new(presentValueCount);
            long[] scaleList = new long[presentValueCount];
            int valueId = 0;
            var valuesSpan = values.Span;
            for (int index = 0; index < presentValueCount; index++)
            {
                decimal value = valuesSpan[index];
                stats.AddValue(value);
                Tuple<long, byte>? longAndScale = value.ToLongAndScale();
                Tuple<long, byte>? rescaled = longAndScale.Rescale(_scale, truncateIfNecessary: false);
                rescaled.Item1.CheckPrecision(_precision);
                wholePartsList.Add(rescaled.Item1);
                scaleList[valueId++] = rescaled.Item2;
            }

            //VarIntWriter? varIntEncoder = new(_dataBuffer);
            VarIntWriter.Write(_dataBuffer, wholePartsList);

            //IntegerRunLengthEncodingV2Writer? scaleEncoder = new(_secondaryBuffer);
            IntegerRunLengthEncodingV2Writer.Write(_secondaryBuffer, new ReadOnlySpan<long>(scaleList, 0, valueId), true, _shouldAlignEncodedValues);
            return presentValueCount;
        }
    }
}