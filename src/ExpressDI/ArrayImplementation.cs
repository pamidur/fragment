using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressDI
{
    public class ArrayImplementation : TransistentImplementation
    {
        private readonly Contract[] _contracts;
        private Expression _exp;
        private Func<object> _func;
        private Type _elemType;
        private Contract _contract;

        public ArrayImplementation(Type type, Type elemType, Contract contract) : base(type, null)
        {
            _contracts = new Contract[] { contract };
            _contract = contract;
            _elemType = elemType;
        }

        protected override List<Implementation> SelectImplementations(Contract c)
        {
            return c.GetAllImplementations().ToList();
        }

        public override Expression GetExpression()
        {
            if (_exp == null)
            {
                //_contract.Changed += ContractChanged;
                var par = GetImplementations(_contracts).Select(d => d.GetExpression());
                _exp = Expression.NewArrayInit(_elemType, par);
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