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
        /// Create a response on received request.
        /// </summary>
        /// <param name="request">Receieved Request</param>
        /// <returns>SRPResponse</returns>
        public SRPResponse ResponseFromRequest(SRPRequest request)
        {
            if (this.State != HandShakeState.NotInitialized && HandShakeState.AllowResponse.HasFlag(this.State) == false)
                throw new SRP6.HandShakeException("Double Request");

            // Set State and start timer
            this.State = HandShakeState.Responding;
            _cache.ExpirationTime = DateTime.Now.AddSeconds(HandShake.ExpirationInSeconds);

            if (request.A.Mod(N).IntValue == 0)
            {
                this.State = HandShakeState.Failed;
                throw new HandShakeException("Request contains invalid data", new SRPException("A mod N is zero."));
            }

            Logger.Verbose("REMOTE A:" + request.A.ToString());

            Byte[] salt;
            NetBigInteger v = Lookup(request.Username, out salt);

            Logger.Verbose("salt:" + new NetBigInteger(salt).ToString());

            // Cache request
            _request = request;
            _cache.UserData = _request.Username;

            // Get public ket B from random private b
            _cache.b = SRPFunctions.Getb();
            _cache.B = SRPFunctions.CalcB(N, g, _cache.b, v);

            Logger.Verbose("b:" + _cache.b.ToString());
            Logger.Verbose("B:" + _cache.B.ToString());

            // Create the response message
            _response = new SRPResponse(salt, _cache.B);

            // First create the key
            KeyFromRequest(request.A, v);

            // Set State
            this.State = HandShakeState.Responded;

            return _response;
        }

        /// <summary>
        /// Generates key from request
        /// </summary>
        /// <param name="A">Generated A from request</param>
        /// <param name="v">Verifier v</param>
        private void KeyFromRequest(NetBigInteger A, NetBigInteger v)
        {
            // Shared random scrambler
            Lidgren.Network.NetBigInteger u = SRPFunctions.Calcu(A, _cache.B, N.ToString().Length);

            // Sessionkey
            _cache.S = SRPFunctions.CalcSServer(N, A, v, u, _cache.b);
            _cache.K = SRPFunctions.CalcK(_cache.S);

            Logger.Verbose("SERVER S:" + _cache.S.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verification"></param>
        private void VerificationOfActiveParty(SRPVerification verification)
        {
            if (HandShakeState.AllowVerificating.HasFlag(this.State) == false)
                throw new SRP6.HandShakeException("Double Request");

            // Set State
            this.State = HandShakeState.Verificating;
            Logger.Debug("It was Bob whom received SRPVerification");

            // Hello I am the one that is being connected to. So let's generate 
            // the value M I should have in the SRPPackedData Object.
            Byte[] M = SRPFunctions.CalcM(N, g, _request.Username, _response.Salt, _request.A, _cache.B, _cache.K);
            Logger.Verbose("A: " + _request.A.BitLength + "B: " + _cache.B.BitLength);

            // Compare
            if (!Utilities.ArraysEqual(M, verification.M))
            {
                this.State = HandShakeState.Failed;
                throw new HandShakeException("Invalid proof of Key. Username or password invalid.", new SRPException("Generated M does not match received M"));
            }

            // Ok, so their verification passed. Now let's proof that mine will to.
            _verification = new SRPVerification(SRPFunctions.CalcM2(_request.A, verification.M, _cache.K));

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
