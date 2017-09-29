using CSDS.Utilities;
using System;
using System.Diagnostics;
using System.Threading;

namespace CSDS_Test
{
    public class RNGTests
    {
        public static void Main(string[] args)
        {
            // Unfortunately, the performance of the non-PRNG benchmarks is highly dependent on the order those benchmarks are run.
            // We can glean some information by reordering the tests and comparing mainly the first one run.
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            int sumi;
            Stopwatch stopwatch = new Stopwatch();
            /*
            {
                RNG rng = new RNG(new ThrustRandomness(123456789));
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += rng.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nThrustRandomness done in {1}", sumi, stopwatch.Elapsed);
                }
            }
            {
                RNG rng = new RNG(new RushRandomness(123, 456, 789));
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += rng.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nRushRandomness done in {1}", sumi, stopwatch.Elapsed);
                }
            }
            {
                RNG rng = new RNG(new SMARandomness(123456789U, 987654321U));
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += rng.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nSMARandomness done in {1}", sumi, stopwatch.Elapsed);
                }
            }
            {
                RNG rng = new RNG(new HerdRandomness(123456789));
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += rng.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nHerdRandomness done in {1}", sumi, stopwatch.Elapsed);
                }
            }
            {
                RNG rng = new RNG(new SplitMixRandomness(123456789UL));
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += rng.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nSplitMixRandomness done in {1}", sumi, stopwatch.Elapsed);
                }
            }
            */
            {
                Random rdm = new Random(123456);
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += rdm.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nSystem.Random done in {1}", sumi, stopwatch.Elapsed);
                }
            }


            {
                PRNG4 p4 = new PRNG4(987654321);
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += p4.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nPRNG4 done in {1}", sumi, stopwatch.Elapsed);
                }
            }


            {
                PRNG2 p2 = new PRNG2(987654321);
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += p2.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nPRNG2 done in {1}", sumi, stopwatch.Elapsed);
                }
            }

            {
                PRNG3 p3 = new PRNG3(987654321);
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += p3.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nPRNG3 done in {1}", sumi, stopwatch.Elapsed);
                }
            }

            {
                PRNG p = new PRNG(123456789);
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    stopwatch.Restart();
                    for(int i = 0; i < 1000000007; i++)
                    {
                        sumi += p.Next();
                    }
                    stopwatch.Stop();
                    Console.WriteLine("{0}\nPRNG done in {1}", sumi, stopwatch.Elapsed);
                }
            }
        }
    }
}
