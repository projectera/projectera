using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using ERAUtils;
using ProjectERA.Protocols;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;

namespace ERAServer.Data.AI
{
    internal class InteractableTeamMember : InteractableComponent, IResetable
    {
        /// <summary>
        /// Interactable Id
        /// </summary>
        [BsonRequired]
        public ObjectId InteractableId
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public DateTime MemberSince
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public static InteractableTeamMember Generate(ObjectId interactableId)
        {
            InteractableTeamMember result = new InteractableTeamMember();
            result.InteractableId = interactableId;
            result.MemberSince = DateTime.Now;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="interactableId"></param>
        /// <returns></returns>
        public static InteractableTeamMember Generate(Interactable root, ObjectId interactableId)
        {
            InteractableTeamMember result = Generate(interactableId);
            result.Root = root;
            return result;
        }

        public Task<Boolean> Join(Team team)
        {
            return team.AddMember(this);
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
            this.InteractableId = ObjectId.Empty;
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
