using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;

namespace ERAAuthentication.SRP6
{
    /// <summary>
    /// This object holds the local cache for generated values of the private
    /// and public key and for the username/password, session key and keybytes.
    /// </summary>
    internal class SRPLocalData
    {
        private NetBigInteger _secretValue;
        private NetBigInteger _publicValue;

        /// <summary>
        /// Private randomly runtime generated value
        /// </summary>
        public NetBigInteger a 
        { 
            get { return _secretValue; } 
            set { _secretValue = value; } 
        }

        /// <summary>
        /// Private randomly runtime generated value
        /// </summary>
        public NetBigInteger b
        {
            get { return _secretValue; }
            set { _secretValue = value; }
        }

        /// <summary>
        /// Public randomly runtime generated counterpart
        /// </summary>
        public NetBigInteger A 
        { 
            get { return _publicValue; } 
            set { _publicValue = value; } 
        }

        /// <summary>
        ///  Public randomly runtime generated counterpart
        /// </summary>
        public NetBigInteger B
        {
            get { return _publicValue; }
            set { _publicValue = value; }
        }

        /// <summary>
        /// UserData (Username or Password)
        /// </summary>
        public String UserData { get; set; }

        /// <summary>
        /// Session key as NetBigInteger
        /// </summary>
        public NetBigInteger S { get; set; }

        /// <summary>
        /// Session key as Bytes (generated from S)
        /// </summary>
        public Byte[] K { get; set; }

        /// <summary>
        /// Expiration Timestamp
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Logon Group
        /// </summary>
        public Int32 ConnectionGroup { get; set; }

        /// <summary>
        /// Flag that this is the active party in the handshake
        /// </summary>
        public Boolean IsActiveParty { get; set; }

        /// <summary>
        /// Logon Type (static means: 1 password for all)
        /// </summary>
        public bool ConnectionGroupIsStatic { get; set; }
    }
}
