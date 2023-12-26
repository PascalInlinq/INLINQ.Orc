using System.Runtime.Intrinsics.X86;

namespace INLINQ.Orc.Encodings
{
    public class BitReader
    {
        private readonly ByteRunLengthEncodingReader _byteReader;

        public BitReader(Stream inputStream)
        {
            _byteReader = new ByteRunLengthEncodingReader(inputStream);
        }

        public IEnumerable<bool> Read()
        {
            foreach (byte[] bytes in _byteReader.ReadSmart())
            {
                foreach (byte b in bytes)
                {
                    if ((b & 0x80) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }

                    if ((b & 0x40) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }

                    if ((b & 0x20) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }

                    if ((b & 0x10) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }

                    if ((b & 0x08) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }

                    if ((b & 0x04) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }

                    if ((b & 0x02) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }

                    if ((b & 0x01) != 0)
                    {
                        yield return true;
                    }
                    else
                    {
                        yield return false;
                    }
                }
            }
        }

        //public uint ReadToArray(byte[] buffer)
        //{
        //    uint length = _byteReader.ReadToArray(buffer);
        //    return length;
        //}

        public uint ReadToArray(byte[] buffer)
        {
            uint valueId = 0;
            foreach (byte b in _byteReader.Read())
            {
                buffer[valueId++] = b;
            }
            return valueId;
        }

        public uint ReadToArray(bool[] data)
        {
            uint i = 0;
            foreach(var b in Read())
            {
                if(i>= data.Length)
                {
                    break;
                }
                data[i++] = b;
            }

            return i;
        }


    }
}
