using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Collections.Concurrent;
using ERAUtils.Logger;

namespace ERAAuthentication.SRP6
{
    /// <summary>
    /// Handshake concentrates all SRPFunctions and SRPPacketData objects
    /// </summary>
    internal partial class HandShake
    {
        private static Int32 keySize;
        private static NetBigInteger g, N;
        private static NetBigInteger k;
        private static Double ExpirationInSeconds = 22;
        private static ILogonManager LogonManager;

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
        /// Gets the received userdata (remote username or local password)
        /// </summary>
        public String UserData
        {
            get { return _cache.UserData; }
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keySize"></param>
        public static void Initialize(Int32 keySize, ILogonManager logonManager)
        {
            HandShake.keySize = keySize;
            HandShake.N = SRPFunctions.GetNandG(HandShake.keySize, out HandShake.g);
            HandShake.k = SRPFunctions.Calck(HandShake.N, HandShake.g);
            HandShake.LogonManager = logonManager;

            Logger.Verbose("keySize: " + keySize);
            Logger.Verbose("N:" + N);
            Logger.Verbose("k:" + k);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keysize"></param>
        public static void Initialize(Int32 keysize)
        {
            Initialize(keysize, null);
        }

        /// <summary>
        /// Creates a new Handshake
        /// </summary>
        public HandShake(Int32 connectionGroup, Boolean connectionGroupIsStatic, Boolean active)
        {
            _cache = new SRPLocalData();
            _cache.ConnectionGroup = connectionGroup;
            _cache.ConnectionGroupIsStatic = connectionGroupIsStatic;
            _cache.IsActiveParty = active;
            this.State = HandShakeState.NotInitialized;

            if (keySize == 0 || HandShake.N == null || HandShake.g == null || k == null)
                throw new InvalidOperationException("Handshake not intialized");

            if (keySize < 1024 || keySize > 4096)
                throw new NetException("SRP6Keysize is not supported by the Lidgren.Network", 
                    new ArgumentOutOfRangeException("keySize"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionGroup"></param>
        /// <param name="connectionGroupIsStatic"></param>
        /// <param name="active"></param>
        public HandShake(Int32 connectionGroup, Boolean connectionGroupIsStatic)
            :this(connectionGroup, connectionGroupIsStatic, true)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public HandShake(Boolean active)
            : this(0, true, active)
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
