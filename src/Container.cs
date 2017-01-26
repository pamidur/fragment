using System;
using System.Collections.Generic;

namespace ExpressDI
{
    public class Container
    {
        private Dictionary<object, Contract> _contracts = new Dictionary<object, Contract>();

        public Container()
        {
        }

        public void Register<TContract, TImplementation>()
            where TImplementation : class, TContract
        {
            var ctype = typeof(TContract);
            var itype = typeof(TImplementation);

            var imp = new Implementation(itype);

            Register(ctype, imp);
            Register(itype, imp);
        }

        public void Register<TImplementation>()
            where TImplementation : class
        {
            var itype = typeof(TImplementation);

            var imp = new Implementation(itype);
            Register(itype, imp);
        }

        private void Register(Type ctype, Implementation imp)
        {
            Contract reg;

            if (!_contracts.TryGetValue(ctype, out reg))
            {
                reg = new Contract { Implementations = new List<Implementation>() };
                _contracts.Add(ctype, reg);
            }

            reg.Implementations.Add(imp);
            reg.LastImplementation = imp;
        }

        public T Resolve<T>()
        {
            var t = typeof(T);
            return (T)Resolve(t);
        }

        public object Resolve(Type type)
        {
            var contract = GetContract(type);

            if (contract.IsSingle)
                return contract.LastImplementation.GetObject();
            else
                return contract.Array.GetObject();
        }

        internal Contract GetContract(Type ctype)
        {
            Contract contract;
            if (_contracts.TryGetValue(ctype, out contract))
            {
                return contract;
            }

            throw new Exception($"Cannot resolve {ctype.Namespace}.{ctype.Name}.");
        }
    }
}