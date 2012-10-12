using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using ERAUtils.Logger;
using System.Net;
using ERAAuthentication.SRP6;

namespace ERAServer
{
    internal partial class Servers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="port"></param>
        private void InitiateHandShake(String destination, Int32 port)
        {
            IPAddress address = NetUtility.Resolve(destination);
            if (address == null)
                Logger.Error("Could not resolve " + destination + " when initiating HandShake.");
            else
                InitiateHandShake(new IPEndPoint(address, port));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        private void InitiateHandShake(IPEndPoint destination)
        {
            NetOutgoingMessage hail = _network.CreateMessage();
            hail.Write(_selfId);
            hail.Write(Properties.Settings.Default.ServerName);
            HandShake.InitiateHandShake(_network, ref hail, destination,
                ERAServer.Properties.Settings.Default.ServerName,
                ERAServer.Properties.Settings.Default.Secret,
                (Int32)DefaultConnectionGroups.Servers,
                DefaultConnectionGroups.Static.HasFlag(DefaultConnectionGroups.Servers));
        }
    }
}
