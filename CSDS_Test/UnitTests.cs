using System;
using CSDS.Collections;
using System.Collections.Generic;
using CSDS.Utilities;

namespace CSDS_Test
{
    public class UnitTests
    {
        public static void Main(string[] args)
        {
            const int count = 1000000;
            DateTime start;
            
            OrderedSet<int> keep;
            start = DateTime.Now;
            keep = new OrderedSet<int>(count * 2);
            for(int i = 0; i < count; ++i)
            {
                keep.Add(i);
            }
            for(int i = 0; i < count; ++i)
            {
                keep.AddBefore(i, -1 - i);
            }
            Console.WriteLine("SPEED TEST done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            
            start = DateTime.Now;
            OrderKeeper<int> keep2;
            keep2 = new OrderKeeper<int>(0);
            for(int i = 1; i < count; ++i)
            {
                keep2.AddAfter(i-1, i);
            }
            for(int i = 0; i < count; ++i)
            {
                keep2.AddBefore(i, -1 - i);
            }
            Console.WriteLine("SPEED TEST done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            /*
            start = DateTime.Now;
            OrderKeeper<string> keeper;
            List<string> items = new List<string>(count);
            keeper = new OrderKeeper<string>("0");
            for(int i = 1; i < count; ++i)
            {
                keeper.AddAfter((i - 1).ToString(), i.ToString());
            }
            Console.WriteLine("ADD AT END done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
#if EXTRA
            Console.WriteLine("AT END:  Relabelings: " + keeper.relabelings);
#endif
            start = DateTime.Now;
            keeper = new OrderKeeper<string>("0");
            for(int i = 1; i < count; ++i)
            {
                keeper.AddAfter(keeper.First, i.ToString());
            }
            Console.WriteLine("ADD AT START done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
#if EXTRA
            Console.WriteLine("AT START:  Relabelings: " + keeper.relabelings);
#endif
            items.Add("0");
            start = DateTime.Now;
            keeper = new OrderKeeper<string>("0");
            for(int i = 1; i < count; ++i)
            {
                string istr = i.ToString();
                keeper.AddAfter(items[items.Count / 2], istr);
                items.Add(istr);
            }
            Console.WriteLine("ADD IN MIDDLE done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
#if EXTRA
            Console.WriteLine("IN MIDDLE:  Relabelings: " + keeper.relabelings);
#endif
            items.Clear();
            items.Add("0");
            start = DateTime.Now;
            keeper = new OrderKeeper<string>("0");
            RNG random = new RNG(0xDADABAFFL);
            for(int i = 1; i < count; ++i)
            {
                string istr = i.ToString();
                keeper.AddAfter(items[random.Next(items.Count)], istr);
                items.Add(istr);
            }
            Console.WriteLine("ADD AT RANDOM done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
#if EXTRA
            Console.WriteLine("RANDOM:  Relabelings: " + keeper.relabelings);
#endif

            
            Console.WriteLine("Finished in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");
            //int size = keeper.Count;
            //SortedSet<ulong> labels = new SortedSet<ulong>();
            //Record<string> current;
            //foreach(string s in keeper)
            //{
            //    current = keeper[s];
            //    if(labels.Add(current.Label))
            //    {
            //        // this info takes way too long to print; commented out
            //        //Console.WriteLine($"{s,-3}" + ": " + current.Label);
            //    }
            //    else
            //    {
            //        // shouldn't happen
            //        Console.WriteLine("DUPLICATE LABEL FOR ITEM " + s + ": " + current.Label);
            //        break;
            //    }
            //}
            //foreach(string s in keeper)
            //{
            //    current = keeper[s];
            //    if(current.Next.Equals(keeper.First)) break;
            //    if(!keeper.OrderOf(s, current.Next.Item))
            //    {
            //        Console.WriteLine("BAD LABEL ORDER; ITEM " + s + " (LABEL " + current.Label + ") IS AFTER ITEM " + (current.Next.Item) + " (LABEL " + current.Next.Label + ")");
            //        break;

            //    }
            //}
            */
            Console.WriteLine("Press any key to close (maybe twice?)");
            Console.ReadKey();
            
        }
    }
}
