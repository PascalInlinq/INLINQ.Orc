using System.Collections;
using System.Collections.Generic;

namespace INLINQ.Orc
{
    public class OrcReader<T> : IEnumerable<T> where T : new()
    {
        private readonly OrcReader _underlyingOrcReader;

        public OrcReader(Stream inputStream, bool ignoreMissingColumns = false)
        {
            _underlyingOrcReader = new OrcReader(typeof(T), inputStream, ignoreMissingColumns);
        }

        public IEnumerable<T> Read()
        {
            var result = _underlyingOrcReader.Read<T>();
            return result;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
