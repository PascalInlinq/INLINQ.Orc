namespace INLINQ.Orc.FluentSerialization
{
    public class SerializationConfiguration
    {
        private readonly Dictionary<Type, ISerializationTypeConfiguration> _types = new();

		public IReadOnlyDictionary<Type, ISerializationTypeConfiguration> Types { get => _types; }

		public SerializationTypeConfiguration<T> ConfigureType<T>()
		{
			if(!_types.TryGetValue(typeof(T), out ISerializationTypeConfiguration? typeConfiguration))
			{
				typeConfiguration = new SerializationTypeConfiguration<T>(this);
				_types.Add(typeof(T), typeConfiguration);
			}
			return (SerializationTypeConfiguration<T>)typeConfiguration;
		}
    }
}
