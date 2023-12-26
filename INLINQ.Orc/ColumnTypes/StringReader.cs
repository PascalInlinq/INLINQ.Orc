using INLINQ.Orc.Stripes;
using System.Text;

namespace INLINQ.Orc.ColumnTypes
{
    public static class StringReader
    {
        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, string[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            Protocol.ColumnEncodingKind? kind = ColumnReader.GetColumnEncodingKind(stripeStreams, columnId, Protocol.StreamKind.Data);
            int stringOffset = 0;
            long[] length = new long[stripeStreams.NumRows];
            int valueId = 0;
            switch (kind)
            {
                case Protocol.ColumnEncodingKind.DirectV2:
                    byte[]? directData = ColumnReader.ReadBinaryStream(stripeStreams, columnId, Protocol.StreamKind.Data);
                    ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Length, false, length);
                    if (directData == null)
                    {
                        throw new InvalidDataException("DATA and LENGTH streams must be available");
                    }

                    foreach (long len in length)
                    {
                        string? value = Encoding.UTF8.GetString(directData, stringOffset, (int)len);
                        stringOffset += (int)len;
                        column[valueId++] = value;
                    }
                    break;
                case Protocol.ColumnEncodingKind.DictionaryV2:
                    long[] data = new long[stripeStreams.NumRows];
                    ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Data, false, data);
                    byte[]? dictionaryData = ColumnReader.ReadBinaryStream(stripeStreams, columnId, Protocol.StreamKind.DictionaryData);
                    ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Length, false, length);
                    if (data == null || dictionaryData == null || length == null)
                    {
                        throw new InvalidDataException("DATA, DICTIONARY_DATA, and LENGTH streams must be available");
                    }

                    List<string>? dictionary = new();
                    foreach (long len in length)
                    {
                        string? dictionaryValue = Encoding.UTF8.GetString(dictionaryData, stringOffset, (int)len);
                        stringOffset += (int)len;
                        dictionary.Add(dictionaryValue);
                    }

                    foreach (long value in data)
                    {
                        column[valueId++] = dictionary[(int)value];
                    }
                    break;
                default: throw new NotImplementedException($"Unsupported column encoding {kind}");
            }

            return presentLength > 0;
        }

        public static IEnumerable<string?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            string[] column = new string[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            //return ColumnReader.Read(presentMaps, column, hasPresentMap);
            int count = column.Length;
            if (hasPresentMap)
            {
                int valueId = 0;
                int mapId = 0;
                byte currentMap = 0;
                for (int i = 0; i < count; i++)
                {
                    if (((byte)i & 7) == 0)
                    {
                        currentMap = presentMaps[mapId++];
                    }

                    if (currentMap >= 0x80)
                    {
                        yield return column[valueId++];
                    }
                    else
                    {
                        yield return default;
                    }
                    currentMap <<= 1;
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    yield return column[i];
                }
            }
        }
    }
}
