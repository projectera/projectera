﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using Lidgren.Network.Authentication;

namespace Lidgren.Network.Lobby
{
    public static class NetLobby
    {
        /// <summary>
        /// 
        /// </summary>
        public static Int32 KeySize = 1024;
        public static ILogonManager LogonManager;

        public delegate void HandshakeFinishedEvent(String reason);
        public static event HandshakeFinishedEvent OnDenied, OnSucces, OnExpired, OnError;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public static void Authenticate(NetConnection connection, String username, String password, Byte[] data = null)
        {
            var handshake = new Handshake(true, KeySize);
            var request = handshake.GenerateSRPRequest(username, password, data ?? new Byte[0]);

            var result = Create(connection, Handshake.Contents.Username, request.ByteSize);
            handshake.WriteSRPRequest(result);

            connection.SendMessage(result, NetDeliveryMethod.ReliableUnordered, 0);
            connection.Tag = handshake;

            Console.WriteLine("Autenticating with {0}:{1}", username, password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        public static void ReceiveAuthenticate(NetIncomingMessage message)
        {
            try
            {
                var handshake = new Handshake(false, KeySize, LogonManager);
                message.SenderConnection.Tag = handshake;
                var response = Handshake.HandshakeFromActive(message);
                var result = Create(message.SenderConnection, Handshake.Contents.Password, response.ByteSize);
                handshake.WriteSRPResponse(result);

                message.SenderConnection.SendMessage(result, NetDeliveryMethod.ReliableUnordered, 0);
                
                Console.WriteLine("Received with {0}", handshake.Username ?? handshake.UserData);
            }
            catch (NetSRP.HandShakeException ex)
            {
                ExceptionHandle(message, ex.Message);
                return;
            }

            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void ReceiveResponse(NetIncomingMessage message)
        {
            try
            {
                var verification = Handshake.HandshakeFromPassive(message);
            }
            catch (NetSRP.HandShakeException ex)
            {
                ExceptionHandle(message, ex.Message);
                return;
            }

            var result = Create(message.SenderConnection, Handshake.Contents.Verification, 21);
            (message.SenderConnection.Tag as Handshake).WriteSRPVerification(result);

            message.SenderConnection.SendMessage(result, NetDeliveryMethod.ReliableUnordered, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void ReceiveActiveVerification(NetIncomingMessage message)
        {
            try
            {
                var verification = Handshake.FinishHandshakeFromActive(message);
            }
            catch (NetSRP.HandShakeException ex)
            {
                ExceptionHandle(message, ex.Message);
                return;
            }

            var result = Create(message.SenderConnection, Handshake.Contents.Verification, 21);
            (message.SenderConnection.Tag as Handshake).WriteSRPVerification(result);

            message.SenderConnection.SendMessage(result, NetDeliveryMethod.ReliableUnordered, 0);

            // Finished!
            (message.SenderConnection.Tag as Handshake).MarkHandshakeAsSucceeded();

            if (OnSucces != null)
                OnSucces.Invoke("Authentication completed!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void ReceivePassiveVerification(NetIncomingMessage message)
        {
            try
            {
                var result = Handshake.FinishHandshakeFromPassive(message);
                // Finished!
            }
            catch (NetSRP.HandShakeException ex)
            {
                ExceptionHandle(message, ex.Message);
                return;
            }

            (message.SenderConnection.Tag as Handshake).MarkHandshakeAsSucceeded();

            if (OnSucces != null)
                OnSucces.Invoke("Authentication completed!");
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ReceiveFromExpired(NetIncomingMessage message)
        {
            var result = Create(message.SenderConnection, Handshake.Contents.Expired);
            message.SenderConnection.SendMessage(result, NetDeliveryMethod.ReliableUnordered, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void ExceptionHandle(NetIncomingMessage message, String reason)
        {
            Handshake.Contents contents;
            switch ((message.SenderConnection.Tag as Handshake).HandshakeState)
            {
                case Handshake.State.Failed:
                    contents = Handshake.Contents.Error;
                    if (OnError != null) OnError.Invoke(reason);
                    break;
                case Handshake.State.Expired:
                    contents = Handshake.Contents.Expired;
                    if (OnExpired != null) OnExpired.Invoke(reason);
                    break;
                case Handshake.State.Denied:
                    contents = Handshake.Contents.Denied;
                    if (OnDenied!= null) OnDenied.Invoke(reason);
                    break;

                default:
                    contents = Handshake.Contents.Error;
                    if (OnError != null) OnError.Invoke(reason);
                    break;
            }
            var result = Create(message.SenderConnection, contents);
            result.Write(reason);
            message.SenderConnection.SendMessage(result, NetDeliveryMethod.ReliableUnordered, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="reason"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static NetOutgoingMessage Create(NetConnection connection, Handshake.Contents reason, Int32 size = 4)
        {
            var message = connection.Peer.CreateMessage(size);
            message.Write((Byte)reason);
            return message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static Handshake.Contents IncomingMessage(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.Data:
                    var reasonByte = message.ReadByte();

                    var reason = (Handshake.Contents)reasonByte;
                    var handshake = message.SenderConnection.Tag as Handshake;
                    switch (reason)
                    {
                        case Handshake.Contents.Succes:
                            if (OnSucces != null) OnSucces.Invoke("Authentication complete!");
                            return reason;
                        case Handshake.Contents.Error:
                            if (OnError != null) OnError.Invoke(message.ReadString());
                            return reason;
                        case Handshake.Contents.Denied:
                            if (OnDenied != null) OnDenied.Invoke(message.ReadString());
                            return reason;
                    }
                    if (handshake == null)  // Server
                    {
                        switch (reason)
                        {
                            case Handshake.Contents.Username:
                                ReceiveAuthenticate(message);
                                break;
                            default:
                                // Can't happen!
                                throw new NetSRP.HandShakeException("Handshake not initialized when receiving " + reason.ToString() + " from client");
                        }
                        return reason;
                    }
                    switch (handshake.HandshakeState)
                    {
                        case Handshake.State.Expired:
                        case Handshake.State.Denied:
                        case Handshake.State.NotInitialized: // Server
                            switch (reason)
                            {
                                case Handshake.Contents.Username:
                                    ReceiveAuthenticate(message);
                                    break;
                                default:
                                    // Can't happen!
                                    throw new NetSRP.HandShakeException("Handshake not initialized when receiving " + reason.ToString() + " from client");
                            }
                            break;
                        case Handshake.State.Succeeded:
                            return Handshake.Contents.Succes;

                        case Handshake.State.Requesting: // Client
                            switch (reason)
                            {
                                case Handshake.Contents.Password:
                                    ReceiveResponse(message);
                                    break;
                                case Handshake.Contents.Expired:
                                    Authenticate(message.SenderConnection, handshake.Username, handshake.UserData);
                                    break;
                                default:
                                    // Can't happen!
                                    throw new NetSRP.HandShakeException("Expected response but received: " + reason.ToString() + " from server");
                            }
                            break;
                        case Handshake.State.Responding: // Server
                            switch (reason)
                            {
                                case Handshake.Contents.Verification:
                                    ReceiveActiveVerification(message);
                                    break;
                                case Handshake.Contents.Expired:
                                    ReceiveFromExpired(message);
                                    break;
                                default:
                                    // Can't happen!
                                    throw new NetSRP.HandShakeException("Expected verification but received: " + reason.ToString() + " from client ");
                            }
                            break;
                        case Handshake.State.Verificating: // Client
                            switch (reason)
                            {
                                case Handshake.Contents.Expired:
                                    Authenticate(message.SenderConnection, handshake.Username, handshake.UserData);
                                    break;
                                case Handshake.Contents.Verification:
                                    ReceivePassiveVerification(message);
                                    break;
                                default:
                                    // Can't happen!
                                    throw new NetSRP.HandShakeException("Expected completion but received: " + reason.ToString() + " from server ");
                            }
                            break;
                        default:
                            throw new NetSRP.HandShakeException("Expected nothing but received: " + reason.ToString());
                    }
                    if (handshake.HandshakeState == Handshake.State.Succeeded)
                        return Handshake.Contents.Succes;

                    return reason;
            }
            return Handshake.Contents.None;
        }
    }
}
