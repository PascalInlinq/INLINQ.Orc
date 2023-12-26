using System.Numerics;

namespace INLINQ.Orc.Encodings
{
    public class VarIntReader
    {
        private readonly Stream _inputStream;

		public VarIntReader(Stream inputStream)
		{
			_inputStream = inputStream;
		}

		public IEnumerable<BigInteger> Read()
		{
			while(true)
			{
                BigInteger? bigInt = BitManipulation.ReadBigVarInt(_inputStream);
				if (bigInt.HasValue)
                {
                    yield return bigInt.Value;
                }
                else
                {
                    yield break;
                }
            }
		}
    }
}
