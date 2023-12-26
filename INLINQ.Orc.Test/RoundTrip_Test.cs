using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace INLINQ.Orc.Test
{
    public class RoundTripTest
    {
        [Fact]
        public void SingleStripeRoundTrip()
        {
            List<RoundTripTestObject> testElements = new();
            Random r = new(123);

            for (int i = 0; i < 10000; i++)
            {
                testElements.Add(RoundTripTestObject.Random(r));
            }

            TestRoundTrip(testElements);
        }

        [Fact]
        public void SingleStripeRoundTripStruct()
        {
            List<RoundTripTestStruct> testElements = new();
            Random random = new(123);
            for (int i = 0; i < 1200000; i++)
            {
                testElements.Add(RoundTripTestStruct.Random(random));
            }

            TestRoundTrip(testElements);
        }


        [Fact]
        public void MultipleStripeRoundTrip()
        {
            List<RoundTripTestObject> testElements = new();
            Random random = new(123);
            for (int i = 0; i < 1200000; i++)
            {
                testElements.Add(RoundTripTestObject.Random(random));
            }

            TestRoundTrip(testElements);
        }

        [Fact]
        public void MultipleStripeRoundTripB()
        {
            List<SinglePropertyObject> testElements = new();
            Random random = new(123);
            for (int i = 0; i < 1200000; i++)
            {
                testElements.Add(SinglePropertyObject.GetNext(random));
            }

            TestRoundTrip(testElements);
        }

        private class SinglePropertyObject
        {
            
            //public int Int { get; set; }
            //public long Long { get; set; }
            //public short Short { get; set; }
            //public uint UInt { get; set; }
            //public ulong ULong { get; set; }
            //public ushort UShort { get; set; }
            //public int? NullableInt { get; set; }
            //public byte Byte { get; set; }
            //public sbyte SByte { get; set; }
            //public bool Bool { get; set; }
            //public float Float { get; set; }
            //public double Double { get; set; }
            //public byte[] ByteArray { get; set; }
            public decimal Decimal { get; set; }
            //public DateTime DateTime { get; set; }
            //public string String { get; set; }

            public static SinglePropertyObject GetNext(Random r)
            {

                SinglePropertyObject result = new()
                {
                    //Int = r.Next(),
                    //Long = r.Next() * 0xFFFFFFL,
                    //Short = (short)r.Next(),
                    //UInt = (uint)r.Next(),
                    //ULong = (ulong)r.Next() * 0xFFFFFFL,
                    //UShort = (ushort)r.Next(),
                    //NullableInt = r.Next() % 10 == 0 ? null : r.Next(),
                    //Byte = (byte)r.Next(),
                    //SByte = (sbyte)r.Next(),
                    //Bool = r.Next() % 2 == 0,
                    //Float = (float)r.NextDouble(),
                    //Double = r.NextDouble(),
                    //ByteArray = new byte[10],
                    Decimal = r.Next() / 1000m,
                    //DateTime = _dateBase.AddSeconds(r.Next()),
                    //String = r.Next().ToString()
                };

                //r.NextBytes(result.ByteArray);
                return result;
            }


            public override bool Equals(object obj)
            {
                return obj is SinglePropertyObject test &&
                                               //Int == test.Int; // &&
                                               //Long == test.Long &&
                                               //                   Short == test.Short &&
                                               //                   UInt == test.UInt &&
                                               //                   ULong == test.ULong &&
                                               //                   UShort == test.UShort &&
                                               //                   NullableInt == test.NullableInt &&
                                               //Byte == test.Byte &&
                                               //       SByte == test.SByte &&
                                               //       Bool == test.Bool &&
                                               //       Float == test.Float &&
                                               //       Double == test.Double &&
                                               //       ByteArray.SequenceEqual(test.ByteArray) &&
                                               Decimal == test.Decimal; //&&
                                        //DateTime == test.DateTime; // &&
                                        //String == test.String;
            }

            public override int GetHashCode()
            {
                int hashCode = 291051517;
                //hashCode = hashCode * -1521134295 + Int.GetHashCode();
                //hashCode = hashCode * -1521134295 + Long.GetHashCode();
                //hashCode = hashCode * -1521134295 + Short.GetHashCode();
                //hashCode = hashCode * -1521134295 + UInt.GetHashCode();
                //hashCode = hashCode * -1521134295 + ULong.GetHashCode();
                //hashCode = hashCode * -1521134295 + UShort.GetHashCode();
                //hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(NullableInt);
                //hashCode = hashCode * -1521134295 + Byte.GetHashCode();
                //hashCode = hashCode * -1521134295 + SByte.GetHashCode();
                //hashCode = hashCode * -1521134295 + Bool.GetHashCode();
                //hashCode = hashCode * -1521134295 + Float.GetHashCode();
                //hashCode = hashCode * -1521134295 + Double.GetHashCode();
                //hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(ByteArray);
                hashCode = hashCode * -1521134295 + Decimal.GetHashCode();
                //hashCode = hashCode * -1521134295 + DateTime.GetHashCode();
                //hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(String);
                return hashCode;
            }
        }

        [Fact]
        public void ColumnsMissing()
        {
            SinglePropertyObject[] testElements = new[]
            {
                new SinglePropertyObject { Decimal = 10 }
            };

            MemoryStream memStream = new();
            using (INLINQ.Orc.OrcWriter<SinglePropertyObject> writer = new(memStream, new INLINQ.Orc.WriterConfiguration())) //Use the default configuration
            {
                writer.AddRows(testElements);
            }

            _ = memStream.Seek(0, SeekOrigin.Begin);

            INLINQ.Orc.OrcReader<RoundTripTestObject> reader = new(memStream, ignoreMissingColumns: true);
            List<RoundTripTestObject> actual = reader.Read().ToList();

            Assert.Equal(testElements.Length, actual.Count);
            for (int i = 0; i < testElements.Length; i++)
            {
                Assert.Equal(testElements[i].Decimal, actual[i].Decimal);
            }
        }

        private static void TestRoundTrip<T>(List<T> expected) where T : new()
        {
            MemoryStream memStream = new();
            using (INLINQ.Orc.OrcWriter<T> writer = new(memStream, new INLINQ.Orc.WriterConfiguration())) //Use the default configuration
            {
                writer.AddRows(expected);
            }

            //long length = memStream.Length;

            _ = memStream.Seek(0, SeekOrigin.Begin);

            INLINQ.Orc.OrcReader<T> reader = new(memStream);
            List<T> actual = reader.Read().ToList();

            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], actual[i]);
            }
        }
    }

    internal class RoundTripTestObject
    {
        private static readonly DateTime _dateBase = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public int Int { get; set; }
        public long Long { get; set; }
        public short Short { get; set; }
        public uint UInt { get; set; }
        public ulong ULong { get; set; }
        public ushort UShort { get; set; }
        public int? NullableInt { get; set; }
        public byte Byte { get; set; }
        public sbyte SByte { get; set; }
        public bool Bool { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public byte[] ByteArray { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public string String { get; set; }

        RoundTripTestObject(int @int, long @long, short @short, uint uInt, ulong uLong, ushort uShort, int? nullableInt, byte @byte, sbyte sByte, bool @bool, float @float, double @double, byte[] byteArray, decimal @decimal, DateTime dateTime, string @string)
        {
            Int = @int;
            Long = @long;
            Short = @short;
            UInt = uInt;
            ULong = uLong;
            UShort = uShort;
            NullableInt = nullableInt;
            Byte = @byte;
            SByte = sByte;
            Bool = @bool;
            Float = @float;
            Double = @double;
            ByteArray = byteArray;
            Decimal = @decimal;
            DateTime = dateTime;
            String = @string;
        }

        public RoundTripTestObject()
        {
            Int = default;
            Long = default;
            Short = default;
            UInt = default;
            ULong = default;
            UShort = default;
            NullableInt = default;
            Byte = default;
            SByte = default;
            Bool = default;
            Float = default;
            Double = default;
            ByteArray = Array.Empty<byte>();
            Decimal = default;
            DateTime = default;
            String = default;
        }

        public static RoundTripTestObject Random(Random r)
        {
            RoundTripTestObject result = new(
                r.Next(),
                r.Next() * 0xFFFFFFL,
                (short)r.Next(),
                (uint)r.Next(),
                (ulong)r.Next() * 0xFFFFFFL,
                (ushort)r.Next(),
                r.Next() % 10 == 0 ? null : r.Next(),
                (byte)r.Next(),
                (sbyte)r.Next(),
                r.Next() % 2 == 0,
                (float)r.NextDouble(),
                r.NextDouble(),
                new byte[10],
                r.Next() / 1000m,
                _dateBase.AddSeconds(r.Next()),
                r.Next().ToString(CultureInfo.InvariantCulture)
            );

            r.NextBytes(result.ByteArray);

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is RoundTripTestObject test &&
                   Int == test.Int &&
                   Long == test.Long &&
                   Short == test.Short &&
                   UInt == test.UInt &&
                   ULong == test.ULong &&
                   UShort == test.UShort &&
                   NullableInt == test.NullableInt &&
                   Byte == test.Byte &&
                   SByte == test.SByte &&
                   Bool == test.Bool &&
                   Float == test.Float &&
                   Double == test.Double &&
                   ByteArray.SequenceEqual(test.ByteArray) &&
                   Decimal == test.Decimal &&
                   DateTime == test.DateTime &&
                   String == test.String;
        }

        public override int GetHashCode()
        {
            int hashCode = 291051517;
            hashCode = hashCode * -1521134295 + Int.GetHashCode();
            hashCode = hashCode * -1521134295 + Long.GetHashCode();
            hashCode = hashCode * -1521134295 + Short.GetHashCode();
            hashCode = hashCode * -1521134295 + UInt.GetHashCode();
            hashCode = hashCode * -1521134295 + ULong.GetHashCode();
            hashCode = hashCode * -1521134295 + UShort.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(NullableInt);
            hashCode = hashCode * -1521134295 + Byte.GetHashCode();
            hashCode = hashCode * -1521134295 + SByte.GetHashCode();
            hashCode = hashCode * -1521134295 + Bool.GetHashCode();
            hashCode = hashCode * -1521134295 + Float.GetHashCode();
            hashCode = hashCode * -1521134295 + Double.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(ByteArray);
            hashCode = hashCode * -1521134295 + Decimal.GetHashCode();
            hashCode = hashCode * -1521134295 + DateTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(String);
            return hashCode;
        }
    }

    internal struct RoundTripTestStruct
    {
        private static readonly DateTime _dateBase = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public int Int { get; set; }
        public long Long { get; set; }
        public short Short { get; set; }
        public uint UInt { get; set; }
        public ulong ULong { get; set; }
        public ushort UShort { get; set; }
        public int? NullableInt { get; set; }
        public byte Byte { get; set; }
        public sbyte SByte { get; set; }
        public bool Bool { get; set; }
        public float Float { get; set; }
        public double Double { get; set; }
        public byte[] ByteArray { get; set; }
        public decimal Decimal { get; set; }
        public DateTime DateTime { get; set; }
        public string String { get; set; }


        public static RoundTripTestStruct Random(Random r)
        {
            RoundTripTestStruct result = new()
            {
                Int = r.Next(),
                Long = r.Next() * 0xFFFFFFL,
                Short = (short)r.Next(),
                UInt = (uint)r.Next(),
                ULong = (ulong)r.Next() * 0xFFFFFFL,
                UShort = (ushort)r.Next(),
                NullableInt = r.Next() % 10 == 0 ? null : r.Next(),
                Byte = (byte)r.Next(),
                SByte = (sbyte)r.Next(),
                Bool = r.Next() % 2 == 0,
                Float = (float)r.NextDouble(),
                Double = r.NextDouble(),
                ByteArray = new byte[10],
                Decimal = r.Next() / 1000m,
                DateTime = _dateBase.AddSeconds(r.Next()),
                String = r.Next().ToString(CultureInfo.InvariantCulture)
            };

            r.NextBytes(result.ByteArray);

            return result;
        }

        public override bool Equals(object obj)
        {
            RoundTripTestStruct test = (RoundTripTestStruct)obj;
            return Int == test.Int &&
                   Long == test.Long &&
                   Short == test.Short &&
                   UInt == test.UInt &&
                   ULong == test.ULong &&
                   UShort == test.UShort &&
                   NullableInt == test.NullableInt &&
                   Byte == test.Byte &&
                   SByte == test.SByte &&
                   Bool == test.Bool &&
                   Float == test.Float &&
                   Double == test.Double &&
                   ByteArray.SequenceEqual(test.ByteArray) &&
                   Decimal == test.Decimal &&
                   DateTime == test.DateTime &&
                   String == test.String;
        }

        public override int GetHashCode()
        {
            int hashCode = 291051517;
            hashCode = hashCode * -1521134295 + Int.GetHashCode();
            hashCode = hashCode * -1521134295 + Long.GetHashCode();
            hashCode = hashCode * -1521134295 + Short.GetHashCode();
            hashCode = hashCode * -1521134295 + UInt.GetHashCode();
            hashCode = hashCode * -1521134295 + ULong.GetHashCode();
            hashCode = hashCode * -1521134295 + UShort.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(NullableInt);
            hashCode = hashCode * -1521134295 + Byte.GetHashCode();
            hashCode = hashCode * -1521134295 + SByte.GetHashCode();
            hashCode = hashCode * -1521134295 + Bool.GetHashCode();
            hashCode = hashCode * -1521134295 + Float.GetHashCode();
            hashCode = hashCode * -1521134295 + Double.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(ByteArray);
            hashCode = hashCode * -1521134295 + Decimal.GetHashCode();
            hashCode = hashCode * -1521134295 + DateTime.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(String);
            return hashCode;
        }
    }
}
