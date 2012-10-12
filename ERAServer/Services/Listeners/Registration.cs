using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using ERAUtils.Logger;
using SimpleDigest;

namespace ERAServer.Services.Listeners
{
    internal class Registration
    {
        /// <summary>
        /// Is true when the registration server is running, set to false to kill the registration server.
        /// </summary>
        public Boolean IsRunning { get; set; }

        private Thread thread;
        private TcpListener server;

        private X509Certificate2 certificate;

        // TODO timeout

        /// <summary>
        /// Creates a new registration server and runs it on a different thread.
        /// </summary>
        public Registration()
        {
            certificate = new X509Certificate2(ERAServer.Properties.Settings.Default.RegistrationCertificate,
                ERAServer.Properties.Settings.Default.RegistrationCertificatePassphrase);
            
            this.IsRunning = true;

            thread = new Thread(new ThreadStart(Run));
            thread.Priority = ThreadPriority.BelowNormal;
            thread.Start();
        }

        /// <summary>
        /// Runs the registration server
        /// </summary>
        private void Run()
        {
            server = new TcpListener(IPAddress.Any, 15937);
            server.Start();

            Logger.Info("Registration Service is Running.");

            while (this.IsRunning)
            {
                if (!server.Pending())
                {
                    Thread.Sleep(50);
                    continue;
                }

                // Accept connection and setup SSL
                TcpClient incoming = server.AcceptTcpClient();
                SslStream ssl = new SslStream(incoming.GetStream(), 
                    false, 
                    new RemoteCertificateValidationCallback(RemoteValidation), 
                    new LocalCertificateSelectionCallback(LocalCertificateSelection)
                );

                Logger.Debug("Registration request started.");

                // Start authenticating
                ssl.BeginAuthenticateAsServer(certificate, 
                    new AsyncCallback(AsyncAuthenticate),
                    new RegistrationState() { Client = incoming, Stream = ssl });

            }

            Logger.Info("Registration Service is Stopping.");
        }

        /// <summary>
        /// Authentication with the client finished
        /// </summary>
        /// <param name="result"></param>
        private void AsyncAuthenticate(IAsyncResult result)
        {
            RegistrationState state = result.AsyncState as RegistrationState;

            // Error that shouldn't happen
            if (state == null)
                return;

            try
            {
                state.Stream.EndAuthenticateAsServer(result);
                state.Readleft = 128 + 128 + 10; //128 bytes for login name, 128 bytes for verifier, 10 byte for salt.
                state.Buffer = new byte[state.Readleft];
                state.Stream.BeginRead(state.Buffer, state.Position, state.Readleft, new AsyncCallback(AsyncReadCredentials), state);
            }
            catch (Exception error)
            {
                Logger.Warning("AsyncAuthenticate error: " + error);

                if (state.Stream != null)
                    state.Stream.Close();
                if (state.Client != null && state.Client.Connected)
                    state.Client.Close();
                return;
            }
        }

        /// <summary>
        /// Credentials have been received
        /// </summary>
        /// <param name="result"></param>
        private void AsyncReadCredentials(IAsyncResult result)
        {
            RegistrationState state = (RegistrationState)result.AsyncState;
            if (!state.CheckFinished(result, new AsyncCallback(AsyncReadCredentials)))
                return;

            // Finished reading credentials
            try
            {
                String username = Encoding.UTF8.GetString(state.Buffer, 0, 128).TrimEnd('\0');
                Byte[] verifier = new Byte[128];
                Byte[] salt = new Byte[10];

                Buffer.BlockCopy(state.Buffer, 128, verifier, 0, 128);
                Buffer.BlockCopy(state.Buffer, 256, salt, 0, 10);

                Logger.Verbose("Registration username: " + username);

                /*StringBuilder b = new StringBuilder(2 * salt.Length);
                for (int i = 0; i < salt.Length; i++)
                    b.Append(salt[i].ToString("x2"));
                Logger.Verbose("Registration salt: " + b.ToString());

                b = new StringBuilder(2 * verifier.Length);
                for (int i = 0; i < verifier.Length; i++)
                    b.Append(verifier[i].ToString("x2"));
                Logger.Verbose("Registration verifier: " + b.ToString());*/

                // If anonymous, generate a username, send it back and retry.
                if (username == "Anonymous")
                {
                    // Generate anonymous username and send it to the client
                    Byte[] buffer = SimpleDigest.SHA1.InternalHash("Anonymous" + DateTime.Now.ToString() + ERAUtils.Environment.LongMachineId.ToString() + Lidgren.Network.NetRandom.Instance.NextUInt().ToString());
                    for (Int32 i = 0; i < buffer.Length; i++)
                        state.Buffer[i] = buffer[i];

                    state.Stream.BeginWrite(buffer, 0, 20, new AsyncCallback(AsyncUsernameSent), state);
                    return;
                }

                Crc32 crc32 = new Crc32();
                String hash = String.Empty;
                foreach (byte b in crc32.ComputeHash(state.Buffer)) hash += b.ToString("x2").ToLower();
                Logger.Debug(String.Format("CRC-32 is {0}", hash));

                // We got all the data we needed, now let's try to register
                try
                {
                    Data.Player registratee = Data.Player.Generate(username, verifier, salt); 
                    Data.Interactable avatar = Data.Interactable.GenerateAvatar(registratee);
                    registratee.Put();
                    avatar.Put();
                }
                catch (Exception)
                {
                    SendFailure(state); // duplicate
                    return;
                }

                // We survived! Send succes
                SendSuccess(state);
                
            }
            catch (Exception error)
            {
                Logger.Warning("AsyncReadCredentials error: " + error);

                if (state.Stream != null)
                    state.Stream.Close();
                if (state.Client != null && state.Client.Connected)
                    state.Client.Close();
                return;
            }
        }

        /// <summary>
        /// We sent the username to the client
        /// </summary>
        /// <param name="result"></param>
        private void AsyncUsernameSent(IAsyncResult result)
        {
            RegistrationState state = (RegistrationState)result.AsyncState;

            try
            {
                state.Stream.EndWrite(result);
                state.Stream.Flush();
            }
            catch (Exception error)
            {
                Logger.Warning("AsyncUsernameSent error: " + error);

                if (state.Stream != null)
                    state.Stream.Close();
                if (state.Client != null && state.Client.Connected)
                    state.Client.Close();
                return;
            }

            // And now reread the credentials still needed
            state.Position = 0;
            state.Readleft = 128 + 128 + 10; // 128 bytes for login name, 128 bytes for verifier, 10 byte for salt.
            state.Buffer = new byte[state.Readleft];
            //state.Position = 128;       // We already read the login name
            //state.Readleft = 128 + 10;  // 128 bytes for verifier, 10 byte for salt.
            state.Stream.BeginRead(state.Buffer, state.Position, state.Readleft, new AsyncCallback(AsyncReadCredentials), state);
        }

        /// <summary>
        /// Sends a success message to the client.
        /// </summary>
        /// <param name="state"></param>
        private void SendSuccess(RegistrationState state)
        {
            try
            {
                Byte[] buffer = new Byte[] { 1 };
                state.Stream.BeginWrite(buffer, 0, 1, new AsyncCallback(AsyncResultSent), state);
            }
            catch (Exception error)
            {
                Logger.Warning("SendSuccess error: " + error);

                if (state.Stream != null)
                    state.Stream.Close();
                if (state.Client != null && state.Client.Connected)
                    state.Client.Close();
                return;
            }
        }

        /// <summary>
        /// Sends a failure message to the client.
        /// </summary>
        /// <param name="state"></param>
        private void SendFailure(RegistrationState state)
        {
            try
            {
                Byte[] buffer = new Byte[] { 2 };
                state.Stream.BeginWrite(buffer, 0, 1, new AsyncCallback(AsyncResultSent), state);
            }
            catch (Exception error)
            {
                Logger.Warning("SendFailure error: " + error);

                if (state.Stream != null)
                    state.Stream.Close();
                if (state.Client != null && state.Client.Connected)
                    state.Client.Close();
                return;
            }
        }

        /// <summary>
        /// Message has been sent to the client, close the connection
        /// </summary>
        /// <param name="result"></param>
        private void AsyncResultSent(IAsyncResult result)
        {
            RegistrationState state = (RegistrationState)result.AsyncState;
            try
            {
                state.Stream.EndWrite(result);
            }
            catch (Exception error) 
            {
                Logger.Warning("AsyncResultSent error: " + error);

                if (state.Stream != null)
                    state.Stream.Close();
                if (state.Client != null && state.Client.Connected)
                    state.Client.Close();
                return;
            }

            // Disconnect
            if (state.Stream != null)
                state.Stream.Close();
            if (state.Client != null && state.Client.Connected)
                state.Client.Close();

            Logger.Debug("Registion request completed.");
        }

        /// <summary>
        /// Verify the remote certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private bool RemoteValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                // Client does not need to authenticate
                if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable)
                    return true;

                return false;
            }
            return true;
        }

        /// <summary>
        /// Select a local certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="targetHost"></param>
        /// <param name="localCertificates"></param>
        /// <param name="remoteCertificate"></param>
        /// <param name="acceptableIssuers"></param>
        /// <returns></returns>
        private X509Certificate LocalCertificateSelection(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return certificate;
        }

        /// <summary>
        /// Holds the state of a registration
        /// </summary>
        private class RegistrationState
        {
            public TcpClient Client;
            public SslStream Stream;
            public Byte[] Buffer;
            public Int32 Position = 0;
            public Int32 Readleft = 0;

            /// <summary>
            /// Makes sure all data has been received.
            /// </summary>
            /// <param name="result"></param>
            /// <param name="callback">The Async function to call if we haven't received all data yet</param>
            /// <returns>Wether there is still data to be received</returns>
            public bool CheckFinished(IAsyncResult result, AsyncCallback callback)
            {
                Int32 length = 0;
                try
                {
                    length = Stream.EndRead(result);
                }
                catch (Exception)
                {
                    this.Stream.Close();
                    this.Client.Close();
                    return true;
                }

                // If all is read
                if (length == this.Readleft)
                {
                    this.Readleft -= length;
                    this.Position += length;
                    return true;
                }

                // If not
                this.Readleft -= length;
                this.Position += length;

                this.Stream.BeginRead(Buffer, Position, Readleft, callback, this);

                return false;
            }
        }
    }
}
