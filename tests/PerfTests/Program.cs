using ExpressDI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PerfTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //TestDictionaryLookup();
            //TestDictionaryLookupMethod();
            TestDictionaryLookupNoCast();
            //TestLookupPrepare();
            //TestForVsForeach();

            //TestSingletons();
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

        private class TypeComparer : IComparer<Type>
        {
            public int Compare(Type x, Type y)
            {
                var xx = x.MetadataToken;
                var yy = y.MetadataToken;

                return xx > yy ? 1 :
                 (xx < yy ? -1 : 0);
            }
        }

        private static void TestLookupPrepare()
        {
            Console.WriteLine("LookupPrepare:");

            var systemTypes = typeof(EventLogEntry).Assembly.GetTypes().ToList();

            var l = typeof(EventLogEntry);
            var tc = new TypeComparer();

            var ln = l.FullName;
            var lc = l.MetadataToken;
            var lcm = l.Module.MetadataToken;

            var lcm1 = lc >> 16;
            var lcm2 = lc & 0xffff;

            Test(10, 5, new Dictionary<string, Action> {
                { "Dict By Type", ()=> {
                    var a = systemTypes.ToDictionary(t => t, t => t);
                } },
                { "Dict by Fullname", ()=> {
                    var b = systemTypes.ToDictionary(t => t.FullName, t => t);
                } },
                { "Dict by Token", ()=> {
                    var b2 = systemTypes.ToDictionary(t => t.MetadataToken, t => t);
                } },
                { "Sorted Dict by Token", ()=> {
                    var b3 = new SortedDictionary<int, Type>(systemTypes.ToDictionary(t => t.MetadataToken, t => t));
                } },
                { "Hasttable", ()=> {
                    var d = new System.Collections.Hashtable();
                    systemTypes.ForEach(t => d.Add(t, t));
                } },
                { "SortedDict", ()=> {
                    var e = new SortedDictionary<int, SortedDictionary<int, Type>>(systemTypes.GroupBy(s => s.Module.MetadataToken).ToDictionary(g => g.Key, g => new SortedDictionary<int, Type>(g.ToDictionary(t => t.MetadataToken, t => t))));
                } },
                { "Two Arrays", ()=> {
                     var f = new Type[short.MaxValue][];
                        var b2 = systemTypes.ToDictionary(t => t.MetadataToken, t => t);
                        b2.ToList().ForEach(t =>
                        {
                            var f1 = f[t.Key >> 16];
                            if (f1 == null)

                                f[t.Key >> 16] = f1 = new Type[short.MaxValue];

                            f1[t.Key & 0xffff] = t.Value;
                        });
                } },
            });

            Console.WriteLine();
        }

        private static void TestDictionaryLookupNoCast()
        {
            Console.WriteLine("DictionaryLookupNoCast:");

            var systemTypes = typeof(EventLogEntry).Assembly.GetTypes().ToList();

            var a = systemTypes.ToDictionary(t => t, t => t);

            var b = systemTypes.ToDictionary(t => t.FullName, t => t);

            var b2 = systemTypes.ToDictionary(t => t.MetadataToken, t => t);

            var b3 = new SortedDictionary<int, Type>(systemTypes.ToDictionary(t => t.MetadataToken, t => t));
            var b4 = systemTypes.ToDictionary(t => t.MetadataToken.ToString(), t => t);

            var c = systemTypes.ToList();
            c.Sort(new TypeComparer());

            var d = new System.Collections.Hashtable();
            systemTypes.ForEach(t => d.Add(t, t));

            var e = new SortedDictionary<int, SortedDictionary<int, Type>>(systemTypes.GroupBy(s => s.Module.MetadataToken).ToDictionary(g => g.Key, g => new SortedDictionary<int, Type>(g.ToDictionary(t => t.MetadataToken, t => t))));

            var before = GC.GetTotalMemory(false);

            var f = new Type[short.MaxValue][];
            b2.ToList().ForEach(t =>
            {
                var f1 = f[t.Key >> 16];
                if (f1 == null)

                    f[t.Key >> 16] = f1 = new Type[short.MaxValue];

                f1[t.Key & 0xffff] = t.Value;
            });

            var after = GC.GetTotalMemory(false);

            long size = 0;
            object o = new object();
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, new Type[short.MaxValue]);
                size = s.Length;
            }

            var l = typeof(EventLogEntry);
            var tc = new TypeComparer();

            Test(500000, 5, new Dictionary<string, Action> {
                { "List Binary", ()=> {
                    var ri = c.BinarySearch(l,tc);
                    var r = c[ri];
                } },
                { "Dict By Type", ()=> {
                    var r = a[l];
                } },
                { "Dict by Fullname", ()=> {
                    var ln = l.FullName;
                    var r = b[ln];
                } },
                { "Dict by string token", ()=> {
                    var ln = l.MetadataToken.ToString();
                    var r = b4[ln];
                } },
                { "Dict by Token", ()=> {
                    var lc = l.MetadataToken;
                    var r = b2[lc];
                } },
                { "Sorted Dict by Token", ()=> {
                    var lc = l.MetadataToken;
                    var r = b3[lc];
                } },
                //{ "Sorted Dict by Type", ()=> {
                //    var r = b4[l];
                //} },
                { "Hasttable", ()=> {
                    var r  =(Type) d[l];
                } },
                { "SortedDict", ()=> {
                    var lcm = l.Module.MetadataToken;
                    var lc = l.MetadataToken;
                    var r  = e[lcm][lc];
                } },
                { "Two Arrays", ()=> {
                    var lc = l.MetadataToken;
                    //var lcm = l.Module.ScopeName;
                    var lcm1 = lc >> 16;
                    var lcm2 = lc & 0xffff;
                    var r  = f[lcm1][lcm2];
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