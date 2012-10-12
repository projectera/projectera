using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Threading.Tasks;
using ERAServer.Data;
using ERAServer.Properties;
using Lidgren.Network.Authentication;

namespace ERAServer.Services
{
    internal class LogonManager : ILogonManager
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public NetBigInteger Lookup(String username, Byte[] data, out Byte[] salt)
        {
            // SERVER LOOKUP
            // data[0] set to 31. Generate on the fly.
            if (data != null && data.Length > 0 && data[0] == 31)
            {
                return Handshake.PasswordVerifier(username, Settings.Default.Secret, Settings.Default.SRP6Keysize, out salt);
            }

            // USER LOOKUP
            // Get salt and v from the database. This means that the verifier was already generated, preferably on adding into the database.
#if DEBUG
            if (username == "")
            {
                return Handshake.PasswordVerifier(username, "", Settings.Default.SRP6Keysize, out salt);
            }
            else
            {
#endif
                salt = new Byte[0];

                Task<Player> playerTask = Player.Get(username);
                if (playerTask == null || playerTask.Result == null || String.IsNullOrEmpty(playerTask.Result.Username))
                    return null;
                if (playerTask.Result.IsBanned)
                    throw new Exception("That player (" + username + ") is banned because " + playerTask.Result.BannedReason + ".");

                // Set salt and obtained username
                salt = playerTask.Result.Salt;
                username = playerTask.Result.Username;
                return new NetBigInteger(1, playerTask.Result.Verifier);
#if DEBUG
            }
#endif
        }
    }
}
