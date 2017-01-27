using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static ExpressDI.Implementation;

namespace ExpressDI
{
    public static class SingletonImplementationFactory
    {
        public static Func<Type, Dictionary<Type, Contract>, Implementation> Singleton(this Lifestyle f)
        {
            return (t, d) => new SingletonImplementation(t, d);
        }
    }

    public class SingletonImplementation : TransistentImplementation
    {
        private readonly object _sync = new object();

        private object _obj;
        private Expression _exp;
        private Func<object> _func;

        public SingletonImplementation(Type type, Dictionary<Type, Contract> contracts) : base(type, contracts)
        {
        }

        public override Expression GetExpression()
        {
            if (_exp == null)
                _exp = Expression.Constant(Resolve());

            return _exp;
        }

        private object Resolve()
        {
            if (_obj == null)
                lock (_sync)
                    if (_obj == null)
                        _obj = Expression.Lambda<Func<object>>(base.GetExpression()).Compile()();

            return _obj;
        }

        public override Func<object> GetActivation()
        {
            if (_func == null)
            {
                var o = Resolve();
                _func = () => o;
            }

            return _func;
        }
    }
}