namespace INLINQ.Orc.Infrastructure
{
    public static class DecimalExtensions
    {
        private static long[] _scaleFactors =
		{
			1L,
			10L,
			100L,
			1000L,
			10000L,
			100000L,
			1000000L,
			10000000L,
			100000000L,
			1000000000L,
			10000000000L,
			100000000000L,
			1000000000000L,
			10000000000000L,
			100000000000000L,
			1000000000000000L,
			10000000000000000L,
			100000000000000000L,
			1000000000000000000L
		};

        private static int GetPrecision(long v)
        {
            if (v >= 1000000000000000000L)
            {
                return 19;
            }

            if (v >= 100000000000000000L)
            {
                return 18;
            }

            if (v >= 10000000000000000L)
            {
                return 17;
            }

            if (v >= 1000000000000000L)
            {
                return 16;
            }

            if (v >= 100000000000000L)
            {
                return 15;
            }

            if (v >= 10000000000000L)
            {
                return 14;
            }

            if (v >= 1000000000000L)
            {
                return 13;
            }

            if (v >= 100000000000L)
            {
                return 12;
            }

            if (v >= 10000000000L)
            {
                return 11;
            }

            if (v >= 1000000000L)
            {
                return 10;
            }

            if (v >= 100000000L)
            {
                return 9;
            }

            if (v >= 10000000L)
            {
                return 8;
            }

            if (v >= 1000000L)
            {
                return 7;
            }

            if (v >= 100000L)
            {
                return 6;
            }

            if (v >= 10000L)
            {
                return 5;
            }

            if (v >= 1000L)
            {
                return 4;
            }

            if (v >= 100L)
            {
                return 3;
            }

            if (v >= 10L)
            {
                return 2;
            }

            return 1;
        }

		public static Tuple<long,byte> ToLongAndScale(this decimal value)
		{
            int[]? bits = decimal.GetBits(value);
			if (bits[2] != 0 || (bits[1] & 0x80000000) != 0)
            {
                throw new OverflowException("Attempted to convert a decimal with greater than 63 bits of precision to a long");
            }

            ulong high = (ulong)(uint)bits[1] << 32;
            uint low = (uint)bits[0];
			long m = (long)high | low;
            byte e = (byte)((bits[3] >> 16) & 0x7F);
            bool isNeg = (bits[3] & 0x80000000) != 0;
			if (isNeg)
            {
                m = -m;
            }

            return Tuple.Create(m, e);
		}

		public static decimal ToDecimal(this Tuple<long, byte> value)
		{
            long m = value.Item1;
            byte e = value.Item2;
            bool isNeg = m < 0;
			if (isNeg)
            {
                m = -m;
            }

            return new decimal((int)m, (int)(m >> 32), 0, isNeg, e);
		}

		public static Tuple<long, byte> Rescale(this Tuple<long, byte> value, int desiredScale, bool truncateIfNecessary)
		{
            long m = value.Item1;
            byte e = value.Item2;

			if (e == desiredScale)
            {
                return value;
            }
            else if (desiredScale > e)
			{
                int scaleAdjustment = desiredScale - e;
				checked
				{
                    //Throw if we overflow a long here
                    long newM = m * _scaleFactors[scaleAdjustment];
                    byte newE = (byte)(e + scaleAdjustment);
					return Tuple.Create(newM, newE);
				}
			}
			else
			{
                int scaleAdjustment = e - desiredScale;
                long newM = m / _scaleFactors[scaleAdjustment];
                byte newE = (byte)(e - scaleAdjustment);
				if (!truncateIfNecessary)
                {
                    if (newM * _scaleFactors[scaleAdjustment] != m)     //We lost information in the scaling
                    {
                        throw new ArithmeticException($"Scaling would have rounded: m={m} e={e} desiredScale={desiredScale} newM={newM} newE={newE} {newM * _scaleFactors[scaleAdjustment]}!={m}");
                    }
                }

                return Tuple.Create(newM, newE);
			}
		}

        public static void CheckPrecision(this long value, int maxPrecision)
        {
            int precision = GetPrecision(value);
            if (precision > maxPrecision)
            {
                throw new OverflowException($"Attempted to serialize a decimal with higher precision than configured. value={value} precision={precision} maxPrecision={maxPrecision}");
            }
        }
    }
}
