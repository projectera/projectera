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
    public class Equipment : Item
    {
        internal static Equipment Empty = new Equipment(Services.Data.ContentDatabase.DefaultArmorEmptyId, 0, EquipmentType.None, EquipmentPart.None, 
            String.Empty, String.Empty, String.Empty, 0.00, 0.00, ElementType.None, ItemFlags.None,
            new BattlerValues());

        #region Private fields

        private String _equipmentAssetName;
        private EquipmentPart _part;
        private EquipmentType _type;
        private Double _integrity;
        private ElementType _elements;
        private BattlerValues _battlerEquipment;
        //NOTE private Int32 _level;

        #endregion

        #region Properties

        /// <summary>
        /// Type of Equipment
        /// </summary>
        public EquipmentType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        /// <summary>
        /// Place of Equipment
        /// </summary>
        public EquipmentPart Part
        {
            get { return _part; }
            set { _part = value; }
        }

        /// <summary>
        /// Equipment AssetName
        /// </summary>
        public String EquipmentAssetName
        {
            get { return _equipmentAssetName; }
            set { _equipmentAssetName = value; }
        }

        /// <summary>
        /// Structural Integrity
        /// </summary>
        public Double Integrity
        {
            get { return _integrity; }
            set { _integrity = value; }
        }

        /// <summary>
        /// Elements Applied
        /// </summary>
        public ElementType ElementType
        {
            get { return _elements; }
            set { _elements = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public BattlerValues BattlerEquipment
        {
            get { return _battlerEquipment; }
            set { _battlerEquipment = value; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Equipment()
        {
            this.BattlerEquipment = new BattlerValues();
        }

        /// <summary>
        /// Constructor with predefined data
        /// </summary>
        /// <param name="id">Id of the equipment</param>
        /// <param name="equipmentAssetName">Name of the equipment asset</param>
        /// <param name="iconAssetName">Name of the icon asset</param>
        /// <param name="equipmentAssetId"></param>
        /// <param name="name">Name of the equipment</param>
        /// <param name="price">Original price of the equipment</param>
        /// <param name="type">Type of equipment</param>
        /// <param name="part">Part of body</param>
        /// <param name="integrity">Structural integrity</param>
        /// <param name="locked">Locked when equipped</param>
        /// <param name="defaultLocked">Default Locked when equipped</param>
        /// <param name="elements">Elemental types</param>
        /// <param name="flags">Item flags</param>
        /// <param name="battlerEquipment">Battler Equipment Values</param>
        internal Equipment(MongoObjectId id, Int32 dbId, EquipmentType type, EquipmentPart part,  String name, String iconAssetName, String equipmentAssetName,
            Double price, Double integrity, ElementType elements, ItemFlags flags, BattlerValues battlerEquipment)
            : base(id, dbId, (ItemType)System.Enum.Parse(typeof(ItemType), (type & ~EquipmentType.Double).ToString()), name, iconAssetName, price, flags)
        {
            this.Type = type;
            this.Part = part;
            this.EquipmentAssetName = equipmentAssetName;
            this.Integrity = integrity;
            this.ElementType = elements;
            this.BattlerEquipment = battlerEquipment;
        }

        /// <summary>
        /// Reset Equipment
        /// </summary>
        public override void Clear()
        {
            System.Diagnostics.Debug.WriteLine("Clearing equipment");

            base.Clear();

            this.Type = EquipmentType.None;
            this.Part = EquipmentPart.None;
            this.EquipmentAssetName = String.Empty;
            this.Integrity = 0;
            this.ElementType = ElementType.None;

            this.BattlerEquipment.Clear();
        }

        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public new Object Clone()
        {
            Equipment clone = Pool<Equipment>.Fetch();
            clone.Type = this.Type;
            clone.Part = this.Part;
            clone.EquipmentAssetName = String.Copy(this.EquipmentAssetName);
            clone.Integrity = this.Integrity;
            clone.ElementType = this.ElementType;
            clone.BattlerEquipment = this.BattlerEquipment;

            base.CastClone(clone);

            return clone;
        }

        /// <summary>
        /// Clone these members to reference members
        /// </summary>
        /// <param name="clone">Reference destination</param>
        public void Clone(Equipment clone)
        {
            base.CastClone(clone);

            clone.Type = this.Type;
            clone.Part = this.Part;
            clone.Integrity = this.Integrity;

            clone.EquipmentAssetName = String.Copy(this.EquipmentAssetName);
            clone.ElementType = this.ElementType;
            clone.BattlerEquipment = this.BattlerEquipment;
        }

        /// <summary>
        /// Clones reference members to these
        /// </summary>
        /// <param name="source">reference source</param>
        public void Set(Equipment source)
        {
            source.Clone(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="equipmentId"></param>s
        /// <returns></returns>
        internal static Equipment GetById(EquipmentType type, Int32 equipmentId)
        {
            return (Equipment)Services.Data.ContentDatabase.GetEquipment(equipmentId, type).Clone();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal static new Equipment Unpack(Lidgren.Network.NetIncomingMessage msg)
        {
            Equipment equipment = Pool<Equipment>.Fetch();
            Item.UnpackTo(equipment, msg);

            equipment.Integrity = msg.ReadInt32();

            Equipment blueprint = ProjectERA.Services.Data.ContentDatabase.GetEquipment(equipment.DatabaseId, equipment.ItemType);
            equipment.Price = blueprint.Price;
            equipment.IconAssetName = blueprint.IconAssetName;
            equipment.Name = blueprint.Name;
            equipment.Part = blueprint.Part;
            equipment.Type = blueprint.Type;
            equipment.EquipmentAssetName = equipment.EquipmentAssetName;
            equipment.BattlerEquipment = equipment.BattlerEquipment;

            return equipment;
        }
    }
}
