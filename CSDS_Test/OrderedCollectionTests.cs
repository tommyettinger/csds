using System;
using CSDS.Collections;
using System.Collections.Generic;
using CSDS.Utilities;

namespace CSDS_Test
{
    public class OrderedCollectionTests
    {
        public static void Main(string[] args)
        {
            const int count = 100;
            Treap<int> treap = new Treap<int>(Comparer<int>.Default);
            for (int i = 0; i < count; ++i)
            {
                treap.Add(i);
            }
            for (int i = 0; i < count; ++i)
            {
                treap.Add(~i);
            }
            //foreach (int item in treap)
            //{
            //    Console.Write(item);
            //    Console.Write(' ');
            //}
            OrderKeeper<int> keep = new OrderKeeper<int>(0);
            for (int i = 1; i < count; ++i)
            {
                keep.AddAfter(i - 1, i);
            }
            for (int i = 0; i < count; ++i)
            {
                keep.AddBefore(-i, ~i);
            }
            //foreach (int item in keep)
            //{
            //    Console.Write(item);
            //    Console.Write(' ');
            //}

            LinnormPRNG rng = new LinnormPRNG(1337);
            for (int i = 0; i < count; i++)
            {
                int idx1 = rng.Next(-count, count), idx2 = rng.Next(-count, count);
                Console.WriteLine("idx1 " + idx1 + " with path " + treap.Path(idx1) + ", idx2 " + idx2 + " with path " + treap.Path(idx2));
                if (keep.OrderOf(idx1, idx2) != treap.Before(idx1, idx2))
                {
                    Console.WriteLine("PROBLEM: idx1 " + idx1 + ", id2 " + idx2 + "; OrderKeeper gives " + keep.OrderOf(idx1, idx2) + "; Treap gives " + treap.Before(idx1, idx2));
                }
            }

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
