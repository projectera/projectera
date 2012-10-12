using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils.Enum;
using Microsoft.Xna.Framework.Content;
using ERAUtils;

namespace ProjectERA.Data
{
    [Serializable]
    public class Item : IResetable, ICloneable
    {
        public static readonly Item EmptyItem = new Item();

        #region Private fields

        private String _name;
        private Double _boughtPrice;
        private MongoObjectId _id;
        private ItemType _itemType;
        private ItemFlags _flags;
        private String _iconAssetName;
        private Double _price;

        #endregion

        #region Properties

        /// <summary>
        /// Item Id
        /// </summary>
        [ContentSerializerIgnore]
        public MongoObjectId ItemId
        {
            get { return _id; }
            set { _id = value; }
        }

        [ContentSerializer(ElementName="Id")]
        public Int32 DatabaseId 
        { 
            get; 
            set; 
        }

        [ContentSerializerIgnore]
        public MongoObjectId OriginalInteractableId 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Item Type
        /// </summary>
        [ContentSerializerIgnore]
        internal ItemType ItemType
        {
            get { return _itemType; }
            set { _itemType = value; }
        }
        
        /// <summary>
        /// Name of the item
        /// </summary>
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Price upon buy
        /// </summary>
        [ContentSerializerIgnore]
        internal Double PriceBought
        {
            get { return _boughtPrice; }
            set { _boughtPrice = value; }
        }

        public Double Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public String IconAssetName
        {
            get { return _iconAssetName; }
            set { _iconAssetName = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ItemFlags ItemFlags
        {
            get { return _flags; }
            set { _flags = value; }
        }            

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Item()
        {

        }

        internal Item(MongoObjectId id, Int32 dbId, ItemType itemType, string name, string iconAssetName, double price, ItemFlags flags)
        {
            this.ItemId = id;
            this.DatabaseId = dbId;
            this.ItemType = itemType;
            this.Name = name;
            this.IconAssetName = iconAssetName;
            this.Price = price;
            this.ItemFlags = flags;
        }

        /// <summary>
        /// Reset Item
        /// </summary>
        public virtual void Clear()
        {
            this.ItemId = MongoObjectId.Empty;
            this.DatabaseId = 0;
            this.OriginalInteractableId = MongoObjectId.Empty;
            this.Name = String.Empty;
            this.ItemType = ItemType.None;
            this.PriceBought = 0;
            this.Price = 0;
            this.IconAssetName = String.Empty;
            this.ItemFlags = ItemFlags.None;

            System.Diagnostics.Debug.WriteLine("Clearing item");
        }

        /// <summary>
        /// Clone these members to reference members
        /// </summary>
        /// <param name="clone">reference destination</param>
        public void Clone(Item clone)
        {
            clone.ItemId = this.ItemId;
            clone.ItemType = this.ItemType;
            clone.DatabaseId = this.DatabaseId;
            clone.OriginalInteractableId = this.OriginalInteractableId;
            clone.Name = String.Copy(this.Name);
            clone.PriceBought = this.PriceBought;
            clone.ItemFlags = this.ItemFlags;
            clone.Price = this.Price;
            clone.IconAssetName = this.IconAssetName;
        }
        
        /// <summary>
        /// Creates a copy of this object
        /// </summary>
        /// <returns>New Instance</returns>
        public Object Clone()
        {
            Item clone = new Item();
            Clone(clone);
            return clone;
        }

        /// <summary>
        /// Casts, Clones, Casts
        /// </summary>
        /// <param name="clone"></param>
        protected void CastClone(Equipment clone)
        {
            Item clonebase = clone;
            
            Clone(clonebase);

            clone = (Equipment)clonebase;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override Boolean Equals(object obj)
        {
            if (obj is Item)
            {
                Item item = (Item)obj;
                return item.DatabaseId == this.DatabaseId &&
                    item.ItemType == this.ItemType &&
                    item.ItemFlags == this.ItemFlags;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.DatabaseId.GetHashCode() ^ 
                this.ItemType.GetHashCode() ^ 
                this.ItemFlags.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equipment"></param>
        /// <param name="msg"></param>
        protected static void UnpackTo(Item equipment, Lidgren.Network.NetIncomingMessage msg)
        {
            equipment.ItemId = (MongoObjectId)msg.ReadBytes(12);
            equipment.DatabaseId = msg.ReadInt32();
            equipment.OriginalInteractableId = (MongoObjectId)msg.ReadBytes(12);
            equipment.PriceBought = msg.ReadInt32();
            equipment.ItemFlags = (ItemFlags)msg.ReadUInt16();
            equipment.ItemType = (ItemType)msg.ReadByte();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected static Item Unpack(Lidgren.Network.NetIncomingMessage msg)
        {
            Item result = Pool<Item>.Fetch();
            UnpackTo(result, msg);

            /*Item blueprint = ProjectERA.Services.Data.ContentDatabase.GetItem(item.DatabaseId);
            result.Price = blueprint.Price;
            result.IconAssetName = blueprint.IconAssetName;
            result.Name = blueprint.Name;*/
            return result;
        }
    }
}
