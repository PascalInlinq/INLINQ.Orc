using BenchmarkDotNet.Attributes;
using Iced.Intel;
using INLINQ.Orc.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INLINQ.Orc.Benchmark
{
    [MemoryDiagnoser(false)]
    public class Benchmark1
    {
        public enum TESTMODE { ApacheOrcDotNet, INLINQ_ORC };

        private readonly MemoryStream memStream = new MemoryStream();
        private readonly List<TestObject> pocos = new();
        private WriterConfiguration INLINQconfig;

        public class TestObject
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public long Value { get; set; }

            public TestObject(long id, string name, long value)
            {
                Id = id;
                Name = name;
                Value = value;
            }

            public TestObject()
            {
            }

        }

        [GlobalSetup]
        public void DoSetup()
        {
            if(LENGTH == 0)
            {
                throw new NotSupportedException();
            }

            INLINQconfig = new WriterConfiguration();

            //create file in memory:
            for (int i = 0; i < LENGTH; i++)
            {
                pocos.Add(new TestObject(i,"Name"+i, 100000+i));
            }

            //write objects:
            using (var writer = new OrcWriter<TestObject>(memStream))
            {
                writer.AddRows(pocos);
            }
        }

        [Params(TESTMODE.INLINQ_ORC)]
        public TESTMODE MODE { get; set; }

        [Params(1000, 100_000, 10_000_000)]
        public int LENGTH { get; set; }


        [Benchmark]
        public void Read()
        {
            long total = 0;
            List<TestObject> actual;
            if (MODE == TESTMODE.ApacheOrcDotNet)
            {
                throw new NotImplementedException();
            }
            else if (MODE == TESTMODE.INLINQ_ORC)
            {
                OrcReader<TestObject> reader = new(memStream);
                foreach(var poco in reader.Read())
                {
                    total += poco.Value;
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [Benchmark]
        public void Write()
        {
            var outputStream = new MemoryStream();
            if (MODE == TESTMODE.ApacheOrcDotNet)
            {
                throw new NotImplementedException();
            }
            else if (MODE == TESTMODE.INLINQ_ORC)
            {
                using(OrcWriter<TestObject> writer = new(outputStream, INLINQconfig))
                {
                    writer.AddRows(pocos);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }




    }
}
