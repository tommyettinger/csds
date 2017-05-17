using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSCCSDS
{
    class Tests
    {
        public static void Main(string[] args)
        {
            DateTime start = DateTime.Now;
            OrderingCollection<object> orders = new OrderingCollection<object>();
            for(int i = 0; i < 1000000; i++)
            {
                orders.InsertAtStart(new object());
            }
            Console.WriteLine("ADD AT START done in " + DateTime.Now.Subtract(start).TotalMilliseconds + " ms");

            Console.WriteLine($"INSERTED SO FAR: {orders.Insertions}     AT START:  Relabelings: {orders.Relabelings}");

            Console.WriteLine("Press any key to close (maybe twice?)");
            Console.ReadKey();
        }
    }
}
