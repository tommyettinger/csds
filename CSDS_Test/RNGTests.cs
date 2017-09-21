using CSDS.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSDS_Test
{
    public class RNGTests
    {
        public static void Main(string[] args)
        {
            //Randomness rand;
            //long sum;
            int sumi;
            DateTime start;

            //rng = new RNG(new RushRandomness(123, 456, 789));
            //for(int n = 0; n < 4; n++)
            //{
            //    sumi = 0;
            //    start = DateTime.Now;
            //    for(int i = 0; i < 1000000000; i++)
            //    {
            //        sumi += rng.Next();
            //    }
            //    Console.WriteLine(sumi + "\nRushRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            //}
            //{
            //    RNG rng = new RNG(new SMARandomness(123456789U, 987654321U));
            //    for(int n = 0; n < 4; n++)
            //    {
            //        sumi = 0;
            //        start = DateTime.Now;
            //        for(int i = 0; i < 1000000000; i++)
            //        {
            //            sumi += rng.Next();
            //        }
            //        Console.WriteLine(sumi + "\nSMARandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            //    }
            //}

            //{
            //    RNG rng = new RNG(new HerdRandomness(123456789));
            //    for(int n = 0; n < 4; n++)
            //    {
            //        sumi = 0;
            //        start = DateTime.Now;
            //        for(int i = 0; i < 1000000000; i++)
            //        {
            //            sumi += rng.Next();
            //        }
            //        Console.WriteLine(sumi + "\nHerdRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            //    }
            //}
            //{
            //    RNG rng = new RNG(new SplitMixRandomness(123456789UL));
            //    for(int n = 0; n < 4; n++)
            //    {
            //        sumi = 0;
            //        start = DateTime.Now;
            //        for(int i = 0; i < 1000000000; i++)
            //        {
            //            sumi += rng.Next();
            //        }
            //        Console.WriteLine(sumi + "\nSplitMixRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            //    }
            //}

            //Random rdm = new Random(123456);
            //for(int n = 0; n < 4; n++)
            //{
            //    sumi = 0;
            //    start = DateTime.Now;
            //    for(int i = 0; i < 1000000000; i++)
            //    {
            //        sumi += rdm.Next();
            //    }
            //    Console.WriteLine(sumi + "\nSystem.Random done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            //}
            {
                PRNG2 p2 = new PRNG2(987654321);
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    start = DateTime.Now;
                    for(int i = 0; i < 1000000000; i++)
                    {
                        sumi += p2.Next();
                    }
                    Console.WriteLine(sumi + "\nPRNG2 done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
                }
            }

            {
                PRNG p = new PRNG(123456789);
                for(int n = 0; n < 4; n++)
                {
                    sumi = 0;
                    start = DateTime.Now;
                    for(int i = 0; i < 1000000000; i++)
                    {
                        sumi += p.Next();
                    }
                    Console.WriteLine(sumi + "\nPRNG done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
                }
            }
        }
    }
}
