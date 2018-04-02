using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CSDS.Collections;
using CSDS.Utilities;
using DSCCSDS;
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
        private readonly PRNG6 p6 = new PRNG6(123456);
        private readonly OriolePRNG oriole = new OriolePRNG(123456);

        public RNGBenchmarks()
        {
        }
        [Benchmark]
        public long TestR() => rdm.Next();
        //[Benchmark]
        public long TestP1() => p1.Next();
        //[Benchmark]
        public long TestP2() => p2.Next();
        [Benchmark]
        public long TestP3() => p3.Next();
        //[Benchmark]
        public long TestP4() => p4.Next();
        [Benchmark]
        public long TestP5() => p5.Next();
        //[Benchmark]
        public long TestP6() => p6.Next();
        [Benchmark]
        public long TestP7() => oriole.Next();
    }
    public class OrderedCollectionBenchmarks
    {
        const int count = 100000;

        public OrderedCollectionBenchmarks()
        {
        }
        [Benchmark]
        public OrderedSet<int> TestOrderedSet()
        {
            OrderedSet<int> keep = new OrderedSet<int>(count * 2);
            for (int i = 0; i < count; ++i)
            {
                keep.Add(i);
            }
            for (int i = 0; i < count; ++i)
            {
                keep.AddBefore(i, ~i);
            }
            return keep;
        }
        //[Benchmark]
        public OrderKeeper<int> TestOrderKeeper()
        {
            OrderKeeper<int> keep = new OrderKeeper<int>(0);
            for (int i = 1; i < count; ++i)
            {
                keep.AddAfter(i - 1, i);
            }
            for (int i = 0; i < count; ++i)
            {
                keep.AddBefore(i, ~i);
            }
            return keep;
        }
        //[Benchmark]
        public OrderingCollection<int> TestOrderingCollection()
        {
            OrderingCollection<int> keep = new OrderingCollection<int>();
            keep.InsertAtStart(0);
            for (int i = 1; i < count; ++i)
            {
                keep.InsertAfter(i, i - 1);
            }
            for (int i = 0; i < count; ++i)
            {
                keep.InsertBefore(~i, i);
            }
            return keep;
        }
        [Benchmark]
        public Treap<int> TestTreap()
        {
            Treap<int> keep = new Treap<int>();
            for (int i = 0; i < count; ++i)
            {
                keep.Add(i);
            }
            for (int i = 0; i < count; ++i)
            {
                keep.Add(~i);
            }
            return keep;
        }

    }

    public class RNGRunner
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RNGBenchmarks>();
        }
    }
    public class OrderedRunner
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<OrderedCollectionBenchmarks>();
        }
    }
}
