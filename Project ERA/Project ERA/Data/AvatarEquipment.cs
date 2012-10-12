using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Data.Enum;
using ERAUtils;

namespace ProjectERA.Data
{
    /// <summary>
    /// List of equipment for avatars. 
    /// 
    /// Note: Normally, a generic list of equipment would suffice, but to reduce list searches 
    /// when drawing data, or looking up stat or graphical values, I choose this way of defining
    /// all equipment. We are now more limited of keeping these equipment slots, steering us
    /// to be more creative in the future.
    /// </summary>
    internal class AvatarEquipment : IResetable
    {
        #region Private fields

        private Equipment _left, 
            _right, _over, _top, _bottom, 
            _head, _hands, _feet, _neck, 
            _armLeft, _armRight, _extra, 
            _keyItem;

        private AvatarInventory _inventory;

        #endregion

        #region Properties

        /// <summary>
        /// Left hand
        /// </summary>
        internal Equipment Left
        {
            get { return _left;}
            set { _left = value; }
        }

        /// <summary>
        /// Right hand
        /// </summary>
        internal Equipment Right
        {
            get { return _right; }
            set { _right = value; }
        }

        /// <summary>
        /// Overlay
        /// </summary>
        internal Equipment Over
        {
            get { return _over; }
            set { _over = value; }
        }

        /// <summary>
        /// Top body
        /// </summary>
        internal Equipment Top
        {
            get { return _top; }
            set { _top = value; }
        }

        /// <summary>
        /// Bottom body
        /// </summary>
        internal Equipment Bottom
        {
            get { return _bottom; }
            set { _bottom = value; }
        }

        /// <summary>
        /// Head
        /// </summary>
        internal Equipment Head
        {
            get { return _head;}
            set { _head = value; }
        }

        /// <summary>
        /// Hands
        /// </summary>
        internal Equipment Hands
        {
            get { return _hands; }
            set { _hands = value; }
        }

        /// <summary>
        /// Feet
        /// </summary>
        internal Equipment Feet
        {
            get { return _feet; }
            set { _feet = value; }
        }
        
        /// <summary>
        /// Neck
        /// </summary>
        internal Equipment Neck
        {
            get { return _neck; }
            set { _neck = value; }
        }

        /// <summary>
        /// Left Arm
        /// </summary>
        internal Equipment ArmLeft
        {
            get { return _armLeft; }
            set { _armLeft = value; }
        }

        /// <summary>
        /// Right Arm
        /// </summary>
        internal Equipment ArmRight
        {
            get { return _armRight; }
            set { _armRight = value; }
        }

        /// <summary>
        /// Extra 
        /// </summary>
        internal Equipment Extra
        {
            get { return _extra; }
            set { _extra = value; }
        }

        /// <summary>
        /// Key item
        /// </summary>
        internal Equipment KeyItem
        {
            get { return _keyItem; }
            set { _keyItem = value; }
        }

        #endregion

        /// <summary>
        /// Empty constructor
        /// </summary>
        public AvatarEquipment()
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        internal AvatarEquipment(AvatarInventory inventory)
        {
            Initialize(inventory);
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="inventory"></param>
        internal void Initialize(AvatarInventory inventory)
        {
            this.Left = Equipment.GetById(EquipmentType.Shield, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.Right = Equipment.GetById(EquipmentType.Weapon, Services.Data.ContentDatabase.DefaultWeaponId);
            this.Top = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorTopId);
            this.Over = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.Bottom = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorBottomId);
            this.Head = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.Hands = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.Feet = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.Neck = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.ArmLeft = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.ArmRight = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.Extra = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);
            this.KeyItem = Equipment.GetById(EquipmentType.Armor, Services.Data.ContentDatabase.DefaultArmorEmptyId);

            _inventory = inventory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="equipment"></param>
        /// <returns></returns>
        internal Boolean Equip(Enum.EquipmentPart part, Item equipment)
        {
            Equipment edit = null;

            switch (part)
            {
                case EquipmentPart.ArmLeft:
                    edit = this.ArmLeft;
                    break;
                case EquipmentPart.ArmRight:
                    edit = this.ArmRight;
                    break;
                case EquipmentPart.Bottom:
                    edit = this.Bottom;
                    break;
                case EquipmentPart.Extra:
                    edit = this.Extra;
                    break;
                case EquipmentPart.Feet:
                    edit = this.Feet;
                    break;
                case EquipmentPart.Hands:
                    edit = this.Hands;
                    break;
                case EquipmentPart.Head:
                    edit = this.Head;
                    break;
                case EquipmentPart.KeyItem:
                    edit = this.KeyItem;
                    break;
                case EquipmentPart.Left:
                    edit = this.Left;
                    break;
                case EquipmentPart.Neck:
                    edit = this.Neck;
                    break;
                case EquipmentPart.Over:
                    edit = this.Over;
                    break;
                case EquipmentPart.Right:
                    edit = this.Right;
                    break;
                case EquipmentPart.Top:
                    edit = this.Top;
                    break;
            }

            if (edit == null || !edit.ItemFlags.HasFlag(ItemFlags.Locked))
            {
                if (edit != null && edit.Equals(Item.EmptyItem) == false)
                {
                    if (!_inventory.Store(equipment))
                    // error
                    {
                        
                    }
                }

                Equipment actualEquipment = (Equipment)(equipment.Equals(Item.EmptyItem) ? equipment : _inventory.Withdraw(equipment));

                if (actualEquipment != null)
                {
                    edit.Set(actualEquipment);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        internal Boolean UnEquip(EquipmentPart part)
        {
            return Equip(part, Item.EmptyItem);
        }

        /// <summary>
        /// Reset Equipment
        /// </summary>
        internal void Clear()
        {
            System.Diagnostics.Debug.WriteLine("Clearing avater equipment");

            this.Left.Clear();
            this.Right.Clear();
            this.Top.Clear();
            this.Over.Clear();
            this.Bottom.Clear();
            this.Head.Clear();
            this.Hands.Clear();
            this.Feet.Clear();
            this.Neck.Clear();
            this.ArmLeft.Clear();
            this.ArmRight.Clear();
            this.Extra.Clear();
            this.KeyItem.Clear();
        }
    }
}
