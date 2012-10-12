using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectERA.Data.Update;
using ProjectERA.Data.Enum;
using ERAUtils;

namespace ProjectERA.Data
{
    public class Avatar : Interactable
    {

        #region Private Fields

        // Id holding
        private UInt16 _userId, _guildId, _teamId, _instanceId;
        private Byte[] _raceId, _classId;
        private Byte _factionId;

        // Avatar looks
        private AvatarBody _body;

        // Location
        private Int32 _respawnId;

        // Battler
        private AvatarStats _stats;
        private AvatarEquipment _equipment;
        private AvatarSynths _synths;
        private AvatarInventory _inventory;

        // Other
        private Int32 _craftingExperience;
        private DateTime _creation;
        #endregion

        /// <summary>
        /// Constructs an empty avatar
        /// </summary>
        public Avatar() : base()
        {
            // TODO these should be components. Now pool usage is wrong, then pool usage is right
            this.Body = Pool<AvatarBody>.Fetch(); //ew AvatarBody();
            this.Stats = Pool<AvatarStats>.Fetch();
            this.Inventory = Pool<AvatarInventory>.Fetch();
            this.Equipment = Pool<AvatarEquipment>.Fetch();
            this.Equipment.Initialize(this.Inventory);
            this.Synths = Pool<AvatarSynths>.Fetch(); 
        }

        /// <summary>
        /// Constructs an Avatar
        /// </summary>
        /// <param name="avatar_id">Id of the avatar</param>
        /// <param name="user_id">Id of the user</param>
        internal Avatar(Byte[] avatarId, UInt16 userId, UInt16 guildId, UInt16 teamId, UInt16 instanceId, UInt16 respawnId, Byte[] raceId, Byte[] classId, Byte factionId,
            AvatarBody body, String name, InteractableFlags flags, Byte[] mapId, Int32 mapX, Int32 mapY, Byte mapDir, AvatarStats stats, AvatarEquipment equipment, AvatarSynths synths,
            AvatarInventory inventory, Int32 craftingExperience, DateTime creation)
            : base(avatarId, 0, name, String.Empty, flags | InteractableFlags.Visible, Enum.InteractableType.Avatar, mapId, mapX, mapY, mapDir, 0, 3, 3, 0)

        {
            _userId = userId;
            _guildId = guildId;
            _teamId = teamId;
            _raceId = raceId;
            _classId = classId;
            _factionId = factionId;
            _instanceId = instanceId;
            _respawnId = respawnId;
            _body = body;
            _stats = stats;
            _equipment = equipment;
            _synths = synths;
            _inventory = inventory;
            _craftingExperience = craftingExperience;
            _creation = creation;
        }

        #region Properties

        /// <summary>
        /// User Id
        /// </summary>
        internal UInt16 UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        /// <summary>
        /// Guild Id
        /// </summary>
        internal UInt16 GuildId
        {
            get { return _guildId; }
            set { _guildId = value; }
        }

        /// <summary>
        /// Team Id
        /// </summary>
        internal UInt16 TeamId
        {
            get { return _teamId; }
            set { _teamId = value; }
        }

        /// <summary>
        /// Race Id
        /// </summary>
        internal Byte[] RaceId
        {
            get { return _raceId; }
            set { _raceId = value; }
        }

        /// <summary>
        /// Class Id
        /// </summary>
        internal Byte[] ClassId
        {
            get { return _classId; }
            set { _classId = value; }
        }

        /// <summary>
        /// Faction Id
        /// </summary>
        internal Byte FactionId
        {
            get { return _factionId; }
            set { _factionId = value; }
        }

        /// <summary>
        /// Instance Id;
        /// </summary>
        internal UInt16 InstanceId
        {
            get { return _instanceId; }
            set { _instanceId = value; }
        }

        /// <summary>
        /// Avatar Body
        /// </summary>
        internal AvatarBody Body
        {
            get { return _body; }
            set { _body = value; }
        }

        /// <summary>
        /// Avatar Respawn Id
        /// </summary>
        internal Int32 RespawnId
        {
            get { return _respawnId; }
            set { _respawnId = value; }
        }
        
        /// <summary>
        /// Avatar Battler Stats
        /// </summary>
        internal AvatarStats Stats
        {
            get { return _stats; }
            set { _stats = value; }
        }

        /// <summary>
        /// Avatar Battler Equipment
        /// </summary>
        internal AvatarEquipment Equipment
        {
            get { return _equipment; }
            set { _equipment = value; }
        }

        /// <summary>
        /// Avatar Battler Synths
        /// </summary>
        internal AvatarSynths Synths
        {
            get { return _synths; }
            set { _synths = value; }
        }

        /// <summary>
        /// Avatar Inventory
        /// </summary>
        internal AvatarInventory Inventory
        {
            get { return _inventory; }
            set { _inventory = value; }
        }

        /// <summary>
        /// Crafting Experience
        /// </summary>
        internal Int32 CraftingExperience
        {
            get { return _craftingExperience; }
            set { _craftingExperience = value; }
        }

        /// <summary>
        /// Avatar Creation Time
        /// </summary>
        internal DateTime Creation
        {
            get { return _creation; }
            set { _creation = value; }
        }

        /// <summary>
        /// Avatar Id
        /// </summary>
        internal Byte[] AvatarId
        {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        /// <summary>
        /// Returns equality for this instance and argument
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if equal</returns>
        public override Boolean Equals(Object obj)
        {
            return obj is Avatar && Equals((Avatar)obj);
        }

        /// <summary>
        /// Returns equality for this instance and argument
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True if equal</returns>
        internal Boolean Equals(Avatar obj)
        {
            return this.AvatarId == obj.AvatarId;
        }

        /// <summary>
        /// Returns the hashcode for this instance
        /// </summary>
        /// <returns>Hash code for this instance</returns>
        public override Int32 GetHashCode()
        {
            return this.AvatarId.GetHashCode() ^ this.GetType().GetHashCode();
        }

        /// <summary>
        /// Reset Avatar
        /// </summary>
        public override void Clear()
        {
            System.Diagnostics.Debug.WriteLine("Clearing avatar");

            this.AvatarId = new Byte[12];
            this.UserId = 0;
            this.GuildId = 0;
            this.TeamId = 0;
            this.RaceId = new Byte[12];
            this.ClassId = new Byte[12];
            this.FactionId = 0;
            this.RespawnId = 0;

            this.Body.Clear();
            this.Stats.Clear();
            this.Equipment.Clear();
            this.Synths.Clear();
            this.Inventory.Clear();

            this.CraftingExperience = 0;
            this.Creation = DateTime.MinValue;

            base.Clear();

        }

        
    }
}