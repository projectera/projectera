using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using ERAUtils;
using ProjectERA.Protocols;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Player : Protocol
    {
        /// <summary>
        /// Message 
        /// </summary>
        /// <param name="message"></param>
        internal static void Message(MongoObjectId dialogue, String message)
        {
            Byte[] bytes = Encoding.UTF8.GetBytes(message);
            NetOutgoingMessage msg = OutgoingMessage(PlayerAction.Message, 13 + bytes.Length);
            msg.Write(dialogue.Id);
            msg.Write(message);
            _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Message Attachment
        /// </summary>
        /// <param name="message"></param>
        internal static void MessageAttachment(MongoObjectId messageId)
        {
            NetOutgoingMessage msg = OutgoingMessage(PlayerAction.MessageAttachment, 12);
            msg.Write(messageId.Id);
            _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Unpacks a player object from an incoming message
        /// </summary>
        /// <remarks>Needs to be synchronized with Pack on Sender</remarks>
        /// <param name="msg">Packed player</param>
        /// <returns>Unpacked player</returns>
        internal static ProjectERA.Data.Player Unpack(NetIncomingMessage msg)
        {
            ProjectERA.Data.Player result = Pool<ProjectERA.Data.Player>.Fetch();

            result.UserId = (MongoObjectId)msg.ReadBytes(12);

            result.ForumId = msg.ReadUInt16();
            result.Name = msg.ReadString();
            result.Email = msg.ReadString();

            Int32 numAvatars = msg.ReadInt32();
            while (numAvatars-- > 0)
            {
                ProjectERA.Data.Interactable avatar = Pool<ProjectERA.Data.Interactable>.Fetch();

                avatar.Id = (MongoObjectId)msg.ReadBytes(12);
                result.Avatars.Add(avatar);
            }

            return result;
        }

    }
}
