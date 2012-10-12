using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace ERAServer.Data.AI
{
    /// <summary>
    /// 
    /// </summary>
    [BsonDiscriminator(RootClass=true)]
    [BsonKnownTypes(typeof(InteractableAppearance), typeof(InteractableBattler), typeof(InteractableFactionMember), 
        typeof(InteractableGuildMember), typeof(InteractableMovement), typeof(InteractableTeamMember))]
    internal abstract class InteractableComponent
    {
        /// <summary>
        /// 
        /// </summary>
        internal Interactable Root
        {
            get;
            set;
        }

        /// <summary>
        /// Expires the component, usually calling IResetable functions
        /// </summary>
        internal abstract void Expire();

        /// <summary>
        /// Encodes the component
        /// </summary>
        /// <param name="component"></param>
        /// <param name="msg"></param>
        internal abstract void Pack(ref Lidgren.Network.NetOutgoingMessage msg);
    }
}
