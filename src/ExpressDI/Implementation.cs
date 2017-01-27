using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static ExpressDI.Events;
using static ExpressDI.Exceptions;

namespace ExpressDI
{
    public abstract class Implementation
    {
        public sealed class Lifestyle
        {
            private Lifestyle()
            {
            }
        }

        public Type Type { get; private set; }
        protected Dictionary<Type, Contract> Contracts { get; private set; }

        public event EventHandler<DependencyTreeChangedEventArgs> Changed;

        public Implementation(Type type, Dictionary<Type, Contract> contracts)
        {
            Type = type;
            Contracts = contracts;
        }

        protected void RaiseChanged(DependencyTreeChangedEventArgs args)
        {
            if (Changed != null)
                Changed(this, args);
        }

        protected Contract[] FindContracts(Type[] dependencies)
        {
            var contracts = new Contract[dependencies.Length];

            for (int i = 0; i < dependencies.Length; i++)
            {
                var dep = dependencies[i];

                Contract contract;
                if (!Contracts.TryGetValue(dep, out contract))
                    throw new DependencyNotFoundException();

                contracts[i] = contract;
            }

            return contracts;
        }

        public abstract Expression GetExpression();

        public abstract Func<object> GetActivation();
    }
}