using INLINQ.Orc.Compression;
using INLINQ.Orc.Encodings;
using INLINQ.Orc.Protocol;
using System.Text;

namespace INLINQ.Orc.ColumnTypes
{
    public class StringWriter : NullableWriter, IColumnWriter
    {
        private readonly bool _shouldAlignLengths;
        private readonly bool _shouldAlignDictionaryLookup;
        private readonly double _uniqueStringThresholdRatio;
        private readonly long _strideLength;
        private readonly OrcCompressedBuffer _presentBuffer;
        private readonly OrcCompressedBuffer _lengthBuffer;
        private readonly OrcCompressedBuffer _dictionaryDataBuffer;
        private readonly Dictionary<string, DictionaryEntry> _unsortedDictionary = new();
        private readonly List<DictionaryEntry?> _dictionaryLookupValues = new();


        public StringWriter(bool shouldAlignLengths, bool shouldAlignDictionaryLookup, double uniqueStringThresholdRatio, long strideLength
            , OrcCompressedBufferFactory bufferFactory, uint columnId)
            : base(true, bufferFactory, columnId, ColumnEncodingKind.Direct, StreamKind.Length, StreamKind.DictionaryData)
        {
            _shouldAlignLengths = shouldAlignLengths;
            _shouldAlignDictionaryLookup = shouldAlignDictionaryLookup;
            _uniqueStringThresholdRatio = uniqueStringThresholdRatio;
            _strideLength = strideLength;
            //ColumnId = columnId;

            _presentBuffer = base.Buffers[0];
            _lengthBuffer = base.Buffers[2];
            _dictionaryDataBuffer = base.Buffers[3];
        }

        //public List<IStatistics> Statistics { get; } = new List<IStatistics>();
        public new long CompressedLength
        {
            get
            {
                if (!ColumnEncodingIsValid())
                {
                    return 0;                                       //We haven't decided on an encoding yet
                }
                else if (ColumnEncoding == ColumnEncodingKind.DirectV2)
                {
                    return Buffers.Sum(s => s.Length);              //We encode these as we go.  The buffer lengths are valid
                }
                else if (ColumnEncoding == ColumnEncodingKind.DictionaryV2)
                {
                    //Dictionary encoding doesn't flush to the buffers until a stripe is complete, but we don't know a stripe is complete without a sense of its compressed length
                    if (_dictionaryDataBuffer.Length != 0)
                    {
                        return Buffers.Sum(s => s.Length);          //The stripe is complete, we've flushed data, return the true size
                    }
                    else
                    {
                        return _dictionaryLookupValues.Count * 2;   //Make a wild approximation about how much data storage will be required for X values
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }
        //public uint ColumnId { get; }
        public new OrcCompressedBuffer[] Buffers
        {
            get
            {
                switch (ColumnEncoding)
                {
                    case ColumnEncodingKind.DirectV2:
                        return new[] { base.Buffers[0], base.Buffers[1], base.Buffers[2] };
                    case ColumnEncodingKind.DictionaryV2:
                        return base.Buffers;
                    default:
                        throw new NotSupportedException($"Only DirectV2 and DictionaryV2 encodings are supported for {nameof(StringWriter)}");
                }
            }
        }
        public uint DictionaryLength => (uint)_unsortedDictionary.Count;

        private bool ColumnEncodingIsValid() => ColumnEncoding == ColumnEncodingKind.DictionaryV2 || ColumnEncoding == ColumnEncodingKind.DirectV2;

        private void EnsureEncodingKindIsSet(ReadOnlyMemory<string> values)
        {
            if (ColumnEncodingIsValid())
            {
                return;
            }

            //Detect the encoding type
            int uniqueValues = values.ToArray().Distinct().Count(); //TODO: inefficient
            int totalValues = values.Length;
            if (uniqueValues / (double)totalValues <= _uniqueStringThresholdRatio)
            {
                ColumnEncoding = ColumnEncodingKind.DictionaryV2;
            }
            else
            {
                ColumnEncoding = ColumnEncodingKind.DirectV2;
            }
        }

        public new void FlushBuffers()
        {
            if (ColumnEncoding == ColumnEncodingKind.DictionaryV2)
            {
                WriteDictionaryEncodedData();
            }

            base.FlushBuffers();

            //foreach (OrcCompressedBuffer buffer in Buffers)
            //{
            //    buffer.Flush();
            //}
        }

        public new void Reset()
        {
            _unsortedDictionary.Clear();
            _dictionaryLookupValues.Clear();
            base.Reset();
        }

        public int AddBlock(ReadOnlyMemory<string> values, ReadOnlyMemory<ulong> presentMaps, int valueCount)
        {
            EnsureEncodingKindIsSet(values);
            var presentMapsSpan = presentMaps.Span;

            int presentIndex = 0;
            ulong presentMap = 0;
            int valueId = 0;
            var valuesSpan = values.Span;
            if (ColumnEncoding == ColumnEncodingKind.DirectV2)
            {
                StringWriterStatistics? stats = new();
                Statistics.Add(stats);
                _presentBuffer.AnnotatePosition(stats, rleValuesToConsume: 0, bitsToConsume: 0);
                _dataBuffer.AnnotatePosition(stats);
                _lengthBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);

                byte[][] bytesList = new byte[valueCount][];
                bool[] presentList = new bool[valueCount];
                long[] lengthList = new long[valueCount];
                int presentCount = 0;
                for (int index = 0; index < valueCount; index++)
                {
                    if ((index & 63) == 0)
                    {
                        presentMap = presentMapsSpan[presentIndex++];
                    }

                    if (presentMap >= 0x8000000000000000)
                    {
                        //is present:
                        string str = valuesSpan[valueId];
                        stats.AddValue(str);
                        presentList[index] = true;
                        byte[]? bytes = Encoding.UTF8.GetBytes(str);
                        bytesList[presentCount++] = bytes;
                        lengthList[valueId++] = bytes.Length;
                    }
                    else
                    {
                        //is not present:
                        stats.AddValue(null);
                        presentList[index] = false;
                    }

                    presentMap <<= 1;
                }

                BitWriter? presentEncoder = new(_presentBuffer);
                presentEncoder.Write(presentList);
                if (stats.HasNull)
                {
                    _presentBuffer.MustBeIncluded = true;
                }

                _dataBuffer.WriteMany(bytesList, presentCount);

                IntegerRunLengthEncodingV2Writer.Write(_lengthBuffer, new ReadOnlySpan<long>(lengthList, 0, valueId), false, _shouldAlignLengths);
            }
            else if (ColumnEncoding == ColumnEncodingKind.DictionaryV2)
            {
                for (int index = 0; index < valueCount; index++)
                {
                    if ((index & 63) == 0)
                    {
                        presentMap = presentMapsSpan[presentIndex++];
                    }

                    if (presentMap >= 0x8000000000000000)
                    {
                        //is present:
                        string value = valuesSpan[valueId++];
                        if (!_unsortedDictionary.TryGetValue(value, out DictionaryEntry? entry))
                        {
                            entry = new DictionaryEntry();
                            _unsortedDictionary.Add(value, entry);
                        }
                        _dictionaryLookupValues.Add(entry);
                    }
                    else
                    {
                        //is not present:
                        _dictionaryLookupValues.Add(null);
                    }

                    presentMap <<= 1;
                }
            }
            else
            {
                throw new ArgumentException("StringWriter.ColumnEncoding");
            }
            return valueId;
        }

        private void WriteDictionaryEncodedData()
        {
            StringWriterStatistics? stats = new();
            Statistics.Add(stats);
            _presentBuffer.AnnotatePosition(stats, rleValuesToConsume: 0, bitsToConsume: 0);
            _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);

            //Sort the dictionary
            List<string>? sortedDictionary = new();
            int i = 0;
            foreach (KeyValuePair<string, DictionaryEntry> dictEntry in _unsortedDictionary.OrderBy(d => d.Key, StringComparer.Ordinal))
            {
                sortedDictionary.Add(dictEntry.Key);
                dictEntry.Value.Id = i++;
            }

            //Write the dictionary
            long[] dictionaryLengthList = new long[sortedDictionary.Count];
            int valueId = 0;
            foreach (string? dictEntry in sortedDictionary)
            {
                byte[]? bytes = Encoding.UTF8.GetBytes(dictEntry);
                dictionaryLengthList[valueId++] = bytes.Length;                 //Save the length
                _dictionaryDataBuffer.Write(bytes, 0, bytes.Length);    //Write to the buffer
            }

            //Write the dictionary lengths
            IntegerRunLengthEncodingV2Writer.Write(_lengthBuffer, dictionaryLengthList, false, _shouldAlignLengths);

            //Write the lookup values
            bool[] presentList = new bool[_dictionaryLookupValues.Count];
            long[] lookupList = new long[_dictionaryLookupValues.Count];
            int lookupId = 0;
            int count = 0;
            int presentId = 0;
            var presentSpan = new Span<bool>(presentList);
            foreach (DictionaryEntry? value in _dictionaryLookupValues)
            {
                if (value == null)
                {
                    stats.AddValue(null);
                    presentList[presentId] =false;
                }
                else
                {
                    string? stringValue = sortedDictionary[value.Id];   //Look up the string value for this Id so we can notate statistics
                    stats.AddValue(stringValue);
                    presentList[presentId] =true;
                    lookupList[lookupId++] = value.Id;
                }

                count++;
                presentId++;

                if (count % _strideLength == 0 || count == _dictionaryLookupValues.Count)                  //If it's time for new statistics
                {
                    //Flush to the buffers
                    BitWriter? presentEncoder = new(_presentBuffer);
                    presentEncoder.Write(presentSpan.Slice(0, presentId));
                    presentId = 0;
                    if (stats.HasNull)
                    {
                        _presentBuffer.MustBeIncluded = true;
                    }

                    IntegerRunLengthEncodingV2Writer.Write(_dataBuffer, new ReadOnlySpan<long>(lookupList, 0, lookupId), false, _shouldAlignDictionaryLookup);
                    lookupId = 0;

                    if (count != _dictionaryLookupValues.Count)     //More values remain
                    {
                        stats = new StringWriterStatistics();
                        Statistics.Add(stats);
                        _presentBuffer.AnnotatePosition(stats, rleValuesToConsume: 0, bitsToConsume: 0);
                        _dataBuffer.AnnotatePosition(stats, rleValuesToConsume: 0);
                    }
                }
            }
        }

        private class DictionaryEntry
        {
            public int Id { get; set; }
        }
    }
}