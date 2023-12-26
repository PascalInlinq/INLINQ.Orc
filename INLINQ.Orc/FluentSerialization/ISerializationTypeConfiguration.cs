using System.Reflection;

namespace INLINQ.Orc.FluentSerialization
{
    public interface ISerializationTypeConfiguration
    {
		IReadOnlyDictionary<PropertyInfo, SerializationPropertyConfiguration> Properties { get; }
    }
}
