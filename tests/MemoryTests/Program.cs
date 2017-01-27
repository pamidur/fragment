using System;
using System.Collections.Generic;
using System.Text;

namespace MemoryTests
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var original = typeof(Program).MetadataToken;

            short firstHalf = (short)(original >> 16);
            short secondHalf = (short)(original & 0xffff);

            int reconstituted = (firstHalf << 16) | (secondHalf & 0xffff);

            var a = typeof(IEnumerable<>);
            var b = typeof(byte[]);
            var c = typeof(StringBuilder[]);

            var ttttt = new Type[Int32.MaxValue & 0xFFFFFFF];

            Console.ReadKey();
        }
    }
}