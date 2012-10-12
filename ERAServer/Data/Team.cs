using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ERAServer.Data.AI;
using System.Threading.Tasks;
using System.Threading;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Collections;
using ERAServer.Services;

namespace ERAServer.Data
{
    internal class Team : IEnumerable
    {
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
        public HashSet<InteractableTeamMember> Members
        {
            get;
            private set;
        }

        private ReaderWriterLockSlim _membersRWLock;

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public InteractableTeamMember Creator
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public DateTime CreationDate
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates a Team
        /// </summary>
        /// <param name="creator"></param>
        /// <returns>Generated Team</returns>
        internal static Team Generate(ObjectId creator)
        {
            Team result = new Team();

            // Set guild values
            result.Id = ObjectId.GenerateNewId();
            result.Creator = InteractableTeamMember.Generate(creator);
            result.Members = new HashSet<InteractableTeamMember> { result.Creator };
            result.CreationDate = DateTime.Now;

            result._membersRWLock = new ReaderWriterLockSlim();

            return result;
        }

        /// <summary>
        /// Adds Team Member to Team
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal Task<Boolean> AddMember(InteractableTeamMember member)
        {
            if (IsMember(member))
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
        /// Adds Guild Member to Guild (not pending!)
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal Task<Boolean> RemoveMember(InteractableTeamMember member)
        {
            if (!IsMember(member))
                return Task<Boolean>.Factory.StartNew(() => { return false; });

            // Update remotely
            ObjectId updateId = this.Id;

            // Adding
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
                }
                finally
                {
                    if (_membersRWLock.IsWriteLockHeld)
                        _membersRWLock.ExitWriteLock();
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        private bool IsMember(InteractableTeamMember member)
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

        #region Database Get/Put operations
        /// <summary>
        /// Gets a Team from the db
        /// </summary>
        /// <param name="id">id of Team to get</param>
        /// <returns></returns>
        internal static Task<Team> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }


        /// <summary>
        /// Gets a Team from the db, blocks while retrieving
        /// </summary>
        /// <param name="id">id of Guild to get</param>
        /// <returns></returns>
        internal static Team GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as Team;
        }

        /// <summary>
        /// Gets the Teams collection
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Team> GetCollection()
        {
            return DataManager.Database.GetCollection<Team>("Teams");
        }

        /// <summary>
        /// Puts a Team to the db
        /// </summary>
        internal void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a Team to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Team>(this, safemode);
        }
        #endregion

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
