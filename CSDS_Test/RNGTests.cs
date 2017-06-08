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
            RNG rng;
            rng = new RNG(new RushRandomness(123, 456, 789));
            for(int n = 0; n < 5; n++)
            {
                sumi = 0;
                start = DateTime.Now;
                for(int i = 0; i < 1000000000; i++)
                {
                    sumi += rng.Next();
                }
                Console.WriteLine(sumi + "\nRushRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            }
            
            rng = new RNG(new HerdRandomness(123456789));
            for(int n = 0; n < 5; n++)
            {
                sumi = 0;
                start = DateTime.Now;
                for(int i = 0; i < 1000000000; i++)
                {
                    sumi += rng.Next();
                }
                Console.WriteLine(sumi + "\nHerdRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            }
            
            rng = new RNG(new SplitMixRandomness(123456789UL));
            for(int n = 0; n < 5; n++) {
                sumi = 0;
                start = DateTime.Now;
                for(int i = 0; i < 1000000000; i++)
                {
                    sumi += rng.Next();
                }
                Console.WriteLine(sumi + "\nSplitMixRandomness done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            }
            
            Random rdm = new Random(123456);
            for(int n = 0; n < 5; n++)
            {
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
}
