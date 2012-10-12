using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace ERAAuthentication.SRP6
{
    internal partial class HandShake
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private NetBigInteger Lookup(SRPRequest request, out Byte[] salt)
        {
            if (_cache.ConnectionGroupIsStatic)
                return HandShake.LogonManager.LookupStatic(_cache.ConnectionGroup, ref request.Username, out salt, N, g);

            return HandShake.LogonManager.LookupVariable(_cache.ConnectionGroup, ref request.Username, out salt);
        }

    }

    /// <summary>
    /// 
    /// </summary>
    internal partial interface ILogonManager
    {
        NetBigInteger LookupVariable(Int32 group, ref String username, out Byte[] salt);
        NetBigInteger LookupStatic(Int32 group, ref String username, out Byte[] salt, NetBigInteger N, NetBigInteger g);
    }
}
