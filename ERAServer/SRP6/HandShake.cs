using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAServer.Protocols.Server.Constants;
using ERAServer.SRP6;
using Lidgren.Network;
using System.Collections.Concurrent;
using ERAUtils.Logger;

namespace ERAServer.SRP6
{
    /// <summary>
    /// Handshake concentrates all SRPFunctions and SRPPacketData objects
    /// </summary>
    internal partial class HandShake
    {
        private static Int32 keySize = ERAServer.Properties.Settings.Default.SRP6Keysize;
        private static NetBigInteger g, N = SRPFunctions.GetNandG(keySize, out g);
        private static NetBigInteger k = SRPFunctions.Calck(N, g);
        private static Double ExpirationInSeconds = 15;

        private SRPRequest _request;
        private SRPResponse _response;
        private SRPVerification _verification;
        private SRPLocalData _cache;
        
        /// <summary>
        /// Current HandShake State
        /// </summary>
        public HandShakeState State 
        { 
            get; 
            private set;
        }

        /// <summary>
        /// HandShake locally initiated flag
        /// </summary>
        public Boolean IsActiveParty
        {
            get { return _cache.IsActiveParty; }
        }

        /// <summary>
        /// Creates a new Handshake
        /// </summary>
        public HandShake(Boolean serverConnection, Boolean active)
        {
            _cache = new SRPLocalData();
            _cache.IsServerConnection = serverConnection;
            _cache.IsActiveParty = active;
            this.State = HandShakeState.NotInitialized;

            if (keySize < 1024 || keySize > 4096)
                throw new NetException("SRP6Keysize is not supported by the Lidgren.Network", 
                    new ArgumentOutOfRangeException("keySize"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public HandShake(Boolean active)
            : this(false, active)
        {
        }

        /// <summary>
        /// Verifies generated keys on both ends
        /// </summary>
        /// <param name="verification">SRPVerification</param>
        /// <returns>Key</returns>
        public Byte[] VerifyData(ref SRPVerification verification)
        {
            // If received message is a response
            if (_verification != null && _verification.IsMessageGenerated)
                VerificationOfPassiveParty(verification);
            // If received message is a request
            else
                VerificationOfActiveParty(verification);

            // Save new verification
            verification = _verification;
            return _cache.K;
        }

        /*  n 	A large prime number. All computations are performed modulo n.
            g 	A primitive root modulo n (often called a generator)
            s 	A random string used as the user's salt
            P 	The user's password
            x 	A private key derived from the password and salt
            v 	The host's password verifier
            u 	Random scrambling parameter, publicly revealed
            a,b 	Ephemeral private keys, generated randomly and not publicly revealed
            A, B 	Corresponding public keys
            H() 	One-way hash function
            m,n 	The two quantities (strings) m and n concatenated
            K 	Session key 
    
             x = H(s, P)    private key:         hashfunctie [salt (on server /w username), password (on client)]
            v = g^x        password verifier:   generator ^ private key 
            A = g^a        public key:          generator ^ emp private key (on client)
 		    
            B = v + g^b    public key:          verifier (on server /w username) + generator ^ emp private key (on server)
         
 	        S = (B - g^x)^(a + ux) 
            S = (A · v^u)^b
         
            K = H(S)
         
            M[1] = H(A, B, K)
            M[2] = H(A, M[1], K)
         */
    }
}
