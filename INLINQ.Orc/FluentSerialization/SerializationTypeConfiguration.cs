using System.Linq.Expressions;
using System.Reflection;

namespace INLINQ.Orc.FluentSerialization
{
    public class SerializationTypeConfiguration<T> : ISerializationTypeConfiguration
    {
        private readonly SerializationConfiguration _root;
        private readonly Dictionary<PropertyInfo, SerializationPropertyConfiguration> _properties = new();

		public SerializationTypeConfiguration(SerializationConfiguration root)
		{
			_root = root;
		}

		public IReadOnlyDictionary<PropertyInfo, SerializationPropertyConfiguration> Properties { get => _properties; }

		internal void AddConfiguration(PropertyInfo propertyInfo, SerializationPropertyConfiguration configuration) => _properties.Add(propertyInfo, configuration);
		internal SerializationConfiguration Root { get => _root; }
	}

	public static class SerializationTypeConfigurationExtensions
	{
		public static SerializationTypeConfiguration<T> ConfigureProperty<T, TProp>(this SerializationTypeConfiguration<T> typeConfiguration, Expression<Func<T, TProp>> expr, Action<SerializationPropertyConfiguration> configBuilder)
		{
            PropertyInfo? propertyInfo = GetPropertyInfoFromExpression(expr);
            SerializationPropertyConfiguration? config = new();
			configBuilder(config);
			typeConfiguration.AddConfiguration(propertyInfo, config);
			return typeConfiguration;
		}

		public static SerializationConfiguration Build<T>(this SerializationTypeConfiguration<T> typeConfiguration)
		{
			return typeConfiguration.Root;
		}

        private static PropertyInfo GetPropertyInfoFromExpression(LambdaExpression expr)
		{
			if (expr.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new NotSupportedException("Fluent interface only supports simple expression identifiers");
            }

            MemberExpression? member = (MemberExpression)expr.Body;
			return (PropertyInfo)member.Member;
		}
	}
}
