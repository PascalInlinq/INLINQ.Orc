using INLINQ.Orc.Infrastructure;
using Xunit;

namespace INLINQ.Orc.Test.Infrastructure
{
    public class DecimalExtensionsTest
    {
        [Fact]
        public void ToLongAndScaleScaleOf0()
        {
            Tuple<long, byte> result = 100m.ToLongAndScale();
            Assert.Equal(100, result.Item1);
            Assert.Equal(0, result.Item2);
        }

        [Fact]
        public void ToLongAndScaleScaleNotNormalized()
        {
            Tuple<long, byte> result = 100.0m.ToLongAndScale();
            Assert.Equal(1000, result.Item1);
            Assert.Equal(1, result.Item2);
        }

        [Fact]
        public void ToLongAndScaleScaleNormalized()
        {
            Tuple<long, byte> result = 100.5m.ToLongAndScale();
            Assert.Equal(1005, result.Item1);
            Assert.Equal(1, result.Item2);
        }

        [Fact]
        public void ToLongAndScaleNegative()
        {
            Tuple<long, byte> result = (-100m).ToLongAndScale();
            Assert.Equal(-100, result.Item1);
            Assert.Equal(0, result.Item2);
        }

        [Fact]
        public void ToLongAndScaleMoreThan32Bits()
        {
            Tuple<long, byte> result = 68719476735m.ToLongAndScale();
            Assert.Equal(68719476735, result.Item1);
            Assert.Equal(0, result.Item2);
        }

        [Fact]
        public void ToLongAndScale64BitsShouldThrow()
        {
            decimal dec = new(ulong.MaxValue);
            try
            {
                Tuple<long, byte> result = dec.ToLongAndScale();
                Assert.True(false, "Should have thrown");
            }
            catch (OverflowException)
            { }
        }

        [Fact]
        public void ToLongAndScaleMoreThan64BitsShouldThrow()
        {
            decimal dec = new(ulong.MaxValue);
            dec += 100;
            try
            {
                Tuple<long, byte> result = dec.ToLongAndScale();
                Assert.True(false, "Should have thrown");
            }
            catch (OverflowException)
            { }
        }

        [Fact]
        public void ToDecimalPositive()
        {
            decimal result = 100m.ToLongAndScale().ToDecimal();
            Assert.Equal(100m, result);
        }

        [Fact]
        public void ToDecimalNegative()
        {
            decimal result = (-100m).ToLongAndScale().ToDecimal();
            Assert.Equal(-100m, result);
        }

        [Fact]
        public void RescaleNoScalingNeeded()
        {
            Tuple<long, byte> tuple = 100.5m.ToLongAndScale();
            Tuple<long, byte> result = tuple.Rescale(1, false);
            Assert.Equal(1005, result.Item1);
            Assert.Equal(1, result.Item2);
        }

        [Fact]
        public void RescaleUpscale()
        {
            Tuple<long, byte> tuple = 100.5m.ToLongAndScale();
            Tuple<long, byte> result = tuple.Rescale(2, false);
            Assert.Equal(10050, result.Item1);
            Assert.Equal(2, result.Item2);
        }

        [Fact]
        public void RescaleUpscaleOverflowShouldThrow()
        {
            Tuple<long, byte> tuple = 100000000000.5m.ToLongAndScale();
            try
            {
                Tuple<long, byte> result = tuple.Rescale(8, false);
                Assert.True(false, "Should have thrown");
            }
            catch (OverflowException)
            { }
        }

        [Fact]
        public void RescaleDownscale()
        {
            Tuple<long, byte> tuple = 100.0m.ToLongAndScale();
            Tuple<long, byte> result = tuple.Rescale(0, false);
            Assert.Equal(100, result.Item1);
            Assert.Equal(0, result.Item2);
        }

        [Fact]
        public void RescaleDownscaleTruncatingDisabledShouldThrow()
        {
            Tuple<long, byte> tuple = 100.5m.ToLongAndScale();
            try
            {
                Tuple<long, byte> result = tuple.Rescale(0, false);
                Assert.True(false, "Should have thrown");
            }
            catch (ArithmeticException)
            { }
        }

        [Fact]
        public void RescaleDownscaleTruncatingEnabled()
        {
            Tuple<long, byte> tuple = 100.5m.ToLongAndScale();
            Tuple<long, byte> result = tuple.Rescale(0, true);
            Assert.Equal(100, result.Item1);
            Assert.Equal(0, result.Item2);
        }

        [Fact]
        public void RescaleDownscale1()
        {
            Tuple<long, byte> tuple = 34328.023927m.ToLongAndScale();
            Tuple<long, byte> result = tuple.Rescale(9, truncateIfNecessary: false);
            Assert.Equal(34328023927000, result.Item1);
            Assert.Equal(9, result.Item2);
        }

        [Fact]
        public void RescaleDownscale2()
        {
            Tuple<long, byte> tuple = 34328.02m.ToLongAndScale();
            Tuple<long, byte> result = tuple.Rescale(9, truncateIfNecessary: false);
            Assert.Equal(34328020000000, result.Item1);
            Assert.Equal(9, result.Item2);
        }

        [Fact]
        public void RescaleDownscale3()
        {
            Tuple<long, byte> tuple = 164.657700m.ToLongAndScale();
            Tuple<long, byte> result = tuple.Rescale(4, truncateIfNecessary: false);
            Assert.Equal(1646577, result.Item1);
            Assert.Equal(4, result.Item2);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(99, 2)]
        [InlineData(123, 3)]
        [InlineData(1000, 4)]
        [InlineData(99999, 5)]
        [InlineData(1234567890123456789, 19)]
        public void CheckPrecisionGood(long value, int maxPrecision)
        {
            value.CheckPrecision(maxPrecision);
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(11, 1)]
        [InlineData(199, 2)]
        [InlineData(1123, 3)]
        [InlineData(11000, 4)]
        [InlineData(199999, 5)]
        [InlineData(1234567890123456789, 18)]
        public void CheckPrecisionBad(long value, int maxPrecision)
        {
            _ = Assert.Throws<OverflowException>(() =>
              {
                  value.CheckPrecision(maxPrecision);
              });
        }
    }
}
