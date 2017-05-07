using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSDS;
using System.Collections.Generic;
namespace CSDS_Test
{
    public class UnitTests
    {
        public static void Main(string[] args)
        {
            OrderKeeper<string> keeper = new OrderKeeper<string>("-1");
            for(int i = 0; i < 999; ++i)
            {
                keeper.AddAfter((i - 1).ToString(), i.ToString());
            }
            int size = keeper.Count;
            SortedSet<ulong> labels = new SortedSet<ulong>();
            for(int i = 0; i < size; i++)
            {
                if(labels.Add(keeper[i].Label))
                    Console.WriteLine(keeper[i].Item + ": " + keeper[i].Label);
                else
                {
                    // shouldn't happen
                    Console.WriteLine("DUPLICATE LABEL: " + keeper[i].Label);
                    break;
                }
            }
            Console.ReadKey();
        }
    }
}
