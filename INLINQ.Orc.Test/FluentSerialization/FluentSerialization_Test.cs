using INLINQ.Orc.FluentSerialization;
using Xunit;

namespace INLINQ.Orc.Test.FluentSerialization
{
    public class FluentSerializationTest
    {
        [Fact]
        public void FluentSerializationIdentifiesBliTRowProperties()
        {
            SerializationConfiguration conf = new();
            _ = conf.ConfigureType<TestType>()
                .ConfigureProperty(x => x.IntColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.DecColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.TimeColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.EnumColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.StructColumn, x => x.ExcludeFromSerialization = true)
            ;

            IReadOnlyDictionary<System.Reflection.PropertyInfo, SerializationPropertyConfiguration> properties = conf.Types[typeof(TestType)].Properties;
            Assert.Contains(typeof(TestType).GetProperty("IntColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("DecColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("TimeColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("EnumColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("StructColumn"), properties.Keys);

            foreach (SerializationPropertyConfiguration propertyConfiguration in properties.Values)
            {
                Assert.True(propertyConfiguration.ExcludeFromSerialization);
            }
        }

        [Fact]
        public void FluentSerializationIdentifiesNullableProperties()
        {
            SerializationConfiguration conf = new();
            _ = conf.ConfigureType<TestType>()
                .ConfigureProperty(x => x.NullableIntColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.NullableDecColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.NullableTimeColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.NullableEnumColumn, x => x.ExcludeFromSerialization = true)
            ;

            IReadOnlyDictionary<System.Reflection.PropertyInfo, SerializationPropertyConfiguration> properties = conf.Types[typeof(TestType)].Properties;
            Assert.Contains(typeof(TestType).GetProperty("NullableIntColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("NullableDecColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("NullableTimeColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("NullableEnumColumn"), properties.Keys);

            foreach (SerializationPropertyConfiguration propertyConfiguration in properties.Values)
            {
                Assert.True(propertyConfiguration.ExcludeFromSerialization);
            }
        }

        [Fact]
        public void FluentSerializationIdentifiesReferenceProperties()
        {
            SerializationConfiguration conf = new();
            _ = conf.ConfigureType<TestType>()
                .ConfigureProperty(x => x.StrColumn, x => x.ExcludeFromSerialization = true)
                .ConfigureProperty(x => x.ClassColumn, x => x.ExcludeFromSerialization = true)
            ;

            IReadOnlyDictionary<System.Reflection.PropertyInfo, SerializationPropertyConfiguration> properties = conf.Types[typeof(TestType)].Properties;
            Assert.Contains(typeof(TestType).GetProperty("StrColumn"), properties.Keys);
            Assert.Contains(typeof(TestType).GetProperty("ClassColumn"), properties.Keys);

            foreach (SerializationPropertyConfiguration propertyConfiguration in properties.Values)
            {
                Assert.True(propertyConfiguration.ExcludeFromSerialization);
            }
        }
    }

    internal enum TestEnum { One, Two }

    internal class TestType
    {
        public int IntColumn { get; set; }
        public decimal DecColumn { get; set; }
        public DateTime TimeColumn { get; set; }
        public TestEnum EnumColumn { get; set; }
        public int? NullableIntColumn { get; set; }
        public decimal? NullableDecColumn { get; set; }
        public DateTime? NullableTimeColumn { get; set; }
        public TestEnum? NullableEnumColumn { get; set; }
        public string StrColumn { get; set; }
        public InternalTestType ClassColumn { get; set; }
        public InternalTestType2 StructColumn { get; set; }
    }

    internal class InternalTestType
    {
        public int InternalInt { get; set; }
    }

    internal struct InternalTestType2
    {
        public int InternalInt { get; set; }
    }
}
