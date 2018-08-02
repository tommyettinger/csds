using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSDS.Collections;

namespace CSDS_Test
{
    class IndexedCollectionTests
    {
        public static void Main(string[] args)
        {
            IndexedSet<string> set1 = new IndexedSet<string>
            {
                "Foo",
                "Bar",
                "Quux"
            };
            IndexedSet<string> set2 = new IndexedSet<string>(new string[] { "Foo", "Bar", "Quux" });
            if (set1[0] != set2[0])
                Console.WriteLine("FAILURE: indexer with 0");
            if (set1[1] != set2[1])
                Console.WriteLine("FAILURE: indexer with 1");
            if (set1[2] != set2[2])
                Console.WriteLine("FAILURE: indexer with 2");
            if (!set1["Foo"])
                Console.WriteLine("FAILURE: lookup in set1 with Foo");
            if (!set2["Foo"])
                Console.WriteLine("FAILURE: lookup in set2 with Foo");
            set1.Insert(2, "Baz");
            set2["Quux"] = false;
            set2["Baz"] = true;
            set2.Add("Quux");
            if (set1[2] != set2[2])
                Console.WriteLine("FAILURE: indexer with 2 after changes");
            if (set1[3] != set2[3])
                Console.WriteLine("FAILURE: indexer with 3 after changes");

            if (!set1["Baz"])
                Console.WriteLine("FAILURE: lookup in set1 with Baz");
            if (!set2["Baz"])
                Console.WriteLine("FAILURE: lookup in set2 with Baz");
            try
            {
                Console.WriteLine(set1[4]);
            }
            catch (ArgumentOutOfRangeException)
            {
                //Console.WriteLine("Expected failure: lookup in set1 with indexer out of bounds");
            } catch(Exception e)
            {
                Console.WriteLine("FALURE: lookup in set1 with indexer out of bounds. " + e);
            }
            try
            {
                Console.WriteLine(set2[4]);
            }
            catch (ArgumentOutOfRangeException)
            {
                //Console.WriteLine("Expected failure: lookup in set2 with indexer out of bounds");
            }
            catch (Exception e)
            {
                Console.WriteLine("FALURE: lookup in set2 with indexer out of bounds. " + e);
            }
            if (!set1.SetEquals(set2))
                Console.WriteLine("FAILURE: SetEquals() not calculated correctly.");
            for (int i = 0; i < set1.Count; i++)
            {
                Console.WriteLine(set1[i] + " vs. " + set2[i]);
            }
            Console.WriteLine("Done! Press any key...");
            Console.ReadKey();

        }
    }
}
