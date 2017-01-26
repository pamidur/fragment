using System.Collections.Generic;

namespace ExpressDI
{
    public struct Contract
    {
        public bool IsSingle;
        public bool IsEnumerable;
        public bool IsArray;
        public List<Implementation> Implementations;
        public ArrayImplementation Array;
        public Implementation LastImplementation;
    }
}