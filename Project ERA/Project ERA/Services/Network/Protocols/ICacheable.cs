using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Services.Network.Protocols
{
    interface ICacheable<T>
    {
        T Key { get; }
    }
}
