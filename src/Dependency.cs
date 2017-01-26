using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static ExpressDI.Events;

namespace ExpressDI
{
    public sealed class Dependency
    {
        private readonly Type _type;
        private readonly IDictionary<object, Implementation> _regs;

        private Implementation _reg;

        private bool _isEnumerable = false;
        private bool _isArray = false;
        private IImplementation _impl;
        private readonly Container _dp;
        private readonly bool _isStaticDependency;

        public event EventHandler<EventArgs> Changed = (s, e) => { };

        public Dependency(Type type, IDictionary<object, Implementation> regs, Container dp)
        {
            _type = type;
            _regs = regs;
            _dp = dp;
        }

        public Dependency(IImplementation implementation)
        {
            _isStaticDependency = true;
            _impl = implementation;
            _impl.Changed += ImplementationChanged;
        }

        public IImplementation GetImplementation()
        {
            if (_impl == null)
            {
                var reg = GetRegistration();

                if (_isArray || _isEnumerable)
                    _impl = new ArrayImplementation(reg.Implementation, reg.GetImplementations().Select(i => _dp.GetDependency(i.Type)));
                else
                    _impl = reg.GetLatest();

                _impl.Changed += ImplementationChanged;
            }

            return _impl;
        }

        private Implementation GetRegistration()
        {
            if (_reg == null)
            {
                _reg = FindRegistration();
                _reg.Changed += RegistrationChanged;
            }

            return _reg;
        }

        private Implementation FindRegistration()
        {
            var elem = GetArrayElement(_type);
            if (elem != null)
            {
                _isArray = true;
                return _regs[elem];
            }

            elem = GetIEnumerableElement(_type);
            if (elem != null)
            {
                _isEnumerable = true;
                return _regs[elem];
            }

            return _regs[_type];
        }

        private void ImplementationChanged(object sender, EventArgs e)
        {
            if (!_isStaticDependency)
            {
                if (_impl != null)
                    _impl.Changed -= ImplementationChanged;

                _impl = null;
            }

            Changed(sender, e);
        }

        private void RegistrationChanged(object sender, RegistrationChangedEventArgs e)
        {
            if (!_isStaticDependency)
            {
                if (_impl != null)
                    _impl.Changed -= ImplementationChanged;

                _impl = null;

                Changed(sender, e);
            }
        }

        private static Type GetIEnumerableElement(Type type)
        {
            var ti = type.GetTypeInfo();
            if (ti.IsGenericType && ti.IsInterface && type.Namespace == Constants.GenericCollectionNS && type.Name == Constants.IEnumerableGenericIFace)
            {
                return type.GenericTypeArguments[0];
            }

            return null;
        }

        private static Type GetArrayElement(Type type)
        {
            if (type.IsArray) //todo:: getarrayRank
            {
                return type.GetElementType();
            }

            return null;
        }
    }
}