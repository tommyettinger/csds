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
            List<ulong> labelList = new List<ulong>(size);
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
                labelList.Add(keeper[i].Label);
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
