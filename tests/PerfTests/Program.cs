using ExpressDI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerfTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            TestDictionaryLookup();
            TestDictionaryLookupMethod();
            TestDictionaryLookupNoCast();
            //TestForVsForeach();

            TestSingletons();
        }

        private static void TestSingletons()
        {
            Console.WriteLine("Singletons:");

            var c = new Container();
            c.Register<Interface1, Class1>(lf => lf.Singleton());
            c.Register<Interface2, Class2>(lf => lf.Singleton());
            c.Register<Interface3, Class3>(lf => lf.Singleton());
            var l = typeof(Interface3);

            Test(500000, 5, new Dictionary<string, Action> {
                { "For", ()=> {
                    var r = c.Resolve(l);
                } },
            });

            Console.WriteLine();
        }

        private static void TestForVsForeach()
        {
            Console.WriteLine("ForVsForeach:");

            var a = Enumerable.Range(0, 20).ToList();

            Test(500000, 5, new Dictionary<string, Action> {
                { "For", ()=> {
                    var k = 0;
                    var array = a.ToArray();
                    for (int i = 0; i < array.Length; i++)
                    {
                        k+= array[i];
                    }
                } },
                { "Foreach", ()=> {
                    var k = 0;
                    foreach (var i in a)
                    {
                        k+= i;
                    }
                } },
            });

            Console.WriteLine();
        }

        private static void TestDictionaryLookupMethod()
        {
            Console.WriteLine("DictionaryLookup:");

            var a = new Dictionary<Type, object> {
                { typeof(StringBuilder), new object() },
                { typeof(Guid), new object() },
                { typeof(GenericUriParser), new object() },
            };

            var l = typeof(GenericUriParser);

            Test(500000, 5, new Dictionary<string, Action> {
                { "Indexer", ()=> {
                    var r = a[l];
                } },
                { "IndexerWithCatch", ()=> {
                    try {
                        var r = a[l];
                    }catch { }
                } },
                { "TryGet", ()=> {
                    object r;
                    a.TryGetValue(l,out r);
                } },
            });

            Console.WriteLine();
        }

        private static void TestDictionaryLookup()
        {
            Console.WriteLine("DictionaryLookup:");

            var a = new Dictionary<Type, object> {
                { typeof(StringBuilder), new object() },
                { typeof(Guid), new object() },
                { typeof(GenericUriParser), new object() },
            };

            var b = new Dictionary<string, object> {
                { typeof(StringBuilder).FullName, new object() },
                { typeof(Guid).FullName, new object() },
                { typeof(GenericUriParser).FullName, new object() },
            };

            var c = new Dictionary<object, object> {
                { typeof(StringBuilder), new object() },
                { typeof(Guid), new object() },
                { typeof(GenericUriParser), new object() },
            };

            Test(500000, 5, new Dictionary<string, Action> {
                { "Object", ()=> {
                    var l = typeof(GenericUriParser);
                    var r = c[l];
                } },
                { "Type", ()=> {
                    var l = typeof(GenericUriParser);
                    var r = a[l];
                } },
                { "Fullname", ()=> {
                    var l = typeof(GenericUriParser).FullName;
                    var r = b[l];
                } },
            });

            Console.WriteLine();
        }

        private static void TestDictionaryLookupNoCast()
        {
            Console.WriteLine("DictionaryLookupNoCast:");

            var a = new Dictionary<Type, object> {
                { typeof(StringBuilder), new object() },
                { typeof(Guid), new object() },
                { typeof(GenericUriParser), new object() },
            };

            var b = new Dictionary<string, object> {
                { typeof(StringBuilder).FullName, new object() },
                { typeof(Guid).FullName, new object() },
                { typeof(GenericUriParser).FullName, new object() },
            };

            var c = new Dictionary<object, object> {
                { typeof(StringBuilder), new object() },
                { typeof(Guid), new object() },
                { typeof(GenericUriParser), new object() },
            };

            var d = new System.Collections.Hashtable();

            d.Add(typeof(StringBuilder), new object());
            d.Add(typeof(Guid), new object());
            d.Add(typeof(GenericUriParser), new StringBuilder());

            var l = typeof(GenericUriParser);

            var ln = typeof(GenericUriParser).FullName;

            Test(500000, 5, new Dictionary<string, Action> {
                { "Object", ()=> {
                    var r = c[l];
                } },
                { "Type", ()=> {
                    var r = a[l];
                } },
                { "Fullname", ()=> {
                    var r = b[ln];
                } },
                { "Hasttable", ()=> {
                    var r  =(StringBuilder) d[l];
                } },
            });

            Console.WriteLine();
        }

        private static void Test(int runs, int repeats, Dictionary<string, Action> variants)
        {
            foreach (var v in variants)
                v.Value();

            var results = variants.ToDictionary(v => v.Key, v => (long)0);

            for (int i = 0; i < repeats; i++)
            {
                foreach (var variant in variants)
                {
                    var sw = new Stopwatch();

                    sw.Start();

                    for (int j = 0; j < runs; j++)
                    {
                        variant.Value();
                    }

                    sw.Stop();
                    results[variant.Key] = results[variant.Key] + sw.ElapsedMilliseconds;
                }
            }

            foreach (var res in results)
                Console.WriteLine($"{res.Key}: {res.Value / repeats} ms");
        }
    }
}