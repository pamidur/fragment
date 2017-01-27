using System;
using System.Collections.Generic;
using System.Linq;
using static ExpressDI.Events;
using static ExpressDI.Exceptions;

namespace ExpressDI
{
    public class Contract
    {
        private readonly Dictionary<Type, Contract> _contracts;
        private readonly Dictionary<Type, Implementation> _implementations = new Dictionary<Type, Implementation>();
        private readonly Dictionary<Implementation, Contract[]> _dependencies = new Dictionary<Implementation, Contract[]>();

        private Implementation _latest;
        internal Func<object> LatestActivation;

        public event EventHandler<DependencyTreeChangedEventArgs> Changed;

        public Contract(Dictionary<Type, Contract> contracts)
        {
            _contracts = contracts;
            LatestActivation = InitAndReplaceActivationFunc;
        }

        internal void AddImplementation(Implementation imp)
        {
            _implementations[imp.Type] = imp;
            _latest = imp;

            //imp.Changed += ImplementationChanged;

            if (Changed != null)
                Changed(this, new DependencyTreeChangedEventArgs { Origin = this, Type = imp.Type, Implementation = imp });
        }

        private void ImplementationChanged(object sender, DependencyTreeChangedEventArgs e)
        {
            e.CheckLoop(this);

            if (_latest == sender)
                LatestActivation = InitAndReplaceActivationFunc;

            if (Changed != null)
                Changed(this, e);
        }

        private object InitAndReplaceActivationFunc()
        {
            LatestActivation = _latest.GetActivation();
            return LatestActivation();
        }

        public Implementation GetLatestImplementation()
        {
            return _latest;
        }

        public Implementation[] GetAllImplementations()
        {
            return _implementations.Values.ToArray();
        }
    }
}