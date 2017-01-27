using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressDI
{
    public class Events
    {
        public class DependencyTreeChangedEventArgs : EventArgs
        {
            public Implementation Implementation { get; set; }
            public Contract Origin { get; internal set; }
            public Type Type { get; internal set; }

            internal void CheckLoop(Contract contract)
            {
            }

            internal void CheckLoop(Implementation transistentImplementation)
            {
            }
        }
    }
}