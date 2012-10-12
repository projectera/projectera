using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ERAServer.Data.Enum;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson.Serialization.Attributes;

namespace ERAServer.Data
{
    [Serializable]
    internal class Avatar
    {
        // Ids
        [BsonId]
        public ObjectId Id;
        [BsonRequired]
        public ObjectId UserId;
        public ObjectId GuildId, TeamId;
        public Byte RaceId, ClassId, FactionId;

        // Map and Movement
        public Int32 MapId, MapX, MapY, TileId;
        public Int32 MoveSpeed, MoveFrequency, StopFrequency;
        public Byte MapDir, AnimationFrame;
        public Int32 RespawnId;

        // InteractableFlags
        public InteractableFlags Flags;
        public String AssetName; //private String _graphicName;
        public String Name;

        // AvatarBody (maybe as object?)
        public AvatarBody Body;

        // Battler
        //public AvatarStats _stats;
        //public AvatarEquipment _equipment;
        //public AvatarSynths _synths;
        //public AvatarInventory _inventory;

        // Other
        public Int32 CraftingExperience;
        public DateTime Creation;

        /// <summary>
        /// Generates Avatar
        /// </summary>
        /// <remarks>If binding to player, don't forget to call player.Update()</remarks>
        /// <remarks>To push this into the database, call Put() on the generated object</remarks>
        /// <param name="player">player this avatar belongs to</param>
        /// <param name="bindToPlayer">if true, sets the avatar ids</param>
        /// <returns></returns>
        internal static Avatar Generate(Data.Player player, Boolean bindToPlayer)
        {
            Avatar result = new Avatar();
            result.Id = ObjectId.GenerateNewId(); // TODO: write code to retrieve increment key 
            result.UserId = player.Id;
            result.GuildId = ObjectId.Empty;
            result.TeamId = ObjectId.Empty;
            result.RaceId = 0;
            result.ClassId = 0;
            result.FactionId = 0;

            result.MapId = -1; 
            result.MapX = -1; 
            result.MapY = -1;
            result.TileId = 0;

            result.MoveSpeed = 3; 
            result.MoveFrequency = 3;
            result.StopFrequency = 0;
            result.MapDir = 2; 
            result.AnimationFrame = 0;
            result.RespawnId = 0;

            result.Flags = InteractableFlags.Visible;
            result.AssetName = String.Empty;
            result.Name = String.Empty;

            result.Body = AvatarBody.Generate();

            result.CraftingExperience = 0;
            result.Creation = DateTime.Now;

            if (bindToPlayer)
            {
                lock(player.AvatarIds)
                {
                    ObjectId[] ids = new ObjectId[player.AvatarIds.Length + 1];
                    player.AvatarIds.CopyTo(ids, 0);
                    ids[ids.Length-1] = result.Id;

                    player.AvatarIds = ids;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets avatar
        /// </summary>
        /// <param name="avatarid">Id of avatar to get</param>
        /// <returns>Running get task</returns>
        internal static Task<Avatar> Get(ObjectId avatarid)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(avatarid); });
        }

        /// <summary>
        /// Gets a avatar from the db
        /// </summary>
        /// <param name="id">id of avatar to get</param>
        /// <returns></returns>
        internal static Avatar GetBlocking(ObjectId id)
        {
            return GetCollection().FindOne(Query.EQ("_id", id)) as Avatar;
        }

        /// <summary>
        /// Gets all avatars for a certain user
        /// </summary>
        /// <param name="player">Player to get avatars for</param>
        /// <returns>Running get task</returns>
        internal static Task<BlockingCollection<Avatar>> GetAllForUser(Data.Player player)
        {
            return Task.Factory.StartNew<BlockingCollection<Avatar>>(() => 
            {
                BlockingCollection<Avatar> avatars = new BlockingCollection<Avatar>();
                ObjectId[] ids = player.AvatarIds;

                Parallel.ForEach(ids, id =>
                    {
                        avatars.Add(GetBlocking(id));
                    });

                avatars.CompleteAdding();

                return avatars;
            });
        }

        /// <summary>
        /// Puts a avatar to the db
        /// </summary>
        internal void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts avatar class into database
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Avatar>(this, safemode); // DISCUSS: Data.Interactable?
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal SafeModeResult Delete(SafeMode safemode)
        {
            // TODO set atomic flag if need atomic
            return GetCollection().Remove(Query.EQ("_id", this.Id), RemoveFlags.Single, safemode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Avatar> GetCollection()
        {
            return DataManager.Database.GetCollection<Avatar>("Avatars"); // Can not use interactable??? or can we? or whaaaat?
        }
    }
}
