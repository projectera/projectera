using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using ERAUtils;
using ProjectERA.Protocols;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Interactable : Protocol
    {
        /// <summary>
        /// Message
        /// </summary>
        /// <param name="message"></param>
        internal static void Message(String message)
        {
            Byte[] bytes = Encoding.UTF8.GetBytes(message);
            NetOutgoingMessage msg = OutgoingMessage(InteractableAction.Message, 1 + bytes.Length);
            msg.Write(message);
            _connection.SendMessage(msg, NetDeliveryMethod.ReliableUnordered);
        }

        /// <summary>
        /// Unpacks an interactable object from an incoming message
        /// </summary>
        /// <remarks>Needs to be synchronized with Unpack on Sender</remarks>
        /// <param name="msg">Encoded interactable</param>
        /// <returns>Decoded interactable</returns>
        internal static ProjectERA.Data.Interactable Unpack(NetIncomingMessage msg)
        {
            ProjectERA.Data.Interactable result = Pool<ProjectERA.Data.Interactable>.Fetch();

            // Basics
            result.Id = (MongoObjectId)msg.ReadBytes(12);
            result.Name = msg.ReadString();
            result.StateFlags = (ERAUtils.Enum.InteractableStateFlags)msg.ReadByte();
            result.MapId = (MongoObjectId)msg.ReadBytes(12);
            result.MapX = msg.ReadInt32();
            result.MapY = msg.ReadInt32();

            // Components
            Int32 num = msg.ReadInt32();
            while (num-- > 0)
            {
                InteractableAction component = (InteractableAction)msg.ReadRangedInteger(0, (Int32)InteractableAction.Max);
                ProjectERA.Data.IInteractableComponent resultComponent = null;
                switch (component)
                {
                    // Appearance component
                    case InteractableAction.Appearance:
                        resultComponent = new ProjectERA.Data.InteractableAppearance(msg);
                        break;

                    // Movement component
                    case InteractableAction.Movement:
                        resultComponent = new ProjectERA.Data.InteractableMovement(msg);
                        break;

                    // Battler component
                    case InteractableAction.Battler:
                        resultComponent = new ProjectERA.Data.InteractableBattler(msg);
                        break;
                }

                if (resultComponent != null)
                    result.AddComponent(resultComponent);
            }

            /*if (result.HasAppearance && result.HasBattler)
            {
                foreach (var equipment in result.Battler)
                d{

                }
            }*/
            return result;
        }

        /// <summary>
        /// Gets the map protocol from the interactable connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        internal static Map GetMapProtocol(Connection connection)
        {
            Protocol protocol;
            if (connection.TryGetProtocol(typeof(Map), out protocol))
                return (Map)protocol;

            return null;
        }

        /// <summary>
        /// Caches an interactable
        /// </summary>
        /// <param name="interactable"></param>
        internal static void CacheInteractable(ProjectERA.Data.Interactable interactable)
        {
            InteractableRequest ir = new InteractableRequest(interactable.Id);
            ir.Result = interactable;
            
            //ir.TimeOut.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            GeneralCache<MongoObjectId, InteractableRequest>.UpdateCache(ir);
        }
    }
}
