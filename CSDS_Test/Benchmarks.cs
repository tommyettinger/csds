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
        private readonly SplitMixPRNG splitmix = new SplitMixPRNG(123456);
        private readonly ThrustPRNG p3 = new ThrustPRNG(123456);
        private readonly PRNG4 p4 = new PRNG4(123456);
        private readonly TAPRNG ta = new TAPRNG(123456);
        private readonly PRNG6 p6 = new PRNG6(123456);
        private readonly OriolePRNG oriole = new OriolePRNG(123456);
        private readonly LinnormPRNG linnorm = new LinnormPRNG(123456);
        private readonly XTPRNG xt = new XTPRNG(123456);
        private readonly LathePRNG lathe = new LathePRNG(123456);

        public RNGBenchmarks()
        {
        }
        [Benchmark]
        public long TestR() => rdm.Next();
        //[Benchmark]
        public long TestXT() => xt.Next();
        //[Benchmark]
        public long TestP1() => p1.Next();
        //[Benchmark]
        public long TestSplitMix() => splitmix.Next();
        //[Benchmark]
        public long TestP3() => p3.Next();
        //[Benchmark]
        public long TestP4() => p4.Next();
        //[Benchmark]
        public long TestTA() => ta.Next();
        //[Benchmark]
        public long TestP6() => p6.Next();
        [Benchmark]
        public long TestOriole() => oriole.Next();
        [Benchmark]
        public long TestLathe() => lathe.Next();
        [Benchmark]
        public long TestLinnorm() => linnorm.Next();
    }
    public class RangedRNGBenchmarks
    {
        private readonly LinnormPRNG linnorm = new LinnormPRNG(123456);
        private readonly LathePRNG lathe = new LathePRNG(123456);

        public RangedRNGBenchmarks()
        {
        }
        [Benchmark]
        public long TestLathe_0x7BCDEF1234567890L() => lathe.NextLong(0x7BCDEF1234567890L);
        [Benchmark]
        public long TestLinnorm_0x7BCDEF1234567890L() => linnorm.NextLong(0x7BCDEF1234567890L);
        //[Benchmark]
        //public long TestLatheOld_0x7BCDEF1234567890L() => lathe.NextLongOld(0x7BCDEF1234567890L);
        //[Benchmark]
        //public long TestLinnorOld_0x7BCDEF1234567890L() => linnorm.NextLongOld(0x7BCDEF1234567890L);

        [Benchmark]
        public long TestLathe_0x1000000000000000L() => lathe.NextLong(0x1000000000000000L);
        [Benchmark]
        public long TestLinnorm_0x1000000000000000L() => linnorm.NextLong(0x1000000000000000L);
        //[Benchmark]
        //public long TestLatheOld_0x1000000000000000L() => lathe.NextLongOld(0x1000000000000000L);
        //[Benchmark]
        //public long TestLinnorOld_0x1000000000000000L() => linnorm.NextLongOld(0x1000000000000000L);

        [Benchmark]
        public long TestLathe_0x100000000000L() => lathe.NextLong(0x100000000000L);
        [Benchmark]
        public long TestLinnorm_0x100000000000L() => linnorm.NextLong(0x100000000000L);
        //[Benchmark]
        //public long TestLatheOld_0x100000000000L() => lathe.NextLongOld(0x100000000000L);
        //[Benchmark]
        //public long TestLinnorOld_0x100000000000L() => linnorm.NextLongOld(0x100000000000L);
    }
    //class LinnormRNG { public ulong state; public LinnormRNG() : this(0UL) { } public LinnormRNG(ulong seed) { state = seed; } public ulong NextULong() { ulong z = (state = state * 0x41C64E6DUL + 1UL); z = (z ^ z >> 28) * 0xAEF17502108EF2D9UL; return (z ^ z >> 30); } }
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

    /// <summary>
    /// 64-bit results
    /// <code>
    ///       Method |      Mean |     Error |    StdDev |
    /// ------------ |----------:|----------:|----------:|
    ///        TestR | 11.112 ns | 0.1511 ns | 0.1413 ns |
    /// TestSplitMix |  2.337 ns | 0.0866 ns | 0.1689 ns |
    ///       TestTA |  2.281 ns | 0.0910 ns | 0.1709 ns |
    ///   TestOriole |  2.853 ns | 0.1024 ns | 0.1766 ns |
    ///... and later...
    ///       Method |      Mean |     Error |    StdDev |
    /// ------------ |----------:|----------:|----------:|
    ///        TestR | 10.223 ns | 0.0533 ns | 0.0499 ns |
    ///   TestOriole |  2.078 ns | 0.0760 ns | 0.0635 ns |
    ///    TestLathe |  1.978 ns | 0.0208 ns | 0.0195 ns |
    ///  TestLinnorm |  2.015 ns | 0.0194 ns | 0.0181 ns |
    /// </code>
    /// 
    /// 32-bit results
    /// <code>
    ///       Method |      Mean |     Error |    StdDev |
    /// ------------ |----------:|----------:|----------:|
    ///        TestR | 10.457 ns | 0.4201 ns | 0.3724 ns |
    /// TestSplitMix |  2.422 ns | 0.2413 ns | 0.2257 ns |
    ///       TestTA |  7.266 ns | 0.2585 ns | 0.2292 ns |
    ///   TestOriole |  4.508 ns | 0.3260 ns | 0.3623 ns |
    ///... and later...  
    ///       Method |     Mean |     Error |    StdDev |
    /// ------------ |---------:|----------:|----------:|
    ///        TestR | 9.374 ns | 0.3142 ns | 0.2785 ns |
    ///   TestOriole | 3.881 ns | 0.0761 ns | 0.0711 ns |
    ///    TestLathe | 3.441 ns | 0.0702 ns | 0.0657 ns |
    ///  TestLinnorm | 9.203 ns | 0.0996 ns | 0.0931 ns |
    ///</code>
    /// Lathe seems best right now.
    /// </summary>
    public class RNGRunner
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RNGBenchmarks>();
        }
    }
    /// <summary>
    /// The TestLathe_0x... and TestLinnorm_0x... methods test the current NextLong(long) methods.
    /// The TestLatheOld_0x... and TestLinnormOld_0x... methods test the previous version of the NextLong(long) methods.
    /// <code>
    ///BenchmarkDotNet=v0.10.12, OS=Windows 7 SP1 (6.1.7601.0)
    ///Intel Core i7-6700HQ CPU 2.60GHz(Skylake), 1 CPU, 8 logical cores and 4 physical cores
    ///Frequency=2531279 Hz, Resolution=395.0572 ns, Timer=TSC
    ///  [Host]     : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
    ///  DefaultJob : .NET Framework 4.6.1 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.2558.0
    ///
    ///
    ///                            Method |      Mean |     Error |    StdDev |
    ///---------------------------------- |----------:|----------:|----------:|
    ///     TestLathe_0x7BCDEF1234567890L |  7.380 ns | 0.0537 ns | 0.0503 ns |
    ///  TestLatheOld_0x7BCDEF1234567890L | 25.473 ns | 0.5964 ns | 0.5287 ns |
    ///   TestLinnorm_0x7BCDEF1234567890L |  4.728 ns | 0.1309 ns | 0.1508 ns |
    /// TestLinnorOld_0x7BCDEF1234567890L | 22.295 ns | 0.0992 ns | 0.0928 ns |
    ///     TestLathe_0x1000000000000000L |  7.462 ns | 0.2791 ns | 0.2330 ns |
    ///  TestLatheOld_0x1000000000000000L | 25.460 ns | 0.2131 ns | 0.1780 ns |
    ///   TestLinnorm_0x1000000000000000L |  4.962 ns | 0.1326 ns | 0.1176 ns |
    /// TestLinnorOld_0x1000000000000000L | 22.173 ns | 0.1255 ns | 0.1174 ns |
    ///         TestLathe_0x100000000000L |  7.228 ns | 0.0269 ns | 0.0238 ns |
    ///      TestLatheOld_0x100000000000L | 24.799 ns | 0.1393 ns | 0.1163 ns |
    ///       TestLinnorm_0x100000000000L |  4.665 ns | 0.0177 ns | 0.0157 ns |
    ///     TestLinnorOld_0x100000000000L | 22.128 ns | 0.1408 ns | 0.1317 ns |
    /// </code>
    /// </summary>
    public class RangedRNGRunner
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<RangedRNGBenchmarks>();
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
