using ApacheOrcDotNet;
using BenchmarkDotNet.Attributes;
using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INLINQ.Orc.BenchmarkFramework
{
    [MemoryDiagnoser(false)]
    public class Benchmark1
    {
        public enum TESTMODE { ApacheOrcDotNet, INLINQ_ORC };

        private readonly MemoryStream memStream = new MemoryStream();
        private readonly List<TestObject> pocos = new List<TestObject>();
        //private INLINQ.Orc. WriterConfiguration INLINQconfig;
        private ApacheOrcDotNet.WriterConfiguration ApacheOrcDotNetConfig;

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

            //INLINQconfig = new WriterConfiguration();
            ApacheOrcDotNetConfig = new ApacheOrcDotNet.WriterConfiguration();

            //create file in memory:
            for (int i = 0; i < LENGTH; i++)
            {
                pocos.Add(new TestObject(i,"Name"+i, 100000+i));
            }

            //write objects:
            using (var writer = new OrcWriter<TestObject>(memStream, ApacheOrcDotNetConfig))
            {
                writer.AddRows(pocos);
            }
        }

        [Params(TESTMODE.ApacheOrcDotNet)]
        public TESTMODE MODE { get; set; }

        [Params(1000, 100_000, 10_000_000)]
        public int LENGTH { get; set; }


        [Benchmark]
        public void Read()
        {
            List<TestObject> actual;
            long total = 0;
            if (MODE == TESTMODE.ApacheOrcDotNet)
            {
                ApacheOrcDotNet.OrcReader< TestObject> reader = new OrcReader<TestObject>(memStream);
                foreach (var poco in reader.Read())
                {
                    total += poco.Value;
                }
            }
            else if (MODE == TESTMODE.INLINQ_ORC)
            {
                //OrcReader<TestObject> reader = new(memStream);
                //actual = reader.Read().ToList();
                throw new NotImplementedException();
            }
            else
            {
                //Console.WriteLine("Mode not detected!");
                //actual = new List<TestObject>();
                throw new NotImplementedException();
            }
        }

        [Benchmark]
        public void Write()
        {
            var outputStream = new MemoryStream();
            if (MODE == TESTMODE.ApacheOrcDotNet)
            {
                using (ApacheOrcDotNet.OrcWriter<TestObject> writer = new OrcWriter<TestObject>(outputStream, ApacheOrcDotNetConfig))
                {
                    writer.AddRows(pocos);
                }   
            }
            else if (MODE == TESTMODE.INLINQ_ORC)
            {
                //using(OrcWriter<TestObject> writer = new(outputStream, INLINQconfig))
                //{
                //    writer.AddRows(pocos);
                //}
            }
            else
            {
                Console.WriteLine("Mode not detected!");
            }
        }




    }
}
