using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static ExpressDI.Implementation;

namespace ExpressDI
{
    public static class TransistentImplementationFactory
    {
        public static Func<Type, Dictionary<Type, Contract>, Implementation> Transistent(this Lifestyle f)
        {
            return (t, d) => new TransistentImplementation(t, d);
        }
    }

    public class TransistentImplementation : Implementation
    {
        private Expression _exp;
        private Func<object> _func;
        private ConstructorInfo _ctor;
        private Contract[] _deps;
        private List<Implementation> _imps;
        private Expression[] _exps;

        public TransistentImplementation(Type type, Dictionary<Type, Contract> contracts) : base(type, contracts)
        {
        }

        protected ConstructorInfo GetCtor()
        {
            if (_ctor == null)
            {
                var data = Type.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic && !c.IsStatic).Select(c => new { par = c.GetParameters(), c }).OrderByDescending(c => c.par.Length).FirstOrDefault();

                if (data == null)
                    throw new Exception();

                var contracts = FindContracts(data.par.Select(p => p.ParameterType).ToArray());

                //foreach (var contract in contracts)
                //contract.Changed += ContractChanged;

                _ctor = data.c;
                _deps = contracts;
            }

            return _ctor;
        }

        protected virtual void ContractChanged(object sender, Events.DependencyTreeChangedEventArgs e)
        {
            e.CheckLoop(this);

            //if (_imps != null)
            //foreach (var imp in _imps)
            //imp.Changed -= ImplementationChanged;

            _imps = null;
            _exps = null;
            _exp = null;
            _func = null;

            RaiseChanged(e);
        }

        protected List<Implementation> GetImplementations(Contract[] contracts)
        {
            if (_imps == null)
            {
                _imps = contracts.SelectMany(SelectImplementations).ToList();

                //foreach (var imp in _imps)
                //imp.Changed += ImplementationChanged;
            }

            return _imps;
        }

        protected virtual List<Implementation> SelectImplementations(Contract c)
        {
            return new List<Implementation> { c.GetLatestImplementation() };
        }

        protected virtual void ImplementationChanged(object sender, Events.DependencyTreeChangedEventArgs e)
        {
            e.CheckLoop(this);

            var imp = (Implementation)sender;

            _exps[_imps.IndexOf(imp)] = imp.GetExpression();

            _exp = null;
            _func = null;

            RaiseChanged(e);
        }

        protected Expression[] GetExpressions()
        {
            if (_exps == null)
            {
                _exps = GetImplementations(_deps).Select(d => d.GetExpression()).ToArray();
            }

            return _exps;
        }

        public override Expression GetExpression()
        {
            if (_exp == null)
            {
                _exp = Expression.New(GetCtor(), GetExpressions());
            }

            return _exp;
        }

        public override Func<object> GetActivation()
        {
            if (_func == null)
            {
                _func = Expression.Lambda<Func<object>>(GetExpression()).Compile();
            }

            return _func;
        }
    }
}