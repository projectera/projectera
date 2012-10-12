using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ERAUtils.Enum;
using System.Threading.Tasks;
using ERAUtils;
using ProjectERA.Protocols;

namespace ERAServer.Data.AI
{
    [Serializable]
    internal class InteractableGuildMember : InteractableComponent, IResetable
    {
        /// <summary>
        /// Interactable Id
        /// </summary>
        [BsonId]
        public ObjectId InteractableId
        {
            get;
            private set;
        }

        /// <summary>
        /// Permissions of this member
        /// </summary>
        [BsonRequired]
        public GuildPermissionGroup Permissions
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public UInt64 Rank // TODO: figure out ranking system
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Member since 
        /// </summary>
        [BsonRequired]
        public DateTime MemberSince
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates a guildmember
        /// </summary>
        /// <param name="avatarId">id of member</param>
        /// <param name="permission">permissions</param>
        /// <returns></returns>
        internal static InteractableGuildMember Generate(ObjectId avatarId, GuildPermissionGroup permission)
        {
            InteractableGuildMember result = new InteractableGuildMember();

            // GuildMember values
            result.InteractableId = avatarId;
            result.Permissions = permission;
            result.MemberSince = DateTime.Now;

            // Default values
            result.Rank = 0;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="avatarId"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        internal static InteractableGuildMember Generate(Interactable root, ObjectId avatarId, GuildPermissionGroup permission)
        {
            InteractableGuildMember result = Generate(avatarId, permission);
            result.Root = root;
            return result;
        }

        /// <summary>
        /// Generates a guildmember with Pending PermissionGroup
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal static InteractableGuildMember Generate(ObjectId avatarId)
        {
            return Generate(avatarId, GuildPermissionGroup.Pending);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal static InteractableGuildMember Generate(Interactable root,  ObjectId avatarId)
        {
            InteractableGuildMember result = Generate(avatarId);
            result.Root = root;
            return result;
        }

        /// <summary>
        /// Joins a guild (pending)
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        internal Task<Boolean> Join(Guild guild)
        {
            return guild.AddPendingMember(this);
        }

        /// <summary>
        /// Leaves a guild (must be member of that guild)
        /// </summary>
        /// <param name="guild"></param>
        /// <returns></returns>
        internal Task<Boolean> Leave(Guild guild)
        {
            return guild.RemoveMember(this);
        }

        /// <summary>
        /// If permissions are pending, returns true
        /// </summary>
        public Boolean IsPending
        {
            get { return this.Permissions.HasFlag(GuildPermissionGroup.Pending); }
        }

        /// <summary>
        /// If permissions are member, returns true
        /// </summary>
        public Boolean IsMember 
        { 
            get { return this.Permissions.HasFlag(GuildPermissionGroup.Member); } 
        }

        /// <summary>
        /// If permission are founder, returns true
        /// </summary>
        public Boolean IsFounder 
        { 
            get { return this.Permissions.HasFlag(GuildPermissionGroup.Founder); } 
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
            this.Permissions = GuildPermissionGroup.None;
            this.MemberSince = DateTime.MinValue;
            this.Rank = 0;
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
