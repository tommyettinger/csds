using System;
using CSDS;
using System.Collections.Generic;
namespace CSDS_Test
{
    public class UnitTests
    {
        public static void Main(string[] args)
        {
            DateTime start = DateTime.Now;
            OrderKeeper<string> keeper = new OrderKeeper<string>("-1");
            for(int i = 0; i < 0x100000; ++i)
            {
                keeper.AddAfter((i - 1).ToString(), i.ToString());
            }
            Console.WriteLine("Finished in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            int size = keeper.Count;
            SortedSet<ulong> labels = new SortedSet<ulong>();
            for(int i = 0; i < size; i++)
            {
                if(!labels.Add(keeper[i].Label))
                    /* // this info takes way too long to print; commented out
                    Console.WriteLine(keeper[i].Item + ": " + keeper[i].Label);
                else
                */
                {
                    // shouldn't happen
                    Console.WriteLine("DUPLICATE LABEL: " + keeper[i].Label);
                    break;
                }
            }
            for(int i = 0; i < size-1; i++)
            {
                if(!keeper.OrderOf(keeper[i].Item, keeper[i+1].Item))
                {
                    Console.WriteLine("BAD LABEL ORDER; LABEL " + i + " IS NOT BEFORE LABEL " + (i+1));
                    break;
                }
            }
            Console.WriteLine("Press any key to close (maybe twice?)");
            Console.ReadKey();
        }
    }
}
