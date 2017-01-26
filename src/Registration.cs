using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using static ExpressDI.Events;

namespace ExpressDI
{
    public sealed class Implementation
    {
        private HashSet<IImplementation> _impls = new HashSet<IImplementation>();
        private HashSet<object> _implTypes = new HashSet<object>();

        private IImplementation _latest = null;

        private readonly Dictionary<object, Implementation> _regs;

        public Implementation(Type type)
        {
            _regs = regs;
        }

        public event EventHandler<RegistrationChangedEventArgs> Changed = (s, e) => { };

        internal void AddImplementation(IImplementation implementation)
        {
            if (implementation == null || _implTypes.Contains(implementation.Type)) return;

            _impls.Add(implementation);
            _latest = implementation;
            Changed(this, new RegistrationChangedEventArgs { Implementation = implementation });
        }

        public IEnumerable<IImplementation> GetImplementations()
        {
            return _impls;
        }

        public Expression GetExpression(Type expectedType)
        {
        }

        public object GetObject()
        {
        }
    }
}