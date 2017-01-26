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
        public class RegistrationChangedEventArgs : EventArgs
        {
            public IImplementation Implementation { get; set; }
        }
    }
}