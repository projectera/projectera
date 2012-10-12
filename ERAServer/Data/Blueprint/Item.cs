using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils.Enum;
using MongoDB.Bson.Serialization.Attributes;
using ERAUtils;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    [Serializable]
    [BsonDiscriminator(RootClass=true)]
    [BsonKnownTypes(typeof(Equipment), typeof(Consumable))]
    internal class Item
    {
        internal static readonly Item Empty = new Item();

        #region Private fields

        private String _name, _iconAssetName;
        private Double _price;
        private ItemFlags _flags;

        #endregion

        #region Properties

        /// <summary>
        /// Item Id
        /// </summary>
        [BsonId]
        public Int32 Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Item Type
        /// </summary
        //[BsonIgnore]
        public ItemType ItemType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Name of the item
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Description Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Icon asset name
        /// </summary>
        [BsonRequired]
        public String IconAssetName
        {
            get { return _iconAssetName; }
            set { _iconAssetName = value; }
        }

        /// <summary>
        /// Estimated original Value
        /// </summary>
        [BsonRequired]
        public Double PriceOriginal
        {
            get { return _price; }
            set { _price = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public ItemFlags ItemFlags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        #endregion

        /// <summary>
        /// Generates a Database Item
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description"></param>
        /// <param name="iconAssetName">Icon Asset Name</param>
        /// <param name="price">Price</param>
        /// <param name="flags">Flags</param>
        /// <returns>Generated item</returns>
        /// <remarks>Generates a ItemType.Item. To Generate different types, call their generate methods</remarks>
        internal static Item Generate(String name, Description description, String iconAssetName, Double price, ItemFlags flags)
        {
            Item result = new Item(); // Pool<Item>.Fetch();

            result.ItemType = ItemType.Item;

            result.Id = DataManager.IncrementalId(result.ItemType == ItemType.Item ? "Items" : "Equipment");
            result.Name = name;
            result.Description = description;
            result.IconAssetName = iconAssetName;
            result.PriceOriginal = price;
            result.ItemFlags = flags;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="iconAssetName"></param>
        /// <param name="price"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        internal static Item Generate(String name, String iconAssetName, Double price, ItemFlags flags)
        {
            return Generate(name, Description.Empty, iconAssetName, price, flags);
        }

        /// <summary>
        /// Generates a Database Item in the result var
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="description"></param>
        /// <param name="iconAssetName">Icon Asset Name</param>
        /// <param name="price">Price</param>
        /// <param name="flags">Flags</param>
        /// <param name="result">Destination</param>
        /// <returns>Generated item</returns>
        protected static Item Generate(String name, Description description, String iconAssetName, Double price, ItemFlags flags, Item result)
        {
            if (result.ItemType == ItemType.None)
                result.ItemType = ItemType.Item;

            result.Id = DataManager.IncrementalId(result.ItemType == ItemType.Item ? "Items" : "Equipment");  //TODO: DataManager.IncrementalId(result.ItemType.ToString());
            result.Name = name;
            result.Description = description;
            result.IconAssetName = iconAssetName;
            result.PriceOriginal = price;
            result.ItemFlags = flags;
            
            return result;
        }

        /// <summary>
        /// Generates a Database Item in the result var
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="iconAssetName">Icon Asset Name</param>
        /// <param name="price">Price</param>
        /// <param name="flags">Flags</param>
        /// <param name="result">Destination</param>
        /// <returns>Generated item</returns>
        protected static Item Generate(String name, String iconAssetName, Double price, ItemFlags flags, Item result)
        {
            return Generate(name, Description.Empty, iconAssetName, price, flags, result);
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(String description)
        {
            SetDescription(Description.Generate(description));
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(params String[] description)
        {
            SetDescription(Description.Generate(String.Join(" ", description)));
        }

        /// <summary>
        /// Sets description
        /// </summary>
        /// <param name="description"></param>
        internal void SetDescription(Description description, Boolean autoUpdate = true)
        {
            this.Description = description;

            if (autoUpdate)
                GetCollection().Update(Query.EQ("_id", this.Id), Update.Set("Description", this.Description.ToBsonDocument()));
        }

        /// <summary>
        /// Returns true if Blueprint is the same as object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override Boolean Equals(object obj)
        {
            if (obj is Item)
            {
                Item item = (Item)obj;
                return item.Id == this.Id &&
                    item.ItemFlags == this.ItemFlags;
            }

            return false;
        }

        /// <summary>
        /// Returns true if InteractableItem was generated with this as blueprint
        /// </summary>
        /// <param name="obj">InteractableItem</param>
        /// <returns></returns>
        internal Boolean HasGenerated(object obj)
        {
            if (obj is InteractableItem)
            {
                InteractableItem item = (InteractableItem)obj;
                return item.BlueprintId == this.Id;
            }

            return false;
        }

        /// <summary>
        /// Returns the Hashcode of this Item BluePrint
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode() ^
                this.ItemType.GetHashCode() ^
                this.ItemFlags.GetHashCode();
        }

        /// <summary>
        /// Gets an item from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static Task<Item> Get(Int32 id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an item from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static Task<Item> Get(String name)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static Item GetBlocking(String name)
        {
            return GetCollection().FindOne(Query.Matches("Name", name)) as Item;
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static Item GetBlocking(Int32 id)
        {
            Item result;
            if (!DataManager.Cache.Items.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as Item;

                if (result != null && result.Id != 0)
                    DataManager.Cache.Items.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Task<IEnumerable<Item>> Search(String name)
        {
            return Task<IEnumerable<Item>>.Factory.StartNew(() => { return SearchBlocking(name); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerable<Item> SearchBlocking(String name)
        {
            return GetCollection().Find(Query.Matches("Name", BsonRegularExpression.Create(new System.Text.RegularExpressions.Regex(name, System.Text.RegularExpressions.RegexOptions.IgnoreCase))));
        }

        /// <summary>
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<Item> GetCollection()
        {
            return DataManager.Database.GetCollection<Item>("Blueprint.Items");
        }

        /// <summary>
        /// Clears collection
        /// </summary>
        public static void ClearCollection()
        {
            ClearCollection(true);
        }

        /// <summary>
        /// Drops collection and recreates content
        /// </summary>
        /// <param name="autoPopulate"></param>
        public static void ClearCollection(Boolean autoPopulate)
        {
            if (GetCollection().Exists())
                GetCollection().Drop();
            

            if (autoPopulate)
                PopulateCollection();
        }

        /// <summary>
        /// Populates collection with default data 
        /// TODO: load from file?
        /// </summary>
        public static void PopulateCollection()
        {
        }

        /// <summary>
        /// Puts a item to the db
        /// </summary>
        public virtual void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts an item to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        public virtual SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Item>(this, safemode);
        }
    }
}
