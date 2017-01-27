using System;
using System.Reflection;

namespace ExpressDI
{
    public class TypeMap
    {
        private Contract[][][][] _map = new Contract[short.MaxValue][][][];

        public void AddOrCreate(Type type, Contract ontract)
        {
            type.GetTypeInfo()
        }
    }
}