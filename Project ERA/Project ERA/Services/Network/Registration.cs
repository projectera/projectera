using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ProjectERA.Services.Data.Storage;
using Microsoft.Xna.Framework;
using System.IO;
using ProjectERA.Services.Data;
using ERAUtils.Logger;
using SimpleDigest;
using Lidgren.Network.Authentication;

namespace ProjectERA.Services.Network
{
    internal class Registration
    {
        private const Int32 KeySize = 1024;

        private Status _registrationStatus;

        /// <summary>
        /// 
        /// </summary>
        internal Status RegistrationStatus
        {
            get { return _registrationStatus; }
            private set
            {
                if (IsAnonymous && value != Status.None && value != Status.RegistrationGenerated)
                    value |= IsAnonymous ? Status.Anonymous : 0;

                OnStatusChanged.Invoke(this, new StatusChangedEventArgs(value, _registrationStatus));
                _registrationStatus = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsBusy
        {
            get
            {
                if (RegistrationStatus == Status.None)
                    return false;
                if (RegistrationStatus == Status.Succes || RegistrationStatus == Status.Failure)
                    return false;
                if (RegistrationStatus == (Status.Succes | Status.Anonymous) || RegistrationStatus == (Status.Failure | Status.Anonymous))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsAnonymous
        {
            get
            {
                return RegistrationStatus.HasFlag(Status.Anonymous);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal event EventHandler<StatusChangedEventArgs> OnStatusChanged = delegate { };

        /// <summary>
        /// Registration Username
        /// </summary>
        internal String Username
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        private String Password
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        internal Game Game
        {
            get;
            set;
        }

        /// <summary>
        /// Generated Verifier
        /// </summary>
        protected Byte[] Verifier 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Generated Salt
        /// </summary>
        protected Byte[] Salt 
        {
            get { return _salt;  }
            private set { _salt = value; }
        }

        /// <summary>
        /// OnSucces action
        /// </summary>
        protected Action<String, String> Success
        {
            get;
            private set;
        }
        
        /// <summary>
        /// OnFailure action
        /// </summary>
        protected Action<Exception> Failure
        {
            get;
            private set;
        }

        private X509Certificate2 _certificate;
        private Byte[] _salt;
        protected Thread _thread;

        /// <summary>
        /// Disabled Constructor
        /// </summary>
        private Registration() { }

        /// <summary>
        /// Generates a new registration
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static Registration Generate(Game game, String username, String password) // TODO Email?
        {
            Registration result = new Registration();
            result.Game = game;
            result.Username = username;
            result.Password = password;
            result.Verifier = Handshake.PasswordVerifier(username.ToLower().Trim(), password, KeySize, out result._salt).ToByteArray();
            result.RegistrationStatus = Status.RegistrationGenerated;

            return result;
        }

        /// <summary>
        /// Generates a new anonymous registration
        /// </summary>
        /// <returns></returns>
        internal static Registration Generate(Game game)
        {
            Registration result = new Registration();
            result.Username = "Anonymous";
            result.Salt = new Byte[10];
            result.Verifier = new Byte[128];
            result.Game = game;
            result.RegistrationStatus = Status.RegistrationGenerated | Status.Anonymous;

            return result;
        }

        /// <summary>
        /// Starts new Registration 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="failure"></param>
        internal void Register(Action<String, String> success, Action<Exception> failure)
        {
            this.Success = success;
            this.Failure = failure;

            _thread = new Thread(new ThreadStart(Run));
            _thread.Start();
        }

        /// <summary>
        /// Cancel the registration
        /// </summary>
        internal void Cancel()
        {
            _thread.Abort();

            Failure.Invoke(new OperationCanceledException());
        }

        /// <summary>
        /// Internal thread run
        /// </summary>
        protected void Run()
        {
            SslStream stream = null;
            TcpClient client = null;

            try
            {
                // Retrieve address to register
                WebClient web = new WebClient();
                string server = web.DownloadString(NetworkManager.ServerRetrieveAddress);
                this.RegistrationStatus = Status.ServerReceived;

                // Setup a TCP client and Start SSL stream
                client = new TcpClient();
                try
                {
                    client.Connect(server, NetworkManager.ServerPort + 1);
                }
                catch (ArgumentOutOfRangeException)
                {
#if DEBUG
                    client.Connect("localhost", NetworkManager.ServerPort + 1);
#else
                    throw
#endif
                }
                this.RegistrationStatus = Status.Connected;

                // Loads certificate and sslStream
                _certificate = new X509Certificate2("./ssl.crt");
                stream = new SslStream(
                    client.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(RemoteValidation),
                    new LocalCertificateSelectionCallback(LocalCertificateSelection));

                // Authenticates as client, be sure the server certificate is loaded
                X509Certificate2Collection pubCollection = new X509Certificate2Collection(_certificate);
                stream.AuthenticateAsClient("server.projectera.org", pubCollection, System.Security.Authentication.SslProtocols.Default, true);
                this.RegistrationStatus = Status.Secured;

                Byte[] buffer = new Byte[128 + 128 + 10]; // 120 for username, /*8 for 2* CRC*/, 128 for verifier, 10 for salt
                Byte[] userbytes = Encoding.UTF8.GetBytes(this.Username);
                if (userbytes.Length > 120)
                    throw new FormatException("Username is too long!");

                Buffer.BlockCopy(userbytes, 0, buffer, 0, userbytes.Length);
                Buffer.BlockCopy(this.Verifier, 0, buffer, 128, 128);
                Buffer.BlockCopy(this.Salt, 0, buffer, 256, 10);

                stream.Write(buffer);
                stream.Flush();
                this.RegistrationStatus = Status.RequestSent;

                if (this.Username == "Anonymous")
                {
                    // Get generated username
                    Byte[] buffer2 = new Byte[20]; 
                    Int32 readleft = buffer2.Length;
                    while (readleft > 0)
                        readleft -= stream.Read(buffer2, buffer2.Length - readleft, readleft);

                    // Convert to string
                    StringBuilder b = new StringBuilder(2 * buffer2.Length);
                    for (int i = 0; i < buffer2.Length; i++)
                        b.Append(buffer2[i].ToString("x2"));

                    this.Username = b.ToString().ToLower().Trim();
                    Logger.Verbose("Registration username: " + this.Username);
                    this.RegistrationStatus = Status.ResponseSecondaryReceived;

                    // Generate password
                    using(System.Security.Cryptography.RNGCryptoServiceProvider random = new System.Security.Cryptography.RNGCryptoServiceProvider())
                        random.GetBytes(buffer2);

                    // Convert to string
                    b = new StringBuilder(2 * buffer2.Length);
                    for (int i = 0; i < buffer2.Length; i++)
                        b.Append(buffer2[i].ToString("x2"));

                    this.Password = b.ToString().Trim();
                    Logger.Verbose("Registration password: " + this.Password);

                    // Generate verifier and salt
                    this.Verifier = Handshake.PasswordVerifier(this.Username, this.Password, KeySize, out _salt).ToByteArray();

                    /*b = new StringBuilder(2 * _salt.Length);
                    for (int i = 0; i < _salt.Length; i++)
                        b.Append(this.Verifier[i].ToString("x2"));
                    Logger.Verbose("Registration salt: " + b.ToString());

                    b = new StringBuilder(2 * this.Verifier.Length);
                    for (int i = 0; i < this.Verifier.Length; i++)
                        b.Append(this.Verifier[i].ToString("x2"));
                    Logger.Verbose("Registration verifier: " + b.ToString());*/

                    // Resend
                    Buffer.BlockCopy(Encoding.UTF8.GetBytes(this.Username), 0, buffer, 0, Encoding.UTF8.GetBytes(this.Username).Length);
                    Buffer.BlockCopy(this.Verifier, 0, buffer, 128, 128);
                    Buffer.BlockCopy(this.Salt, 0, buffer, 256, 10);
                    stream.Write(buffer);
                    stream.Flush();
                    this.RegistrationStatus = Status.RequestSecondarySent;

                    using (Crc32 crc32 = new Crc32())
                    {
                        String hash = String.Empty;
                        foreach (Byte hashByte in crc32.ComputeHash(buffer)) hash += hashByte.ToString("x2").ToLower();
                        Logger.Debug(String.Format("CRC-32 is {0}", hash));
                    }
                }

                // Get result byte
                buffer = new Byte[1];
                stream.Read(buffer, 0, 1);
                stream.Close();
                client.Close();
                this.RegistrationStatus = Status.ResponseReceived;

                switch (buffer[0])
                {
                    case 1:
                        Success.Invoke(this.Username, this.Password);
                        this.RegistrationStatus = Status.Succes;
                        break;
                    case 2:
                        throw new Exception("User already exists!");
                    default:
                        throw new Exception("Unknown error!");
                }
            }
            catch (Exception e)
            {
                Failure.Invoke(e);
                this.RegistrationStatus = Status.Failure;

            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }

        /// <summary>
        /// Verify the remote certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        private Boolean RemoteValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
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
            return _certificate;
        }

        /// <summary>
        /// 
        /// </summary>
        internal enum Status
        {
            None = 0,

            /// <summary>
            /// Session is anonymous
            /// </summary>
            Anonymous = 1,

            /// <summary>
            /// Object is generates
            /// </summary>
            RegistrationGenerated = 2,

            /// <summary>
            /// Endpoint retrieved
            /// </summary>
            ServerReceived = 4,

            /// <summary>
            /// TCP connection is set up
            /// </summary>
            Connected = 6,

            /// <summary>
            /// SSL Authentication
            /// </summary>
            Secured = 8,

            /// <summary>
            /// Registration request
            /// </summary>
            RequestSent = 10,

            /// <summary>
            /// Registration response (anon)
            /// </summary>
            ResponseSecondaryReceived = 16,

            /// <summary>
            /// Password sent (anon)
            /// </summary>
            RequestSecondarySent = 12,

            /// <summary>
            /// Registration response
            /// </summary>
            ResponseReceived = 14,

            /// <summary>
            /// 
            /// </summary>
            Succes = 20,

            /// <summary>
            /// 
            /// </summary>
            Failure = 22,
        }

        /// <summary>
        /// 
        /// </summary>
        internal class StatusChangedEventArgs : EventArgs
        {
            internal Status Current;
            internal Status Previous;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="current"></param>
            /// <param name="previous"></param>
            internal StatusChangedEventArgs(Status current, Status previous)
                : base()
            {
                this.Current = current;
                this.Previous = previous;
            }
        }
    }
}
