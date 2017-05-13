using System;
using CSDS.Collections;
using System.Collections.Generic;
namespace CSDS_Test
{
    public class UnitTests
    {
        public static void Main(string[] args)
        {
            DateTime start = DateTime.Now;
            OrderKeeper<string> keeper = new OrderKeeper<string>("0");
            for(int i = 1; i < 280; ++i)
            {
                keeper.AddBefore((i - 1).ToString(), i.ToString());
            }
            Console.WriteLine("Finished in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            int size = keeper.Count;
            SortedSet<ulong> labels = new SortedSet<ulong>();
            Record<string> current;
            foreach(string s in keeper)
            {
                current = keeper[s];
                if(labels.Add(current.Label))
                {
                    // this info takes way too long to print; commented out
                    //Console.WriteLine($"{s,-3}" + ": " + current.Label);
                }
                else
                {
                    // shouldn't happen
                    Console.WriteLine("DUPLICATE LABEL FOR ITEM " + s + ": " + current.Label);
                    break;
                }
            }
            foreach(string s in keeper)
            {
                current = keeper[s];
                if(current.Next.Equals(keeper.First)) break;
                if(!keeper.OrderOf(s, current.Next.Item))
                {
                    Console.WriteLine("BAD LABEL ORDER; ITEM " + s + " (LABEL " + current.Label + ") IS AFTER ITEM " + (current.Next.Item) + " (LABEL " + current.Next.Label + ")");
                    break;

                }
            }
            Console.WriteLine("Press any key to close (maybe twice?)");
            Console.ReadKey();
        }
    }
}
