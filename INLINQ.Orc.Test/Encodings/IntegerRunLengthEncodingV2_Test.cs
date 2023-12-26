
namespace INLINQ.Orc.Test.Encodings
{
    public class IntegerRunLengthEncodingV2Test
    {
        //[Fact]
        //public void ReadWriteShortRepeat()
        //{
        //    long[] longs = new long[] { 10000, 10000, 10000, 10000, 10000 };
        //    byte[] bytes = new byte[] { 0x0a, 0x27, 0x10 };
        //    TestRead(longs, bytes, false);
        //    TestWrite(bytes, longs, false, false);
        //}

        //[Fact]
        //public void RoundTripShortRepeat()
        //{
        //    TestRoundTrip(new long[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, false, false);
        //    TestRoundTrip(new long[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }, true, false);
        //    TestRoundTrip(new long[] { 0xffff, 0xffff, 0xffff, 0xffff }, false, false);
        //    TestRoundTrip(new long[] { 0xffffff, 0xffffff, 0xffffff, 0xffffff }, false, false);
        //    TestRoundTrip(new long[] { 0xffffffff, 0xffffffff, 0xffffffff, 0xffffffff }, false, false);
        //    TestRoundTrip(new long[] { 0x11223344, 0x11223344, 0x11223344, 0x11223344 }, false, false);
        //    TestRoundTrip(new long[] { 0x1122334455, 0x1122334455, 0x1122334455, 0x1122334455 }, false, false);
        //    TestRoundTrip(new long[] { 0x112233445566, 0x112233445566, 0x112233445566, 0x112233445566 }, false, false);
        //    TestRoundTrip(new long[] { 0x11223344556677, 0x11223344556677, 0x11223344556677, 0x11223344556677 }, false, false);
        //    TestRoundTrip(new long[] { 0x1122334455667788, 0x1122334455667788, 0x1122334455667788, 0x1122334455667788 }, false, false);
        //}

        //[Fact]
        //public void ReadWriteDirect()
        //{
        //    long[] longs = new long[] { 23713, 43806, 57005, 48879 };
        //    byte[] bytes = new byte[] { 0x5e, 0x03, 0x5c, 0xa1, 0xab, 0x1e, 0xde, 0xad, 0xbe, 0xef };
        //    TestRead(longs, bytes, false);
        //    TestWrite(bytes, longs, false, false);
        //}

        //[Fact]
        //public void RoundTripDirect()
        //{
        //    long[] longs = new long[] { -5, 5, -5, 5 };
        //    TestRoundTrip(longs, true, false);
        //    TestRoundTrip(longs, true, true);
        //}

        //[Fact]
        //public void ReadWritePatchedBase()
        //{
        //    long[] longs = new long[] { 2030, 2000, 2020, 1000000, 2040, 2050, 2060, 2070, 2080, 2090, 2100, 2110, 2120, 2130, 2140, 2150, 2160, 2170, 2180, 2190 };
        //    byte[] bytes = new byte[] { 0x8e, 0x13, 0x2b, 0x21, 0x07, 0xd0, 0x1e, 0x00, 0x14, 0x70, 0x28, 0x32, 0x3c, 0x46, 0x50, 0x5a, 0x64, 0x6e, 0x78, 0x82, 0x8c, 0x96, 0xa0, 0xaa, 0xb4, 0xbe, 0xfc, 0xe8 };
        //    TestRead(longs, bytes, false);
        //    TestWrite(bytes, longs, false, false);
        //}

        //[Fact]
        //public void ReadWriteDelta()
        //{
        //    long[] longs = new long[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
        //    byte[] bytes = new byte[] { 0xc6, 0x09, 0x02, 0x02, 0x22, 0x42, 0x42, 0x46 };
        //    TestRead(longs, bytes, false);
        //    TestWrite(bytes, longs, false, true);
        //}

        //[Fact]
        //public void ReadWriteDelta2()
        //{
        //    long[] longs = new long[0x120];
        //    for (int i = 0; i < longs.Length; i++)
        //    {
        //        longs[i] = 0x6;
        //    }

        //    byte[] bytes = new byte[] { 0xc1, 0x1f, 0x0c, 0x00 };
        //    TestRead(longs, bytes, true);
        //    TestWrite(bytes, longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaRepeatingUnsigned()
        //{
        //    long[] longs = new long[] { 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000 };
        //    TestRoundTrip(longs, false, true);
        //    TestRoundTrip(longs, false, false);
        //}

        //[Fact]
        //public void RoundTripDeltaRepeatingSignedPositive()
        //{
        //    long[] longs = new long[] { 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000, 10000 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaRepeatingSignedNegative()
        //{
        //    long[] longs = new long[] { -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000, -10000 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaFixedOffsetPositive()
        //{
        //    long[] longs = new long[] { 10000, 30000, 50000, 70000, 90000, 110000 };
        //    TestRoundTrip(longs, false, true);
        //    TestRoundTrip(longs, false, false);
        //}

        //[Fact]
        //public void RoundTripDeltaFixedOffsetNegativeBasePositiveDelta()
        //{
        //    long[] longs = new long[] { -10000, 10000, 30000, 50000, 70000, 90000 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaFixedOffsetNegativeBaseNegativeDelta()
        //{
        //    long[] longs = new long[] { -10000, -30000, -50000, -70000, -90000, -110000 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaFixedOffsetPositiveBaseNegativeDelta()
        //{
        //    long[] longs = new long[] { 10000, -10000, -30000, -50000, -70000, -90000 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaVariedOffsetPositive()
        //{
        //    long[] longs = new long[] { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };
        //    TestRoundTrip(longs, false, true);
        //    TestRoundTrip(longs, false, false);
        //}

        //[Fact]
        //public void RoundTripDeltaVariedOffsetNegative()
        //{
        //    long[] longs = new long[] { -1, -2, -4, -8, -16, -32, -64, -128, -256, -512, -1024, -2048, -4096 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaSingleNumberThenLotsOfADifferentNumber()
        //{
        //    List<long> longs = new();
        //    longs.Add(8);
        //    for (int i = 0; i < 511; i++)
        //    {
        //        longs.Add(22);
        //    }

        //    long[] longArray = longs.ToArray();
        //    TestRoundTrip(longArray, false, true);
        //    TestRoundTrip(longArray, false, false);
        //}


        //[Fact]
        //public void RoundTripDeltaOneBitRequired()
        //{
        //    long[] longs = new long[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaIncreasingOnly()
        //{
        //    long[] longs = new long[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaDecreasingOnly()
        //{
        //    long[] longs = new long[] { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaMonotonicDecrease()
        //{
        //    long[] longs = new long[] { 2, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaNonMonotonicDecrease()
        //{
        //    long[] longs = new long[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaMonotonicIncrease()
        //{
        //    long[] longs = new long[] { 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaNonMonotonicIncrease()
        //{
        //    long[] longs = new long[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaBigFirstDelta()
        //{
        //    long[] longs = new long[] { 0, 99, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };
        //    TestRoundTrip(longs, true, true);
        //    TestRoundTrip(longs, true, false);
        //}

        //#region From Java Source
        //[Fact]
        //public void RoundTripFixedDeltaZero()
        //{
        //    List<long> longs = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(123);
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 50);
        //}

        //[Fact]
        //public void RoundTripFixedDeltaOne()
        //{
        //    List<long> longs = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(i % 512);
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 40);
        //}

        //[Fact]
        //public void RoundTripFixedDeltaOneDescending()
        //{
        //    List<long> longs = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(512 - (i % 512));
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 50);
        //}

        //[Fact]
        //public void RoundTripFixedDeltaLarge()
        //{
        //    List<long> longs = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(i % 512 + (i % 512 * 100));
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 50);
        //}

        //[Fact]
        //public void RoundTripFixedDeltaLargeDescending()
        //{
        //    List<long> longs = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(512 - i % 512 + (i % 512 * 100));
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 60);
        //}

        //[Fact]
        //public void RoundTripShortRepeatB()
        //{
        //    List<long> longs = new();
        //    for (int i = 0; i < 5; i++)
        //    {
        //        longs.Add(10);
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 2);
        //}

        //[Fact]
        //public void RoundTripUnknownSign()
        //{
        //    List<long> longs = new();
        //    longs.Add(0);
        //    for (int i = 0; i < 511; i++)
        //    {
        //        longs.Add(i);
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 642);
        //}

        //[Fact]
        //public void RoundTripPatchedBase()
        //{
        //    List<long> longs = new();
        //    Random random = new(123);
        //    longs.Add(10000000);
        //    for (int i = 0; i < 511; i++)
        //    {
        //        longs.Add(random.Next() % (i + 1));
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false, 583);
        //}

        //[Fact]
        //public void RoundTripBasicNew()
        //{
        //    long[] longs = new long[] {
        //        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 3, 4, 5, 6,
        //        7, 8, 9, 10, 1, 1, 1, 1, 1, 1, 10, 9, 7, 6, 5,
        //        4, 3, 2, 1, 1, 1, 1, 1, 2, 5, 1, 3, 7, 1, 9, 2,
        //        6, 3, 7, 1, 9, 2, 6, 3, 7, 1, 9, 2, 6, 3, 7, 1,
        //        9, 2, 6, 3, 7, 1, 9, 2, 6, 2000, 2, 1, 1, 1, 1,
        //        1, 3, 7, 1, 9, 2, 6, 1, 1, 1, 1, 1};

        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripBasicDelta1()
        //{
        //    long[] longs = new long[] { -500, -400, -350, -325, -310 };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripBasicDelta2()
        //{
        //    long[] longs = new long[] { -500, -600, -650, -675, -710 };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripBasicDelta3()
        //{
        //    long[] longs = new long[] { 500, 400, 350, 325, 310 };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripBasicDelta4()
        //{
        //    long[] longs = new long[] { 500, 600, 650, 675, 710 };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaOverflow()
        //{
        //    long[] longs = new long[] { 4513343538618202719L, 4513343538618202711L, 2911390882471569739L, -9181829309989854913L };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaOverflow2()
        //{
        //    long[] longs = new long[] { long.MaxValue, 4513343538618202711L, 2911390882471569739L, long.MinValue };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripDeltaOverflow3()
        //{
        //    long[] longs = new long[] { -4513343538618202711L, -2911390882471569739L, -2, long.MaxValue };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripIntegerMin()
        //{
        //    long[] longs = new long[] { int.MinValue };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripIntegerMax()
        //{
        //    long[] longs = new long[] { int.MaxValue };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripLongMin()
        //{
        //    long[] longs = new long[] { long.MinValue };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripLongMax()
        //{
        //    long[] longs = new long[] { long.MaxValue };
        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripRandomInt()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 100000; i++)
        //    {
        //        longs.Add(random.Next());
        //    }

        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripRandomLong()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 100000; i++)
        //    {
        //        byte[] bytes = new byte[8];
        //        random.NextBytes(bytes);
        //        longs.Add(BitConverter.ToInt64(bytes, 0));
        //    }
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseNegativeMin()
        //{
        //    long[] longs = new long[] {
        //        20, 2, 3, 2, 1, 3, 17, 71, 35, 2, 1, 139, 2, 2, 3, 1783, 475, 2, 1,
        //        1, 3, 1, 3, 2, 32, 1, 2, 3, 1, 8, 30, 1, 3, 414, 1, 1, 135, 3, 3, 1,
        //        414, 2, 1, 2, 2, 594, 2, 5, 6, 4, 11, 1, 2, 2, 1, 1, 52, 4, 1, 2, 7,
        //        1, 17, 334, 1, 2, 1, 2, 2, 6, 1, 266, 1, 2, 217, 2, 6, 2, 13, 2, 2,
        //        1, 2, 3, 5, 1, 2, 1, 7244, 11813, 1, 33, 2,
        //        -13,
        //        1, 2, 3, 13, 1, 92, 3, 13, 5, 14, 9, 141, 12, 6, 15, 25, 1, 1, 1, 46,
        //        2, 1, 1, 141, 3, 1, 1, 1, 1, 2, 1, 4, 34, 5, 78, 8, 1, 2, 2, 1, 9, 10,
        //        2, 1, 4, 13, 1, 5, 4, 4, 19, 5, 1, 1, 1, 68, 33, 399, 1, 1885, 25, 5,
        //        2, 4, 1, 1, 2, 16, 1, 2966, 3, 1, 1, 25501, 1, 1, 1, 66, 1, 3, 8, 131,
        //        14, 5, 1, 2, 2, 1, 1, 8, 1, 1, 2, 1, 5, 9, 2, 3, 112, 13, 2, 2, 1, 5,
        //        10, 3, 1, 1, 13, 2, 3, 4, 1, 3, 1, 1, 2, 1, 1, 2, 4, 2, 207, 1, 1, 2,
        //        4, 3, 3, 2, 2, 16 };

        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseNegativeMin2()
        //{
        //    long[] longs = new long[] {
        //        20, 2, 3, 2, 1, 3, 17, 71, 35, 2, 1, 139, 2, 2, 3, 1783, 475, 2, 1,
        //        1, 3, 1, 3, 2, 32, 1, 2, 3, 1, 8, 30, 1, 3, 414, 1, 1, 135, 3, 3, 1,
        //        414, 2, 1, 2, 2, 594, 2, 5, 6, 4, 11, 1, 2, 2, 1, 1, 52, 4, 1, 2, 7,
        //        1, 17, 334, 1, 2, 1, 2, 2, 6, 1, 266, 1, 2, 217, 2, 6, 2, 13, 2, 2,
        //        1, 2, 3, 5, 1, 2, 1, 7244, 11813, 1, 33, 2,
        //        -1,
        //        1, 2, 3, 13, 1, 92, 3, 13, 5, 14, 9, 141, 12, 6, 15, 25, 1, 1, 1, 46,
        //        2, 1, 1, 141, 3, 1, 1, 1, 1, 2, 1, 4, 34, 5, 78, 8, 1, 2, 2, 1, 9, 10,
        //        2, 1, 4, 13, 1, 5, 4, 4, 19, 5, 1, 1, 1, 68, 33, 399, 1, 1885, 25, 5,
        //        2, 4, 1, 1, 2, 16, 1, 2966, 3, 1, 1, 25501, 1, 1, 1, 66, 1, 3, 8, 131,
        //        14, 5, 1, 2, 2, 1, 1, 8, 1, 1, 2, 1, 5, 9, 2, 3, 112, 13, 2, 2, 1, 5,
        //        10, 3, 1, 1, 13, 2, 3, 4, 1, 3, 1, 1, 2, 1, 1, 2, 4, 2, 207, 1, 1, 2,
        //        4, 3, 3, 2, 2, 16 };


        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseNegativeMin3()
        //{
        //    long[] longs = new long[] {
        //        20, 2, 3, 2, 1, 3, 17, 71, 35, 2, 1, 139, 2, 2, 3, 1783, 475, 2, 1,
        //        1, 3, 1, 3, 2, 32, 1, 2, 3, 1, 8, 30, 1, 3, 414, 1, 1, 135, 3, 3, 1,
        //        414, 2, 1, 2, 2, 594, 2, 5, 6, 4, 11, 1, 2, 2, 1, 1, 52, 4, 1, 2, 7,
        //        1, 17, 334, 1, 2, 1, 2, 2, 6, 1, 266, 1, 2, 217, 2, 6, 2, 13, 2, 2,
        //        1, 2, 3, 5, 1, 2, 1, 7244, 11813, 1, 33, 2,
        //        0,
        //        1, 2, 3, 13, 1, 92, 3, 13, 5, 14, 9, 141, 12, 6, 15, 25, 1, 1, 1, 46,
        //        2, 1, 1, 141, 3, 1, 1, 1, 1, 2, 1, 4, 34, 5, 78, 8, 1, 2, 2, 1, 9, 10,
        //        2, 1, 4, 13, 1, 5, 4, 4, 19, 5, 1, 1, 1, 68, 33, 399, 1, 1885, 25, 5,
        //        2, 4, 1, 1, 2, 16, 1, 2966, 3, 1, 1, 25501, 1, 1, 1, 66, 1, 3, 8, 131,
        //        14, 5, 1, 2, 2, 1, 1, 8, 1, 1, 2, 1, 5, 9, 2, 3, 112, 13, 2, 2, 1, 5,
        //        10, 3, 1, 1, 13, 2, 3, 4, 1, 3, 1, 1, 2, 1, 1, 2, 4, 2, 207, 1, 1, 2,
        //        4, 3, 3, 2, 2, 16 };


        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseNegativeMin4()
        //{
        //    long[] longs = new long[] {
        //        13, 13, 11, 8, 13, 10, 10, 11, 11, 14, 11, 7, 13, 12, 12, 11, 15, 12,
        //        12, 9, 8, 10, 13, 11, 8, 6, 5, 6, 11, 7, 15, 10, 7, 6, 8, 7, 9, 9, 11,
        //        33, 11, 3, 7, 4, 6, 10, 14, 12, 5, 14, 7, 6 };

        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseAt0()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 100);
        //    }

        //    longs[0] = 20000;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseAt1()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 100);
        //    }

        //    longs[1] = 20000;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseAt255()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 100);
        //    }

        //    longs[255] = 20000;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseAt256()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 100);
        //    }

        //    longs[256] = 20000;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseAt510()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 100);
        //    }

        //    longs[510] = 20000;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseAt511()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 100);
        //    }

        //    longs[511] = 20000;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseMax1()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 60);
        //    }

        //    longs[511] = long.MaxValue;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseMax2()
        //{
        //    List<long> longs = new();
        //    Random random = new();
        //    for (int i = 0; i < 5120; i++)
        //    {
        //        longs.Add(random.Next() % 60);
        //    }

        //    longs[128] = long.MaxValue;
        //    longs[256] = long.MaxValue;
        //    longs[511] = long.MaxValue;
        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseMax3()
        //{
        //    long[] longs = new long[] {
        //        371946367L, 11963367L, 68639400007L, 100233367L, 6367L, 10026367L,
        //        3670000L, 3602367L, 4719226367L, 7196367L, 444442L, 210267L, 21033L,
        //        160267L, 400267L, 23634347L, 16027L, 46026367L, long.MaxValue, 33333L};

        //    TestRoundTrip(longs, true, false);
        //}

        //[Fact]
        //public void RoundTripPatchedBaseMax4()
        //{
        //    List<long> longs = new();
        //    long[] testSequence = new long[] {
        //        371292224226367L, 119622332222267L, 686329400222007L, 100233333222367L,
        //        636272333322222L, 10202633223267L, 36700222022230L, 36023226224227L,
        //        47192226364427L, 71963622222447L, 22244444222222L, 21220263327442L,
        //        21032233332232L, 16026322232227L, 40022262272212L, 23634342227222L,
        //        16022222222227L, 46026362222227L, 46026362222227L, 33322222222323L };

        //    for (int i = 0; i < 25; i++)
        //    {
        //        longs.AddRange(testSequence);
        //    }

        //    longs.Add(long.MaxValue);

        //    TestRoundTrip(longs.ToArray(), true, false);
        //}

        //[Fact]
        //public void RoundTripDirectLargeNegatives()
        //{
        //    long[] longs = new long[] { -7486502418706614742L, 0L, 1L, 1L, -5535739865598783616L };
        //    TestRoundTrip(longs, true, false);
        //}
        //#endregion

        //private static void TestRead(long[] expected, byte[] input, bool isSigned)
        //{
        //    MemoryStream stream = new(input);
        //    IntegerRunLengthEncodingV2Reader reader = new(stream, isSigned);
        //    long[] actual = reader.Read().ToArray();
        //    Assert.Equal(expected.Length, actual.Length);
        //    for (int i = 0; i < expected.Length; i++)
        //    {
        //        Assert.Equal(expected[i], actual[i]);
        //    }
        //}

        //private static void TestWrite(byte[] expected, long[] input, bool isSigned, bool aligned)
        //{
        //    MemoryStream stream = new();
        //    //IntegerRunLengthEncodingV2Writer writer = new(stream);
        //    IntegerRunLengthEncodingV2Writer.Write(stream, input, isSigned, aligned);
        //    byte[] actual = stream.ToArray();
        //    Assert.Equal(expected.Length, actual.Length);
        //    for (int i = 0; i < expected.Length; i++)
        //    {
        //        Assert.Equal(expected[i], actual[i]);
        //    }
        //}

        //private static void TestRoundTrip(long[] test, bool isSigned, bool aligned, int? expectedEncodeLength = null)
        //{
        //    MemoryStream stream = new();
        //    //IntegerRunLengthEncodingV2Writer writer = new(stream);
        //    IntegerRunLengthEncodingV2Writer.Write(stream, test, isSigned, aligned);

        //    //If we know the encode length, make sure it's correct
        //    if (expectedEncodeLength.HasValue)
        //    {
        //        Assert.Equal(expectedEncodeLength.Value, stream.Length);
        //    }

        //    _ = stream.Seek(0, SeekOrigin.Begin);

        //    IntegerRunLengthEncodingV2Reader reader = new(stream, isSigned);
        //    long[] result = reader.Read().ToArray();

        //    //Make sure all bytes in the written stream were consumed
        //    Assert.Equal(stream.Length, stream.Position);

        //    //Check the actual values
        //    Assert.Equal(test.Length, result.Length);
        //    for (int i = 0; i < test.Length; i++)
        //    {
        //        Assert.Equal(test[i], result[i]);
        //    }
        //}
    }
}
