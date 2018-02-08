using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CSDS.Utilities;
namespace CSDS_Test
{
    public class RNGBenchmarks
    {
        private readonly Random rdm = new Random(123456);
        private readonly PRNG p1 = new PRNG(123456);
        private readonly PRNG2 p2 = new PRNG2(123456);
        private readonly PRNG3 p3 = new PRNG3(123456);
        private readonly PRNG4 p4 = new PRNG4(123456);
        private readonly PRNG5 p5 = new PRNG5(123456);

        public RNGBenchmarks()
        {
        }
        [Benchmark]
        public long TestR() => rdm.Next();
        //[Benchmark]
        public long TestP1() => p1.NextLong();
        //[Benchmark]
        public long TestP2() => p2.NextLong();
        [Benchmark]
        public long TestP3() => p3.NextLong();
        //[Benchmark]
        public long TestP4() => p4.NextLong();
        [Benchmark]
        public long TestP5() => p5.NextLong();
    }

    public class Bench
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RNGBenchmarks>();
        }
    }
}
