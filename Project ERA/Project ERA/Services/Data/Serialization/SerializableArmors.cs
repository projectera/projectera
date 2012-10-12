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
    internal class SerializableArmors : SerializableDatabaseContent<SerializableArmors>
    {
        /// <summary>
        /// Serializable Content
        /// </summary>
        [ContentSerializer(ElementName = "Armors", CollectionItemName = "Armor", AllowNull = false)]
        internal List<ProjectERA.Data.Equipment> Armors { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items serialized</returns>
        internal Int32 Serialize()
        {
            base.Serialize(SerializableArmors.GetFilePath());

            return this.Armors.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items deserialized</returns>
        internal static Int32 Deserialize()
        {
            List<ProjectERA.Data.Equipment> data = null;
            Int32 resultCount = 0;

            SerializableArmors loaded = SerializableDatabaseContent<SerializableArmors>.Deserialize(SerializableArmors.GetFilePath());

            if (loaded == null)
                Logger.Notice("Armors could not be loaded, because an exception was thrown while deserializing.");
            else
                data = loaded.Armors;

            // Load data
            if (data != null)
            {
                foreach (ProjectERA.Data.Equipment armor in data)
                    ContentDatabase.SetEquipmentArmor(armor);

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
            return ContentDatabase.GetFilePath("Armors");
        }

    }
}
