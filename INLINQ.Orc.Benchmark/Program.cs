// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using INLINQ.Orc.Benchmark;

//var benchmark = new Benchmark1();
//benchmark.LENGTH = 10;
//benchmark.MODE = Benchmark1.TESTMODE.ApacheOrcDotNet;
//benchmark.DoSetup();
//benchmark.Read();

//benchmark.Write();

BenchmarkRunner.Run<Benchmark1>();
