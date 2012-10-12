using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAAuthentication.SRP6
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    internal enum DefaultConnectionGroups : int
    {
        Servers = 0,
        Clients = 1,
        Terminals = 2,

        Static = DefaultConnectionGroups.Servers | DefaultConnectionGroups.Terminals,
        Variable = DefaultConnectionGroups.Clients
    }
}
