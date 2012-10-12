using System;
using System.Threading.Tasks;
using ERAUtils.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ERAServer.Services;

namespace ERAServer.Data.Blueprint
{
    [Serializable]
    internal class Equipment : Item
    {
        internal static new readonly Equipment Empty = new Equipment();

        /// <summary>
        /// Type of Equipment
        /// </summary>
        [BsonRequired]
        public EquipmentType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Place of Equipment
        /// </summary>
        [BsonRequired]
        public EquipmentPart Part
        {
            get;
            private set;
        }

        /// <summary>
        /// Equipment AssetName
        /// </summary>
        [BsonRequired]
        public String EquipmentAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// Elements Applied
        /// </summary>
        [BsonRequired]
        public ElementType ElementType
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public BattlerValues EquipmentModifier
        {
            get;
            private set;
        }

        [BsonIgnoreIfNull]
        public Nullable<Int32> HitSkill
        {
            get;
            private set;
        }

        /// <summary>
        /// Generates a Database Item
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="iconAssetName">Icon Asset Name</param>
        /// <param name="price">Price</param>
        /// <param name="flags">Flags</param>
        /// <param name="elementType">Equipment ElementType</param>
        /// <param name="equipmentAssetName">Equipment Asset Name</param>
        /// <param name="part">Body Part</param>
        /// <param name="type">Equipment Type</param>
        /// <param name="equipmentModifier">Battler stats modifier</param>
        /// <param name="hitSkill"></param>
        /// <returns>Generated item</returns>
        /// <remarks>Generates a ItemType.Weapon or Shield. To Generate different types, call their generate methods</remarks>
        internal static Equipment Generate(String name, Description description, String iconAssetName, Double price, ItemFlags flags,
            EquipmentType type, EquipmentPart part, String equipmentAssetName, ElementType elementType, BattlerValues equipmentModifier,
            Skill hitSkill)
        {
            return Generate(name, description, iconAssetName, price, flags,
                type, part, equipmentAssetName, elementType, equipmentModifier,
                hitSkill.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="iconAssetName"></param>
        /// <param name="price"></param>
        /// <param name="flags"></param>
        /// <param name="type"></param>
        /// <param name="part"></param>
        /// <param name="equipmentAssetName"></param>
        /// <param name="elementType"></param>
        /// <param name="equipmentModifier"></param>
        /// <param name="hitSkill"></param>
        /// <returns></returns>
        internal static Equipment Generate(String name, String iconAssetName, Double price, ItemFlags flags,
            EquipmentType type, EquipmentPart part, String equipmentAssetName, ElementType elementType, BattlerValues equipmentModifier,
            Skill hitSkill)
        {
            return Generate(name, Description.Empty, iconAssetName, price, flags,
                type, part, equipmentAssetName, elementType, equipmentModifier,
                hitSkill.Id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="iconAssetName"></param>
        /// <param name="price"></param>
        /// <param name="flags"></param>
        /// <param name="type"></param>
        /// <param name="part"></param>
        /// <param name="equipmentAssetName"></param>
        /// <param name="elementType"></param>
        /// <param name="equipmentModifier"></param>
        /// <param name="hitSkill"></param>
        /// <returns></returns>
        internal static Equipment Generate(String name, String iconAssetName, Double price, ItemFlags flags,
            EquipmentType type, EquipmentPart part, String equipmentAssetName, ElementType elementType, BattlerValues equipmentModifier,
            Int32? hitSkill = null)
        {
            return Generate(name, Description.Empty, iconAssetName, price, flags, type, part, equipmentAssetName, elementType, equipmentModifier, hitSkill);
        }

        /// <summary>
        /// Generates a Database Item
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="iconAssetName">Icon Asset Name</param>
        /// <param name="price">Price</param>
        /// <param name="flags">Flags</param>
        /// <param name="elementType">Equipment ElementType</param>
        /// <param name="equipmentAssetName">Equipment Asset Name</param>
        /// <param name="part">Body Part</param>
        /// <param name="type">Equipment Type</param>
        /// <param name="equipmentModifier">Battler stats modifier</param>
        /// <param name="hitSkill"></param>
        /// <returns>Generated item</returns>
        /// <remarks>Generates a ItemType.Weapon or Shield. To Generate different types, call their generate methods</remarks>
        internal static Equipment Generate(String name, Description description, String iconAssetName, Double price, ItemFlags flags, 
            EquipmentType type, EquipmentPart part, String equipmentAssetName, ElementType elementType, BattlerValues equipmentModifier,
            Int32? hitSkill = null)
        {
            // Set Item Data
            Equipment result = new Equipment(); // Pool<Item>.Fetch();

            // Update type field
            result.ItemType = (ItemType)System.Enum.Parse(typeof(ItemType), (type & ~EquipmentType.Double).ToString());

            // Generate base
            Item.Generate(name, description, iconAssetName, price, flags, result);

            // Set Equipment Data
            result.Type = type;
            result.Part = part;
            result.EquipmentAssetName = equipmentAssetName;
            result.ElementType = elementType;
            result.EquipmentModifier = equipmentModifier;
            result.ItemFlags |= ItemFlags.NoConsume;

            if (result.ItemType == ItemType.Weapon)
            {
                result.HitSkill = hitSkill;
            }

            return result;
        }

        /// <summary>
        /// Gets an equipment from the db
        /// </summary>
        /// <param name="id">id of item to get</param>
        /// <returns></returns>
        public static new Task<Equipment> Get(Int32 id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets an equipment from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static new Equipment GetBlocking(Int32 id)
        {
            Equipment result;
            if (!DataManager.Cache.Equipments.TryGetValue(id, out result))
            {
                result = GetCollection().FindOneById(id) as Equipment;

                if (result != null && result.Id != 0)
                    DataManager.Cache.Equipments.Enqueue(id, result);
            }

            return result;
        }

        /// <summary>
        /// Gets an item from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">id of item to get</param>
        /// <returns></returns>
        public static new Equipment GetBlocking(String name)
        {
            return GetCollection().FindOne(Query.Matches("Name", name)) as Equipment;
        }

        /// <summary>
        /// Gets the equipment collection
        /// </summary>
        /// <returns></returns>
        public static new MongoCollection<Equipment> GetCollection()
        {
            return DataManager.Database.GetCollection<Equipment>("Blueprint.Equipments");
        }

        /// <summary>
        /// Clears collection
        /// </summary>
        public static new void ClearCollection()
        {
            ClearCollection(true);
        }

        /// <summary>
        /// Drops collection and recreates content
        /// </summary>
        /// <param name="autoPopulate"></param>
        public static new void ClearCollection(Boolean autoPopulate)
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
        public static new void PopulateCollection()
        {
            Equipment.Generate("Stick", Description.Generate(String.Join(" ", "Simple stick that can be used to", "slash", "with")), String.Empty, 0.00, ItemFlags.NoActions | ItemFlags.Special, EquipmentType.Weapon, EquipmentPart.Left, "stick_sword", ElementType.None, new Data.Blueprint.BattlerValues(), 1).Put();
            Equipment.Generate("Sloppy Shirt", Description.Generate("No description"), String.Empty, 0.00, ItemFlags.NoActions | ItemFlags.Special, EquipmentType.Armor, EquipmentPart.Top, "sloppy_top", ElementType.None, new Data.Blueprint.BattlerValues()).Put();
            Equipment.Generate("Sloppy Trousers", Description.Generate("No description"), String.Empty, 0.00, ItemFlags.NoActions | ItemFlags.Special, EquipmentType.Armor, EquipmentPart.Bottom, "sloppy_bottom", ElementType.None, new Data.Blueprint.BattlerValues()).Put();
        }

        /// <summary>
        /// Puts a equipment to the db
        /// </summary>
        public override void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts an equipment to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        public override SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Equipment>(this, safemode);
        }     
    }
}
