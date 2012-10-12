using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace ERAServer.SRP6
{
    internal partial class HandShake
    {
        private NetBigInteger Lookup(String username, out Byte[] salt)
        {
            if (_cache.IsServerConnection)
            {
                // SERVER LOOKUP
                // Servers don't need a username and password. They just need to be able to use SRPP and have the secret, N and g right.
                salt = SRPFunctions.GenerateSalt();
                return SRPFunctions.PasswordVerifier(username, ERAServer.Properties.Settings.Default.Secret, salt, N, g);
            }
            
            // USER LOOKUP
            // Get salt and v from the database. This means that the verifier was already generated, preferably on adding into the database.

            String saltString = null;
            String vString = null;

            //usernotfound
            //new HandShakeException("Username not in database")

            salt = Convert.FromBase64String(saltString);
            return new NetBigInteger(Convert.FromBase64String(vString));
        }
    }
}
