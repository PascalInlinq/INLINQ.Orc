namespace INLINQ.Orc.Encodings
{
    public static class VarIntWriter
    {
        //private readonly Stream _outputStream;

        //public VarIntWriter(Stream outputStream)
        //{
        //    _outputStream = outputStream;
        //}

        //public void Write(IList<Tuple<uint, uint, uint, bool>> values)
        //{
        //    //Console.WriteLine(System.DateTime.Now.ToString("T") + " APACHE ORC VarIntWriter Write1 Start");
        //    foreach (Tuple<uint, uint, uint, bool>? tuple in values)
        //    {
        //        _outputStream.WriteVarIntSigned(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        //    }
        //}

        public static void Write(Stream outputStream, IList<long> values)
        {
            //Console.WriteLine(System.DateTime.Now.ToString("T") + " APACHE ORC VarIntWriter Write2 Start");
            foreach (long value in values)
            {
                outputStream.WriteVarIntSigned(value);
            }
        }
    }
}
