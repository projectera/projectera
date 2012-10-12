using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ERAUtils.Enum;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Threading;
using ERAServer.Data.AI;
using ERAUtils;
using System.Collections;
using ERAServer.Services;

namespace ERAServer.Data
{
    [Serializable]
    internal class Guild : IResetable, IEnumerable
    {
        /// <summary>
        /// Guild ID
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Founder ID
        /// </summary>
        [BsonRequired]
        public InteractableGuildMember Founder
        {
            get;
            private set;
        }
        
        /// <summary>
        /// Guild Members
        /// </summary>
        [BsonRequired]
        public HashSet<InteractableGuildMember> Members
        {
            get;
            private set;
        }

        /// <summary>
        /// Pending Guild Members
        /// </summary>
        internal IEnumerable<InteractableGuildMember> PendingMembers
        {
            get
            {
                _membersRWLock.EnterReadLock();
                IEnumerable<InteractableGuildMember> result = Members.Where(mem => mem.IsPending);
                _membersRWLock.ExitReadLock();
                
                return result;
            }
        }

        private ReaderWriterLockSlim _membersRWLock;

        /// <summary>
        /// Guild Name
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public UInt64 Points //TODO split into GuildPoints and several fields
        {
            get;
            private set;
        }

        /// <summary>
        /// Founded Time
        /// </summary>
        [BsonRequired]
        public DateTime FoundedDate
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates a Guild
        /// </summary>
        /// <param name="name">Name of the Guild</param>
        /// <param name="founder">Guild founder id</param>
        /// <returns>Generated Guild</returns>
        internal static Guild Generate(String name, ObjectId founder)
        {
            Guild result = Guild.GetBlocking(name) ?? new Guild();
            if (result.Id.Equals(ObjectId.Empty) == false)
                throw new Exception("Guild already exists");

            // Set guild values
            result.Id = ObjectId.GenerateNewId();
            result.Founder = InteractableGuildMember.Generate(founder, GuildPermissionGroup.Founder);
            result.Name = name;
            result.Members = new HashSet<InteractableGuildMember> { result.Founder };
            result.FoundedDate = DateTime.Now;

            result._membersRWLock = new ReaderWriterLockSlim();

            return result;
        }

        /// <summary>
        /// Generates a guild
        /// </summary>
        /// <param name="name">Name of the guild</param>
        /// <param name="founder">Guild founder</param>
        /// <returns>Generated Guil</returns>
        internal static Guild Generate(String name, Interactable founder)
        {
            return Generate(name, founder.Id);
        }

        /// <summary>
        /// Adds Guild Member to Guild (not pending!)
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal Task<Boolean> AddMember(ObjectId avatarId)
        {
            if (IsMember(avatarId))
                return Task<Boolean>.Factory.StartNew(() => { return false; });

            // Update remotely
            ObjectId updateId = this.Id;
            InteractableGuildMember member = InteractableGuildMember.Generate(avatarId, GuildPermissionGroup.Member);

            // Adding
            return Task<Boolean>.Factory.StartNew(() =>
            {
                try
                {
                    _membersRWLock.EnterWriteLock();

                    if (this.Members.Add(member))
                    {
                        SafeModeResult sfr = GetCollection().Update(Query.EQ("_id", updateId), Update.AddToSet("Members", member.ToBsonDocument()), SafeMode.True);

                        // Return succession flag
                        return sfr.UpdatedExisting;
                    }

                    return false;
                } finally {
                    _membersRWLock.ExitWriteLock();
                }
            });
        }

        /// <summary>
        /// Adds Guild Member to Guild (pending)
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal Task<Boolean> AddPendingMember(InteractableGuildMember member)
        {
            if (IsMember(member) || member.IsPending == false)
                return Task<Boolean>.Factory.StartNew(() => { return false; });

            // Update remotely
            ObjectId updateId = this.Id;

            // Adding
            return Task<Boolean>.Factory.StartNew(() =>
            {
                try
                {
                    _membersRWLock.EnterWriteLock();

                    if (this.Members.Add(member))
                    {
                        SafeModeResult sfr = GetCollection().Update(Query.EQ("_id", updateId), Update.AddToSet("Members", member.ToBsonDocument()), SafeMode.True);

                        // Return succession flag
                        return sfr.UpdatedExisting;
                    }

                    return false;
                }
                finally
                {
                    _membersRWLock.ExitWriteLock();
                }
            });
        }

        /// <summary>
        /// Removes Guild Member from Guild
        /// </summary>
        /// <param name="member">Member to remove</param>
        /// <returns></returns>
        internal Task<Boolean> RemoveMember(InteractableGuildMember member)
        {
            if (IsMember(member) == false)
                return Task<Boolean>.Factory.StartNew(() => { return false; });

            // Update remotely
            ObjectId updateId = this.Id;

            return Task<Boolean>.Factory.StartNew(() =>
            {
                try
                {
                    _membersRWLock.EnterWriteLock();

                    if (this.Members.Remove(member))
                    {

                        SafeModeResult sfr = GetCollection().Update(Query.EQ("_id", updateId), Update.Pull("Members", member.ToBsonDocument()), SafeMode.True);
                        // Return succession flag
                        return sfr.UpdatedExisting;
                    }

                    return false;

                } finally {
                    if (_membersRWLock.IsWriteLockHeld)
                        _membersRWLock.ExitWriteLock(); 
                }

            });
        }

        /// <summary>
        /// Removes Guild Member from Guild
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal Task<Boolean> RemoveMember(ObjectId avatarId)
        {
            if (IsMember(avatarId) == false)
                return Task<Boolean>.Factory.StartNew(() => { return false; });

            // Finding
            InteractableGuildMember member = FindMember(avatarId);

            // Removing
            return RemoveMember(member);
        }

        /// <summary>
        /// Finds member with id
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal InteractableGuildMember FindMember(ObjectId avatarId)
        {
            try
            {
                _membersRWLock.EnterReadLock();
                return this.Members.Single((gm) => gm.InteractableId == avatarId);
            }
            finally
            {
                _membersRWLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Elevates guildmember with avatarId to permission
        /// </summary>
        /// <param name="avatarId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        internal Task<Boolean> ElevateMember(ObjectId avatarId, GuildPermissionGroup permissions)
        {
            if (IsMember(avatarId) == false)
                return Task<Boolean>.Factory.StartNew(() => { return false; });

            // Finding
            InteractableGuildMember member = FindMember(avatarId);

            // Elevating
            return ElevateMember(member, permissions);
        }

        /// <summary>
        /// Elevates guildmember to permission
        /// </summary>
        /// <param name="member"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        internal Task<Boolean> ElevateMember(InteractableGuildMember member, GuildPermissionGroup permissions)
        {
            // Can not remove founder permission from founder
            if (this.IsFounder(member) && !permissions.HasFlag(GuildPermissionGroup.Founder))
                return Task<Boolean>.Factory.StartNew(() => { return false; });

            // Update remotely
            ObjectId updateId = this.Id;
            
            return Task<Boolean>.Factory.StartNew(() =>
            {
                // Elevating
                try
                {
                    _membersRWLock.EnterWriteLock();
                    member.Permissions = permissions;

                    SafeModeResult sfr = GetCollection().Update(Query.And(Query.EQ("_id", updateId), Query.EQ("Members._id", member.InteractableId)), 
                        Update.Set("Members.$.Permissions", permissions), SafeMode.True); 

                    // Return succession flag
                    return sfr.Ok;
                }
                finally
                {
                    if (_membersRWLock.IsWriteLockHeld)
                        _membersRWLock.ExitWriteLock();
                }
            });
        }

        /// <summary>
        /// True if any member has the avatarId
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal Boolean IsMember(ObjectId avatarId)
        {
            try
            {
                _membersRWLock.EnterReadLock();
                return Members.Any(gm => gm.InteractableId.Equals(avatarId));
            }
            finally
            {
                _membersRWLock.ExitReadLock();
            }
        }

        /// <summary>
        /// True if member is a member of this guild
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal Boolean IsMember(InteractableGuildMember member)
        {
            try
            {
                _membersRWLock.EnterReadLock();
                return Members.Any(gm => gm.Equals(member));
            }
            finally
            {
                _membersRWLock.ExitReadLock();
            }
        }

        /// <summary>
        /// True if avatar Id is the id of the founder of this Guild
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal Boolean IsFounder(ObjectId avatarId)
        {
            return Founder.InteractableId.Equals(avatarId);
        }

        /// <summary>
        /// True if member is the founder of this Guild
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        internal Boolean IsFounder(InteractableGuildMember member)
        {
            return Founder.Equals(member);
        }

        #region Database Get/Put operations
        /// <summary>
        /// Gets a guild from the db
        /// </summary>
        /// <param name="id">id of Guild to get</param>
        /// <returns></returns>
        internal static Task<Guild> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets a guild from the db
        /// </summary>
        /// <param name="username">name of Guild to get</param>
        /// <returns></returns>
        internal static Task<Guild> Get(String name)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Gets a guild from the db, blocks while retrieving
        /// </summary>
        /// <param name="id">id of Guild to get</param>
        /// <returns></returns>
        internal static Guild GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as Guild;
        }

        /// <summary>
        /// Gets a guild from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">name of guild to get</param>
        /// <returns></returns>
        internal static Guild GetBlocking(String name)
        {
            return GetCollection().FindOneAs<Guild>(Query.Matches("Name", new BsonRegularExpression("^(?i)" + name + "$"))) as Guild;
        }

        /// <summary>
        /// Gets the guilds collection
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Guild> GetCollection()
        {
            return DataManager.Database.GetCollection<Guild>("Guilds");
        }

        /// <summary>
        /// Puts a guild to the db
        /// </summary>
        internal void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a guild to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Guild>(this, safemode);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            this.Id = ObjectId.Empty;
            this.Name = String.Empty;
            //this.Founder.Clear();
            this.Founder = null;
            this.FoundedDate = DateTime.MinValue;
            // Clear all members
            this.Members.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void EnterReadLock()
        {
            _membersRWLock.EnterReadLock();
        }

        /// <summary>
        /// 
        /// </summary>
        internal void ExitReadLock()
        {
            _membersRWLock.ExitReadLock();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return this.Members.GetEnumerator();
        }
    }
}
