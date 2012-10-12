using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils.Logger;
using Lidgren.Network;

namespace ERAServer.SRP6
{
    internal partial class HandShake
    {
        /// <summary>
        /// Initiates the SRPP Request
        /// </summary>
        /// <param name="username">Username to login with</param>
        /// <param name="destination">Password to login with</param>
        /// <returns>SRPRequest</returns>
        public SRPRequest GetSRPRequest(String username, String password)
        {
            if (this.State != HandShakeState.NotInitialized && HandShakeState.AllowRequest.HasFlag(this.State) == false)
                throw new SRP6.HandShakeException("Double Request");

            // Set state and timer
            this.State = HandShakeState.Requesting;
            _cache.ExpirationTime = DateTime.Now.AddSeconds(HandShake.ExpirationInSeconds);

            // First get the public key A from random private a
            _cache.a = SRPFunctions.Geta();
            _cache.A = SRPFunctions.CalcA(N, g, _cache.a);

            Logger.Verbose("a:" + _cache.a.ToString());
            Logger.Verbose("A:" + _cache.A.ToString());

            // Save the password to use when the response comes in
            _cache.UserData = password;

            // Create a new request
            _request = new SRPRequest(username, _cache.A);

            Logger.Verbose("username:" + username);

            // Set State
            this.State = HandShakeState.Requested;

            return _request;
        }



        /// <summary>
        /// Generates Session key from response
        /// </summary>
        /// <param name="response">SRPResponse</param>
        /// <response>SRPVerification</response>
        public SRPVerification KeyFromResponse(SRPResponse response)
        {
            if (HandShakeState.AllowVerificating.HasFlag(this.State) == false)
                throw new SRP6.HandShakeException("Double Request");

            // When we get the response, get their public key B
            if (response.B.Mod(N).IntValue == 0)
            {
                this.State = HandShakeState.Failed;
                throw new HandShakeException("Response contains invalid data", new SRPException("B mod N is zero."));
            }

            Logger.Verbose("REMOTE B:" + response.B.ToString());
            Logger.Verbose("REMOTE Salt:" + new NetBigInteger(response.Salt).ToString());
            Logger.Verbose("REMOTE Username:" + _request.Username);

            // Shared random scrambler
            NetBigInteger u = SRPFunctions.Calcu(_cache.A, response.B, N.ToString().Length);
            if (u.IntValue == 0)
            {
                this.State = HandShakeState.Failed;
                throw new HandShakeException("Response contains invalid data", new SRPException("u is zero."));
            }

            // Private key x
            NetBigInteger x = SRPFunctions.Calcx(response.Salt, _request.Username, _cache.UserData);

            // Cache Response;
            _response = response;

            // Session key
            _cache.S = SRPFunctions.CalcSClient(N, g, response.B, k, x, _cache.a, u);
            _cache.K = SRPFunctions.CalcK(_cache.S);

            Logger.Verbose("CLIENT S:" + _cache.S.ToString());

            // Create the verification
            _verification = new SRPVerification(SRPFunctions.CalcM(N, g, _request.Username, response.Salt, _cache.A, response.B, _cache.K));
            Logger.Verbose("A: " + _cache.A.BitLength + "B: " + response.B.BitLength);

            // Set State
            this.State = HandShakeState.Verificating;
            return _verification;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verification"></param>
        private void VerificationOfPassiveParty(SRPVerification verification)
        {
            if (HandShakeState.AllowVerification.HasFlag(this.State) == false)
                throw new SRP6.HandShakeException("Double Request");

            Logger.Debug("It was Claire whom received SRPVerification");

            // Hello I am the one that tries to connect. So let's generate the
            // value M2 I should have in the SRPPackedData Object.
            Byte[] M2 = SRPFunctions.CalcM2(_cache.A, _verification.M, _cache.K);

            // Compare
            if (!Utilities.ArraysEqual(M2, verification.M2))
            {
                this.State = HandShakeState.Failed;
                throw new HandShakeException("Username or password invalid.", new SRPException("Generated M2 does not match received M2"));
            }

            // Check expiration
            if (_cache.ExpirationTime.CompareTo(DateTime.Now) < 0)
            {
                this.State = HandShakeState.Expired;
                throw new HandShakeException("Hand was not shaken before it expired.");
            }

            // Set State
            this.State = HandShakeState.Verificated;
        }
    }
}
