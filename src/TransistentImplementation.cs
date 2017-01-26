using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressDI
{
    public static class TransistentImplementationFactory
    {
        public static Func<Type, IImplementation> Transistent(this Container f)
        {
            return t => Create(t, f);
        }

        private static IImplementation Create(Type type, Container dp)
        {
            return new TransistentImplementation(type, dp);
        }
    }

    public class TransistentImplementation : IImplementation
    {
        protected readonly Container _deps;

        private Tuple<ConstructorInfo, Dependency[]> _ctor;

        private Expression _exp;
        private Func<object> _func;

        public event EventHandler<EventArgs> Changed = (s, e) => { };

        public Type Type { get; private set; }

        public TransistentImplementation(Type type, Container deps)
        {
            Type = type;
            _deps = deps;
        }

        protected virtual void DependenciesChanged(object sender, EventArgs e)
        {
            _exp = null;
            _func = null;
            Changed(this, EventArgs.Empty);
        }

        protected Tuple<ConstructorInfo, Dependency[]> GetCtor()
        {
            if (_ctor == null)
            {
                var data = Type.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic && !c.IsStatic).Select(c => new { par = c.GetParameters(), c }).OrderByDescending(c => c.par.Length).FirstOrDefault();

                if (data == null)
                    throw new Exception();

                var deps = data.par.Select(p => p.ParameterType).Select(_deps.GetDependency).ToArray();

                foreach (var dep in deps)
                    dep.Changed += DependenciesChanged;

                _ctor = new Tuple<ConstructorInfo, Dependency[]>(data.c, deps);
            }

            return _ctor;
        }

        public virtual Expression GetExpression()
        {
            if (_exp == null)
            {
                var ctor = GetCtor();
                var par = ctor.Item2.Select(d => d.GetImplementation().GetExpression()).ToArray();

                _exp = Expression.New(ctor.Item1, par);
            }

            return _exp;
        }

        public virtual object Resolve()
        {
            if (_func == null)
            {
                _func = Expression.Lambda<Func<object>>(GetExpression()).Compile();
            }

            return _func();
        }
    }
}