using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.IO;
using ERAUtils.Logger;

namespace ProjectERA.Services.Data.Serialization
{
    [Serializable]
    public class SerializableWeapons : SerializableDatabaseContent<SerializableWeapons>
    {
        /// <summary>
        /// Serializable Content
        /// </summary>
        [ContentSerializer(ElementName = "Weapons", CollectionItemName = "Weapon", AllowNull = false)]
        public List<ProjectERA.Data.Equipment> Weapons { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items serialized</returns>
        internal Int32 Serialize()
        {
            base.Serialize(SerializableWeapons.GetFilePath());

            return this.Weapons.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items deserialized</returns>
        internal static Int32 Deserialize()
        {

            List<ProjectERA.Data.Equipment> data = null;
            Int32 resultCount = 0;

            SerializableWeapons loaded = SerializableDatabaseContent<SerializableWeapons>.Deserialize(GetFilePath());

            if (loaded == null)
                Logger.Notice("Weapons could not be loaded, because an exception was thrown while deserializing.");
            else
                data = loaded.Weapons;

            // Load data
            if (data != null)
            {
                foreach (ProjectERA.Data.Equipment weapon in data)
                    ContentDatabase.SetEquipmentWeapon(weapon);

                resultCount = data.Count;
            }

            return resultCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static String GetFilePath()
        {
            return ContentDatabase.GetFilePath("Weapons");
        }

    }
}
