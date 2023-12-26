using INLINQ.Orc.Stripes;
using System.Numerics;

namespace INLINQ.Orc.ColumnTypes
{
    public static class DecimalReader
    {
        private static decimal FromBigInteger(BigInteger numerator, long scale)
        {
            if (scale < 0 || scale > 255)
            {
                throw new OverflowException("Scale must be positive number");
            }

            decimal decNumerator = (decimal)numerator;      //This will throw for an overflow or underflow
            decimal scaler = new(1, 0, 0, false, (byte)scale);

            return decNumerator * scaler;
        }

        public static bool ReadAll(StripeStreamReaderCollection stripeStreams, uint columnId, byte[] presentMaps, decimal[] column)
        {
            uint presentLength = ColumnReader.ReadBooleanStreamToPresentMap(stripeStreams, columnId, Protocol.StreamKind.Present, presentMaps);
            BigInteger[]? data = ColumnReader.ReadVarIntStream(stripeStreams, columnId, Protocol.StreamKind.Data);
            if (data == null)
            {
                throw new InvalidDataException("DATA and SECONDARY streams must be available");
            }

            long[] secondary = new long[data.Length];
            ColumnReader.ReadNumericStreamToArray(stripeStreams, columnId, Protocol.StreamKind.Secondary, true, secondary);
            if (data == null || secondary == null)
            {
                throw new InvalidDataException("DATA and SECONDARY streams must be available");
            }
            for (uint index = 0; index < data.Length; index++)
            {
                decimal value = FromBigInteger(data[index], secondary[index]);
                column[index] = value;
            }
            return presentLength > 0;
        }

        public static IEnumerable<decimal?> Read(StripeStreamReaderCollection stripeStreams, uint columnId)
        {
            byte[] presentMaps = new byte[(stripeStreams.NumRows + 7) / 8];
            decimal[] column = new decimal[stripeStreams.NumRows];
            bool hasPresentMap = ReadAll(stripeStreams, columnId, presentMaps, column);
            return ColumnReader.Read(presentMaps, column, hasPresentMap);
        }
    }
}
