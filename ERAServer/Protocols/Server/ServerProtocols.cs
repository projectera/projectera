using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Protocols.Server
{

    public enum ServerProtocols : byte
    {
        Extension = 0,

        /// <summary>
        /// 
        /// </summary>
        PeerExchange = 1,

        /// <summary>
        /// Does Network management like exchanging peers and possibly routing
        /// </summary>
        NetworkManagement = 3,

        /// <summary>
        /// Miscellaneous actions that don't need a full network protocol
        /// </summary>
        Misc = 255,

        /// <summary>
        /// 
        /// </summary>
        Max = ProjectERA.Protocols.ProtocolConstants.NetworkMaxValue
    }
}
