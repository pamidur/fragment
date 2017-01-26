using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressDI
{
    public class ArrayImplementation : IImplementation
    {
        private Func<object> _func;
        private object _obj;
        private Expression _exp;
        private NewArrayExpression _eexp;
        private readonly IEnumerable<Dependency> _deps;

        public event EventHandler<EventArgs> Changed = (s, e) => { };

        public Type Type { get; private set; }

        public ArrayImplementation(Type type, IEnumerable<Dependency> deps)
        {
            Type = type;
            _deps = deps;

            foreach (var dep in _deps)
                dep.Changed += DependencyChanged;
        }

        private void DependencyChanged(object sender, EventArgs e)
        {
            _eexp = null;
            _func = null;
            _obj = null;
            _exp = null;
            Changed(this, EventArgs.Empty);
        }

        public Expression GetExpression()
        {
            if (_exp == null)
                _exp = Expression.Constant(Resolve());

            return _exp;
        }

        public object Resolve()
        {
            if (_obj == null)
            {
                if (_func == null)
                {
                    _func = Expression.Lambda<Func<object>>(GetEnumerableExpression()).Compile();
                }

                _obj = _func();
            }

            return _obj;
        }

        private Expression GetEnumerableExpression()
        {
            if (_eexp == null)
            {
                var par = _deps.Select(d => d.GetImplementation().GetExpression());
                _eexp = Expression.NewArrayInit(Type, par);
            }

            return _eexp;
        }

        internal object GetObject()
        {
            throw new NotImplementedException();
        }
    }
}