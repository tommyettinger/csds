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
            Randomness rand;
            long sum;
            int sumi;
            //rng = new RNG();
            rand = new SplitMixRandomness(123456789UL);
            Random rdm;
            //rng.Rand = new RushRandomness(123, 456, 789);
            DateTime start;
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rand.Next32();
            }
            Console.WriteLine(sumi + "\nSplitMixRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

            rand = new RushRandomness(123, 456, 789);
            //rng.Rand = new SplitMixRandomness(123456789UL);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rand.Next32();
            }
            Console.WriteLine(sumi + "\nRushRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

            rdm = new Random(123456);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rdm.Next();
            }
            Console.WriteLine(sumi + "\nSystem.Random done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

            rand = new SplitMixRandomness(123456789UL);
            //rng.Rand = new RushRandomness(123, 456, 789);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rand.Next32();
            }
            Console.WriteLine(sumi + "\nSplitMixRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            
            rand = new RushRandomness(123, 456, 789);
            //rng.Rand = new SplitMixRandomness(123456789UL);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rand.Next32();
            }
            Console.WriteLine(sumi + "\nRushRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

            rdm = new Random(123456);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rdm.Next();
            }
            Console.WriteLine(sumi + "\nSystem.Random done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

            rand = new SplitMixRandomness(123456789UL);
            //rng.Rand = new RushRandomness(123, 456, 789);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rand.Next32();
            }
            Console.WriteLine(sumi + "\nSplitMixRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            
            rand = new RushRandomness(123, 456, 789);
            //rng.Rand = new SplitMixRandomness(123456789UL);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rand.Next32();
            }
            Console.WriteLine(sumi + "\nRushRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

            rdm = new Random(123456);
            sumi = 0;
            start = DateTime.Now;
            for(int i = 0; i < 1000000000; i++)
            {
                sumi += rdm.Next();
            }
            Console.WriteLine(sumi + "\nSystem.Random done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

        }
    }
}
