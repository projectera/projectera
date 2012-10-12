using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data
{
    /// <summary>
    /// 
    /// </summary>
    internal interface IInteractableComponent
    {
        /// <summary>
        /// Expires the component, usually calling IResetable functions
        /// </summary>
        void Expire();

        /// <summary>
        /// Decodes the component
        /// </summary>
        /// <param name="component"></param>
        /// <param name="msg"></param>
        void Unpack(Lidgren.Network.NetIncomingMessage msg);
    }
}
