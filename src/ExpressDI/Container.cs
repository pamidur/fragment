using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static ExpressDI.Implementation;

namespace ExpressDI
{
    public class Container
    {
        private Dictionary<Type, Contract> _contracts = new Dictionary<Type, Contract>();

        private readonly Func<Lifestyle, Func<Type, Dictionary<Type, Contract>, Implementation>> _defaultLifestyle;

        public Container()
        {
            _defaultLifestyle = lf => lf.Transistent();
        }

        public void Register<TContract, TImplementation>(Func<Lifestyle, Func<Type, Dictionary<Type, Contract>, Implementation>> lifestyle = null)
            where TImplementation : class, TContract
        {
            if (lifestyle == null)
                lifestyle = _defaultLifestyle;

            var ctype = typeof(TContract);
            var itype = typeof(TImplementation);

            var imp = lifestyle(null)(itype, _contracts);

            Register<TContract>(imp);
            Register<TImplementation>(imp);
        }

        public void Register<TImplementation>(Func<Lifestyle, Func<Type, Dictionary<Type, Contract>, Implementation>> lifestyle = null)
            where TImplementation : class
        {
            if (lifestyle == null)
                lifestyle = _defaultLifestyle;

            var itype = typeof(TImplementation);

            var imp = lifestyle(null)(itype, _contracts);
            Register<TImplementation>(imp);
        }

        private void Register<T>(Implementation imp)
        {
            var type = typeof(T);

            Contract reg;

            if (!_contracts.TryGetValue(type, out reg))
            {
                var etype = typeof(IEnumerable<T>);
                var atype = typeof(T[]);

                reg = new Contract(_contracts);
                _contracts.Add(type, reg);

                var ereg = new Contract(_contracts);
                ereg.AddImplementation(new ArrayImplementation(atype, type, reg));
                _contracts.Add(etype, ereg);

                var areg = new Contract(_contracts);
                areg.AddImplementation(new ArrayImplementation(atype, type, reg));
                _contracts.Add(atype, areg);
            }

            reg.AddImplementation(imp);
        }

        public T Resolve<T>()
        {
            var t = typeof(T);
            return (T)Resolve(t);
        }

        //[MethodImpl(MethodImplHints.AggressingInlining)]

        [MethodImpl((MethodImplOptions)256)]
        public object Resolve(Type type)
        {
            Contract contract;
            if (_contracts.TryGetValue(type, out contract))
                return contract.LatestActivation();

            throw new Exception($"Cannot resolve {type.Namespace}.{type.Name}.");
        }
    }
}