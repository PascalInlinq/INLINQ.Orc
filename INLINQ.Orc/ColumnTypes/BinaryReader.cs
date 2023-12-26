using INLINQ.Orc.Stripes;

namespace INLINQ.Orc.ColumnTypes
{
    public static class BinaryReader
    {
        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, byte[][] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            byte[]? data = ColumnReader.ReadBinaryStream(stripeStreams, columnId, Protocol.StreamKind.Data);
            long[] length = new long[data == null ? 0 : data.Length];
            ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Length, false, length);
            if (data == null)
            {
                throw new InvalidDataException("DATA and LENGTH streams must be available");
            }

            int byteOffset = 0;
            for(int valueId = 0; valueId < column.Length; valueId++)
            {
                long len = length[valueId];
                byte[]? bytes = new byte[len];
                Buffer.BlockCopy(data, byteOffset, bytes, 0, (int)len);
                byteOffset += (int)len;
                column[valueId] = bytes;
            }

            return presentLength > 0;
        }

        public static IEnumerable<byte[]?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            byte[][] column = new byte[stripeStreams.NumRows][];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
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
