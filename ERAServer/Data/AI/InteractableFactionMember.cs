using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ProjectERA.Protocols;

namespace ERAServer.Data.AI
{
    internal class InteractableFactionMember : InteractableComponent, IResetable
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public Byte FactionId
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public UInt64 Points
        {
            get;
            private set;
        }

        //TODO: Assignments
        //TODO: Point system

        /// <summary>
        /// 
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        internal static InteractableFactionMember Generate(Byte faction)
        {
            InteractableFactionMember result = new InteractableFactionMember();
            result.Id = ObjectId.GenerateNewId();
            result.FactionId = faction;
            result.Points = 0;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="faction"></param>
        /// <returns></returns>
        internal static InteractableFactionMember Generate(Interactable root, Byte faction)
        {
            InteractableFactionMember result = Generate(faction);
            result.Root = root;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Expire()
        {
            this.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.Id = ObjectId.Empty;
            this.Points = 0;
            this.Root = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        internal override void Pack(ref Lidgren.Network.NetOutgoingMessage msg)
        {
            msg.WriteRangedInteger(0, (Int32)InteractableAction.Max, (Int32)InteractableAction.None);
        }
    }
}
