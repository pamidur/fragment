using System;
using System.Linq.Expressions;

namespace ExpressDI
{
    public static class SingletonImplementationFactory
    {
        public static Func<Type, IImplementation> Singleton(this Container f)
        {
            return t => Create(t, f);
        }

        private static IImplementation Create(Type type, Container dp)
        {
            return new SingletonImplementation(type, dp);
        }
    }

    public class SingletonImplementation : TransistentImplementation
    {
        private readonly object _sync = new object();

        private object _obj;
        private Expression _exp;

        public SingletonImplementation(Type type, Container dp) : base(type, dp)
        {
        }

        protected override void DependenciesChanged(object sender, EventArgs e)
        {
            //do nothing
            //todo:: maybe check if already instatinaned
        }

        public override Expression GetExpression()
        {
            if (_exp == null)
                _exp = Expression.Constant(Resolve());

            return _exp;
        }

        public override object Resolve()
        {
            if (_obj == null)
                lock (_sync)
                    if (_obj == null)
                        _obj = Expression.Lambda<Func<object>>(base.GetExpression()).Compile()();

            return _obj;
        }
    }
}