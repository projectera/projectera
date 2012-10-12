using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using ProjectERA.Services.Data.Serialization;
using System.Threading;
using System.Collections.Concurrent;
using ERAUtils.Logger;
using ERAUtils;
using System.Security;

namespace ProjectERA.Services.Data
{
    /// <summary>
    /// The ContentDatabase is shared by Server and Client and contains all the information regarding
    /// Game Data. All GameData EquipmentId's are binded to information in this class. The ContentDatabase is
    /// populated by the ContentPopulator.
    /// </summary>
    internal static partial class ContentDatabase
    {
        #region Constants
        internal const String DefaultBody = "body-male";
        internal const String DefaultHead = "longbrown";
        internal const String DefaultEyes = "";

        internal const Int32 DefaultWeaponAssetId = 1;
        internal static MongoObjectId DefaultArmorEmptyId = MongoObjectId.Empty;
        #endregion

#if !NOMULTITHREAD
        private static Int32 _asyncOperations;
        private static SpinWait _spinWait;
        private static ReaderWriterLockSlim _lockBodies, _lockEyes, _lockHeads, _lockColors;
        private static ReaderWriterLockSlim _lockWeapons, _lockArmors, _lockAccessories, _lockKeyItems;
        private static ReaderWriterLockSlim _lockStates, _lockBuffs, _lockBattlerClasses, _lockBattlerRaces; 
        
        /// <summary>
        /// Operation is occuring flag
        /// </summary>
        public static Boolean IsBusy
        {
            get { return _asyncOperations > 0; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Boolean IsReading(ContentDatabaseType type)
        {
            switch (type)
            {
                case ContentDatabaseType.Body:
                    return _lockBodies.IsReadLockHeld;
                case ContentDatabaseType.Eyes:
                    return _lockEyes.IsReadLockHeld;
                case ContentDatabaseType.Head:
                    return _lockHeads.IsReadLockHeld;
                case ContentDatabaseType.Color:
                    return _lockColors.IsReadLockHeld;
                case ContentDatabaseType.BattlerState:
                    return _lockStates.IsReadLockHeld;
                case ContentDatabaseType.BattlerBuff:
                    return _lockBuffs.IsReadLockHeld;
                case ContentDatabaseType.Weapon:
                    return _lockWeapons.IsReadLockHeld;
                case ContentDatabaseType.Armor:
                    return _lockArmors.IsReadLockHeld;
                case ContentDatabaseType.Accessory:
                    return _lockAccessories.IsReadLockHeld;
                case ContentDatabaseType.KeyItem:
                    return _lockKeyItems.IsReadLockHeld;
                case ContentDatabaseType.BattlerClass:
                    return _lockBattlerClasses.IsReadLockHeld;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Boolean IsWriting(ContentDatabaseType type)
        {
            switch (type)
            {
                case ContentDatabaseType.Body:
                    return _lockBodies.IsWriteLockHeld;
                case ContentDatabaseType.Eyes:
                    return _lockEyes.IsWriteLockHeld;
                case ContentDatabaseType.Head:
                    return _lockHeads.IsWriteLockHeld;
                case ContentDatabaseType.Color:
                    return _lockColors.IsWriteLockHeld;
                case ContentDatabaseType.BattlerState:
                    return _lockStates.IsWriteLockHeld;
                case ContentDatabaseType.BattlerBuff:
                    return _lockBuffs.IsWriteLockHeld;
                case ContentDatabaseType.Weapon:
                    return _lockWeapons.IsWriteLockHeld;
                case ContentDatabaseType.Armor:
                    return _lockArmors.IsWriteLockHeld;
                case ContentDatabaseType.Accessory:
                    return _lockAccessories.IsWriteLockHeld;
                case ContentDatabaseType.KeyItem:
                    return _lockKeyItems.IsWriteLockHeld;
                case ContentDatabaseType.BattlerClass:
                    return _lockBattlerClasses.IsWriteLockHeld;
            }

            return false;
        }

#endif

        /// <summary>
        /// 
        /// </summary>
        public static void Initialize()
        {
#if !NOMULTITHREAD
            _spinWait = new SpinWait();
            _lockBodies = new ReaderWriterLockSlim();
            _lockEyes = new ReaderWriterLockSlim();
            _lockHeads = new ReaderWriterLockSlim();
            _lockColors = new ReaderWriterLockSlim();
            _lockWeapons = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            _lockArmors = new ReaderWriterLockSlim();
            _lockAccessories = new ReaderWriterLockSlim();
            _lockKeyItems = new ReaderWriterLockSlim();
            _lockStates = new ReaderWriterLockSlim();
            _lockBuffs = new ReaderWriterLockSlim();
            _lockBattlerClasses = new ReaderWriterLockSlim();
            _lockBattlerRaces = new ReaderWriterLockSlim();
#endif
        }

        #region Character Body
        private static Dictionary<Byte, String> _bodies = new Dictionary<Byte, String>();
        private static Dictionary<String, Byte> _colors = new Dictionary<String, Byte>();
        private static Dictionary<Byte, Dictionary<Byte, String>> _heads = new Dictionary<Byte, Dictionary<Byte, String>>();
        private static Dictionary<Byte, Dictionary<Byte, String>> _eyes = new Dictionary<Byte, Dictionary<Byte, String>>();

        /// <summary>
        /// Get Body Asset Name
        /// </summary>
        /// <param name="skintonid">Skintone</param>
        /// <returns>Body Asset Name</returns>
        internal static String GetBodyAssetName(Byte skintoneId)
        {
#if !NOMULTITHREAD
            _lockBodies.EnterReadLock();
#endif

            String body;

            try
            {
                if (_bodies.TryGetValue(skintoneId, out body))
                    return body;

                return ContentDatabase.DefaultBody;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockBodies.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Get Color Byte
        /// </summary>
        /// <param name="colorName">Color</param>
        /// <returns>Color Byte</returns>
        internal static Byte GetColorByte(String colorName)
        {
#if !NOMULTITHREAD
            _lockColors.EnterReadLock();
#endif

            Byte color;

            try
            {
                if (_colors.TryGetValue(colorName, out color))
                    return color;

                return 0;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockColors.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Get Head Asset Name
        /// </summary>
        /// <param name="hairtypid">Hairtype</param>
        /// <param name="haircolor_id">HairColor</param>
        /// <returns>Head Asset Name</returns>
        internal static String GetHeadAssetName(Byte hairTypeId, Byte hairColorId)
        {
            Dictionary<Byte, String> headRange;
            String head;
#if !NOMULTITHREAD
            _lockHeads.EnterReadLock();
#endif
            try
            {
                if (_heads.TryGetValue(hairTypeId, out headRange))
                    if (headRange.TryGetValue(hairColorId, out head))
                        return head;

                return ContentDatabase.DefaultHead;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockHeads.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Get Eyes Asset Name
        /// </summary>
        /// <param name="eyetypeId">EyeType</param>
        /// <param name="eyecolorId">EyeColor</param>
        /// <returns>Eyes Asset Name</returns>
        internal static String GetEyesAssetName(Byte eyesTypeId, Byte eyesColorId)
        {
            Dictionary<Byte, String> eyeRange;
            String eye;
#if !NOMULTITHREAD
            _lockEyes.EnterReadLock();
#endif

            try
            {
                if (_eyes.TryGetValue(eyesTypeId, out eyeRange))
                    if (eyeRange.TryGetValue(eyesColorId, out eye))
                        return eye;

                return ContentDatabase.DefaultEyes;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockEyes.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Set Body Asset Name
        /// </summary>
        /// <param name="skintoneId">SkinTone</param>
        /// <param name="assetName">Body Asset Name</param>
        internal static void SetBodyAssetName(Byte skintoneId, String assetName)
        {
#if !NOMULTITHREAD
            _lockBodies.EnterWriteLock();
#endif
            _bodies[skintoneId] = assetName;
#if !NOMULTITHREAD
            _lockBodies.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Set Color Byte
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="colorByte">Color Byte</param>
        internal static void SetColorByte(String colorName, Byte colorByte)
        {
#if !NOMULTITHREAD
            _lockColors.EnterWriteLock();
#endif
            _colors[colorName] = colorByte;
#if !NOMULTITHREAD
            _lockColors.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Set Head Asset Name
        /// </summary>
        /// <param name="hairTypeId">HairType</param>
        /// <param name="hairColorId">HairColor</param>
        /// <param name="assetName">Head Asset Name</param>
        internal static void SetHeadAssetName(Byte hairTypeId, Byte hairColorId, String assetName)
        {
            Dictionary<Byte, String> headsRange;
#if !NOMULTITHREAD
            _lockHeads.EnterWriteLock();
#endif

            if (_heads.TryGetValue(hairTypeId, out headsRange) == false)
                _heads[hairTypeId] = new Dictionary<Byte, String>();

            _heads[hairTypeId][hairColorId] = assetName;
#if !NOMULTITHREAD
            _lockHeads.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Set Eyes Asset Name
        /// </summary>
        /// <param name="eyesTypeId">EyesType</param>
        /// <param name="eyesColorId">EyesColor</param>
        /// <param name="assetName">Eyes Asset Name</param>
        internal static void SetEyesAssetName(Byte eyesTypeId, Byte eyesColorId, String assetName)
        {
            Dictionary<Byte, String> eyesRange;
#if !NOMULTITHREAD
            _lockEyes.EnterWriteLock();
#endif
            if (_eyes.TryGetValue(eyesTypeId, out eyesRange) == false)
                _eyes[eyesTypeId] = new Dictionary<Byte, String>();

            _eyes[eyesTypeId][eyesColorId] = assetName;
#if !NOMULTITHREAD
            _lockEyes.ExitWriteLock();
#endif
        }
        #endregion

        #region Equipment

        private static Dictionary<Int32, ProjectERA.Data.Equipment> _weapons = new Dictionary<Int32, ProjectERA.Data.Equipment>();
        private static Dictionary<Int32, ProjectERA.Data.Equipment> _armors = new Dictionary<Int32, ProjectERA.Data.Equipment>();
        private static Dictionary<Int32, ProjectERA.Data.Equipment> _accessoiries = new Dictionary<Int32, ProjectERA.Data.Equipment>();
        private static Dictionary<Int32, ProjectERA.Data.Equipment> _keyItems = new Dictionary<Int32, ProjectERA.Data.Equipment>();

        /// <summary>
        /// Get equipment
        /// </summary>
        /// <param name="id">Equipment ID</param>
        /// <param name="type">Type</param>
        /// <returns>Equipment</returns>
        internal static ProjectERA.Data.Equipment GetEquipment(Int32 id, ERAUtils.Enum.EquipmentType type)
        {
            switch (type)
            {
                case ERAUtils.Enum.EquipmentType.None:
                    return GetEquipmentAccessoiry(0);
                case ERAUtils.Enum.EquipmentType.DoubleWeapon:
                case ERAUtils.Enum.EquipmentType.Weapon:
                    return GetEquipmentWeapon(id);
                case ERAUtils.Enum.EquipmentType.DoubleShield:
                case ERAUtils.Enum.EquipmentType.Shield:
                case ERAUtils.Enum.EquipmentType.Armor:
                    return GetEquipmentArmor(id);
                default:
                    return GetEquipmentAccessoiry(0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static ProjectERA.Data.Equipment GetEquipment(Int32 id, ERAUtils.Enum.ItemType type)
        {
            switch (type)
            {
                case ERAUtils.Enum.ItemType.None:
                    return GetEquipmentAccessoiry(0);
                case ERAUtils.Enum.ItemType.Shield:
                case ERAUtils.Enum.ItemType.Armor:
                    return GetEquipmentArmor(id);
                case ERAUtils.Enum.ItemType.Weapon:
                    return GetEquipmentWeapon(id);
                case ERAUtils.Enum.ItemType.Special:
                    return GetEquipmentAccessoiry(id);
                default:
                    return GetEquipmentAccessoiry(0);
            }
        }

        /// <summary>
        /// Get Weapon Equipment
        /// </summary>
        /// <param name="weaponAssetId">Weapon EquipmentId</param>
        /// <returns>Weapon Equipment</returns>
        internal static ProjectERA.Data.Equipment GetEquipmentWeapon(Int32 weaponAssetId)
        {
            ProjectERA.Data.Equipment weapon;

#if !NOMULTITHREAD
            _lockWeapons.EnterReadLock();
#endif
            try
            {
                if (_weapons.TryGetValue(weaponAssetId, out weapon))
                    return weapon;

                if (DefaultWeaponAssetId != weaponAssetId)
                {
                    return GetEquipmentWeapon(DefaultWeaponAssetId);
                }

                throw new InvalidOperationException("DefaultWeapon is missing");
            }
            finally
            {
#if !NOMULTITHREAD
                _lockWeapons.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Get Armor Equipment
        /// </summary>
        /// <param name="armorAssetId">Armor EquipmentId</param>
        /// <returns>Armor Equipment</returns>
        internal static ProjectERA.Data.Equipment GetEquipmentArmor(Int32 armorAssetId)
        {
            ProjectERA.Data.Equipment armor;
#if !NOMULTITHREAD
            _lockArmors.EnterReadLock();
#endif
            try
            {
                if (_armors.TryGetValue(armorAssetId, out armor))
                    return armor;

                if (armorAssetId > 0)
                    return ProjectERA.Data.Equipment.Empty;

                return null;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockArmors.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Get Equipment (Accesoiry)
        /// </summary>
        /// <param name="assetId">Equipment EquipmentId</param>
        /// <returns>Accesoiry</returns>
        internal static ProjectERA.Data.Equipment GetEquipmentAccessoiry(Int32 assetId)
        {
            ProjectERA.Data.Equipment accesoiry;
#if !NOMULTITHREAD
            _lockAccessories.EnterReadLock();
#endif
            try
            {
                if (_accessoiries.TryGetValue(assetId, out accesoiry))
                    return accesoiry;

                return ProjectERA.Data.Equipment.Empty;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockAccessories.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Get Equipment (KeyItem)
        /// </summary>
        /// <param name="assetId">Equipment EquipmentId</param>
        /// <returns>KeyItem</returns>
        internal static ProjectERA.Data.Equipment GetEquipmentKeyItem(Int32 assetId)
        {
            ProjectERA.Data.Equipment keyItem;
#if !NOMULTITHREAD
            _lockKeyItems.EnterReadLock();
#endif
            try
            {
                if (_keyItems.TryGetValue(assetId, out keyItem))
                    return keyItem;

                return ProjectERA.Data.Equipment.Empty;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockKeyItems.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// Set Weapon Equipment
        /// </summary>
        /// <param name="weapon">Weapon Equipment</param>
        internal static void SetEquipmentWeapon(ProjectERA.Data.Equipment weapon)
        {
#if !NOMULTITHREAD
            _lockWeapons.EnterWriteLock();
#endif
            _weapons[weapon.DatabaseId] = weapon;
#if !NOMULTITHREAD
            _lockWeapons.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Set Armor Equipment
        /// </summary>
        /// <param name="weapon">Armor Equipment</param>
        internal static void SetEquipmentArmor(ProjectERA.Data.Equipment armor)
        {
#if !NOMULTITHREAD
            _lockArmors.EnterWriteLock();
#endif
            _armors[armor.DatabaseId] = armor;
#if !NOMULTITHREAD
            _lockArmors.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Set Accesoiry Equipment
        /// </summary>
        /// <param name="accessoiry">Acessoiry</param>
        internal static void SetEquipmentAccessoiry(ProjectERA.Data.Equipment accessoiry)
        {
#if !NOMULTITHREAD
            _lockAccessories.EnterWriteLock();
#endif
            _accessoiries[accessoiry.DatabaseId] = accessoiry;
#if !NOMULTITHREAD
            _lockAccessories.ExitWriteLock();
#endif
        }

        /// <summary>
        /// Set KeyItem Equipment
        /// </summary>
        /// <param name="keyItem">KeyItem</param>
        internal static void SetEquipmentKeyItem(ProjectERA.Data.Equipment keyItem)
        {
#if !NOMULTITHREAD
            _lockKeyItems.EnterWriteLock();
#endif
            _keyItems[keyItem.DatabaseId] = keyItem;
#if !NOMULTITHREAD
            _lockKeyItems.ExitWriteLock();
#endif
        }

        #endregion

        #region BattlerClass/BattlerRace
        private static Dictionary<Int32, ProjectERA.Data.BattlerClass> _battlerClasses = new Dictionary<Int32, ProjectERA.Data.BattlerClass>();
        private static Dictionary<Int32, ProjectERA.Data.BattlerRace> _battlerRaces = new Dictionary<Int32, ProjectERA.Data.BattlerRace>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        internal static ProjectERA.Data.BattlerClass GetBattlerClass(Int32 classId)
        {
            ProjectERA.Data.BattlerClass battlerClass;
#if !NOMULTITHREAD
            _lockBattlerClasses.EnterReadLock();
#endif
            try
            {
                if (_battlerClasses.TryGetValue(classId, out battlerClass))
                    return battlerClass;

                return null;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockBattlerClasses.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classId"></param>
        /// <returns></returns>
        internal static ProjectERA.Data.BattlerRace GetBattlerRace(Int32 raceId)
        {
            ProjectERA.Data.BattlerRace battlerRace;
#if !NOMULTITHREAD
            _lockBattlerRaces.EnterReadLock();
#endif
            try
            {
                if (_battlerRaces.TryGetValue(raceId, out battlerRace))
                    return battlerRace;

                return null;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockBattlerRaces.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="battlerClass"></param>
        internal static void SetBattlerClass(ProjectERA.Data.BattlerClass battlerClass)
        {
           
#if !NOMULTITHREAD
            _lockBattlerClasses.EnterWriteLock();
#endif
            _battlerClasses[battlerClass.DatabaseId] = battlerClass;
#if !NOMULTITHREAD
            _lockBattlerClasses.ExitWriteLock();
#endif
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="battlerRace"></param>
        internal static void SetBattlerRace(ProjectERA.Data.BattlerRace battlerRace)
        {

#if !NOMULTITHREAD
            _lockBattlerRaces.EnterWriteLock();
#endif
            _battlerRaces[battlerRace.DatabaseId] = battlerRace;
#if !NOMULTITHREAD
            _lockBattlerRaces.ExitWriteLock();
#endif

        }

        #endregion

        #region BattlerModifiers
        private static Dictionary<Int32, ProjectERA.Data.BattlerModifier> _states = new Dictionary<Int32, ProjectERA.Data.BattlerModifier>();
        private static Dictionary<Int32, ProjectERA.Data.BattlerModifier> _buffs = new Dictionary<Int32, ProjectERA.Data.BattlerModifier>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isState"></param>
        /// <returns></returns>
        internal static ProjectERA.Data.BattlerModifier GetBattlerModifier(Int32 id, Boolean isState)
        {
            if (isState)
                return GetBattlerModifierState(id);

            return GetBattlerModifierBuff(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        internal static ProjectERA.Data.BattlerModifier GetBattlerModifierState(Int32 stateId)
        {
            ProjectERA.Data.BattlerModifier state;
#if !NOMULTITHREAD
            _lockStates.EnterReadLock();
#endif
            try
            {
                if (_states.TryGetValue(stateId, out state))
                    return state;

                return null;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockStates.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffId"></param>
        /// <returns></returns>
        internal static ProjectERA.Data.BattlerModifier GetBattlerModifierBuff(Int32 buffId)
        {
            ProjectERA.Data.BattlerModifier buff;
#if !NOMULTITHREAD
            _lockBuffs.EnterReadLock();
#endif
            try
            {
                if (_buffs.TryGetValue(buffId, out buff))
                    return buff;

                return null;
            }
            finally
            {
#if !NOMULTITHREAD
                _lockBuffs.ExitReadLock();
#endif
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifier"></param>
        internal static void SetBattlerModifier(ProjectERA.Data.BattlerModifier modifier)
        {
            if (modifier.IsState)
            {
#if !NOMULTITHREAD
                _lockStates.EnterWriteLock();
#endif
                _states[modifier.ModifierId] = modifier;
#if !NOMULTITHREAD
                _lockStates.ExitWriteLock();
#endif
            }
            else if (modifier.IsBuff)
            {
#if !NOMULTITHREAD
                _lockBuffs.EnterWriteLock();
#endif
                _buffs[modifier.ModifierId] = modifier;
#if !NOMULTITHREAD
                _lockBuffs.ExitWriteLock();
#endif
            }
            else
#if DEBUG && NOFAILSAFE
                throw new FormatException("Invalid flags provided for modifier.");
#else
                Logger.Error(new String[] { "Modifier (m:", modifier.ModifierId.ToString(), ") has invalid flags." });
#endif
        }
        #endregion

        #region Serializing
        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal static String GetFilePath(String p)
        {
            return @"Data/Database/" + p + ".xml";
        }

        /// <summary>
        /// Get Weapons serialized
        /// </summary>
        /// <returns></returns>
        internal static SerializableColors GetSerializableColors()
        {
            SerializableColors result = new SerializableColors();

            String[] dataA;
            Byte[] dataB;
#if !NOMULTITHREAD
            _lockColors.EnterReadLock();
#endif
            dataA = new String[_colors.Count];
            _colors.Keys.CopyTo(dataA, 0);

            dataB = new Byte[_colors.Count];
            _colors.Values.CopyTo(dataB, 0);
#if !NOMULTITHREAD
            _lockColors.ExitReadLock();
#endif
            result.Colors = dataB.ToList<Byte>();
            result.ColorNames = dataA.ToList<String>();

            return result;
        }

        /// <summary>
        /// Get Weapons serialized
        /// </summary>
        /// <returns></returns>
        internal static SerializableWeapons GetSerializableWeapons()
        {
            SerializableWeapons result = new SerializableWeapons();

            ProjectERA.Data.Equipment[] data;
#if !NOMULTITHREAD
            _lockWeapons.EnterReadLock();
#endif
            data = new ProjectERA.Data.Equipment[_weapons.Count];
            _weapons.Values.CopyTo(data, 0);
#if !NOMULTITHREAD
            _lockWeapons.ExitReadLock();
#endif
            result.Weapons = data.ToList<ProjectERA.Data.Equipment>();

            return result;
        }

        /// <summary>
        /// Get Armors serialized
        /// </summary>
        /// <returns></returns>
        internal static SerializableArmors GetSerializableArmors()
        {
            SerializableArmors result = new SerializableArmors();

            ProjectERA.Data.Equipment[] data;
#if !NOMULTITHREAD
            _lockArmors.EnterReadLock();
#endif
            data = new ProjectERA.Data.Equipment[_armors.Count];
            _armors.Values.CopyTo(data, 0);
#if !NOMULTITHREAD
            _lockArmors.ExitReadLock();
#endif
            result.Armors = data.ToList<ProjectERA.Data.Equipment>();

            return result;
        }

        /// <summary>
        /// Get Accessoiries serialized
        /// </summary>
        /// <returns></returns>
        internal static SerializableAccessoiries GetSerializableAccessoiries()
        {
            SerializableAccessoiries result = new SerializableAccessoiries();

            ProjectERA.Data.Equipment[] data;
#if !NOMULTITHREAD
            _lockAccessories.EnterReadLock();
#endif
            data = new ProjectERA.Data.Equipment[_accessoiries.Count];
            _accessoiries.Values.CopyTo(data, 0);
#if !NOMULTITHREAD
            _lockAccessories.ExitReadLock();
#endif
            result.Accessoiries = data.ToList<ProjectERA.Data.Equipment>();

            return result;
        }

        /// <summary>
        /// Get Battler Modifiers serialized
        /// </summary>
        /// <returns></returns>
        internal static SerializableBattlerModifiers GetSerializableBattlerModifiers()
        {
            SerializableBattlerModifiers result = new SerializableBattlerModifiers();

            ProjectERA.Data.BattlerModifier[] data;
#if !NOMULTITHREAD
            _lockStates.EnterReadLock();
            _lockBuffs.EnterReadLock();
#endif
            data = new ProjectERA.Data.BattlerModifier[_buffs.Count + _states.Count];
            using (IEnumerator<ProjectERA.Data.BattlerModifier> enumerator = _buffs.Values.Concat(_states.Values).GetEnumerator())
            {
                Int32 i = 0;
                while (enumerator.MoveNext())
                    data[i++] = enumerator.Current;
            }
#if !NOMULTITHREAD
            _lockBuffs.ExitReadLock();
            _lockStates.ExitReadLock();
#endif
            result.BattlerModifiers = data.ToList<ProjectERA.Data.BattlerModifier>();

            return result;
        }

        /// <summary>
        /// Get BattlerClasses serialized
        /// </summary>
        /// <returns></returns>
        internal static SerializableBattlerClasses GetSerializableBattlerClasses()
        {
            SerializableBattlerClasses result = new SerializableBattlerClasses();

            ProjectERA.Data.BattlerClass[] data;
#if !NOMULTITHREAD
            _lockBattlerClasses.EnterReadLock();
#endif
            data = new ProjectERA.Data.BattlerClass[_battlerClasses.Count];
            _battlerClasses.Values.CopyTo(data, 0);
#if !NOMULTITHREAD
            _lockBattlerClasses.ExitReadLock();
#endif
            result.BattlerClasses = data.ToList<ProjectERA.Data.BattlerClass>();

            return result;
        }

        /// <summary>
        /// Get BattlerRaces serialized
        /// </summary>
        /// <returns></returns>
        internal static SerializableBattlerRaces GetSerializableBattlerRaces()
        {
            SerializableBattlerRaces result = new SerializableBattlerRaces();

            ProjectERA.Data.BattlerRace[] data;
#if !NOMULTITHREAD
            _lockBattlerRaces.EnterReadLock();
#endif
            data = new ProjectERA.Data.BattlerRace[_battlerRaces.Count];
            _battlerRaces.Values.CopyTo(data, 0);
#if !NOMULTITHREAD
            _lockBattlerRaces.ExitReadLock();
#endif
            result.BattlerRaces = data.ToList<ProjectERA.Data.BattlerRace>();

            return result;
        }

        #endregion

        

    }

    /// <summary>
    /// 
    /// </summary>
    internal enum ContentDatabaseType
    {
        None = 0,

        Body,
        Eyes,
        Head,
        Color,

        Weapon,
        Armor,
        Accessory,
        KeyItem,
        Equipment = Weapon | Armor | Accessory | KeyItem,

        BattlerState,
        BattlerBuff,
        BattlerModifier = BattlerBuff | BattlerState,

        BattlerClass,
        BattlerRace,
    }
}
