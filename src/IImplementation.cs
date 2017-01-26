using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExpressDI
{
    public interface IImplementation
    {
        Type Type { get; }

        event EventHandler<EventArgs> Changed;

        Expression GetExpression();

        object Resolve();
    }
}