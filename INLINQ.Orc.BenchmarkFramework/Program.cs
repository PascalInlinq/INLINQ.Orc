using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INLINQ.Orc.BenchmarkFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var benchmark = new Benchmark1();
            //benchmark.LENGTH = 10;
            //benchmark.MODE = Benchmark1.TESTMODE.ApacheOrcDotNet;
            //benchmark.DoSetup();
            //benchmark.Read();
            //benchmark.Write();

            BenchmarkRunner.Run<Benchmark1>();
            Console.WriteLine("DONE!");
            Console.ReadKey();
        }
    }
}
