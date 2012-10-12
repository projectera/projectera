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
    internal class SerializableAccessoiries : SerializableDatabaseContent<SerializableAccessoiries>
    {
        /// <summary>
        /// Serializable Content
        /// </summary>
        [ContentSerializer(ElementName = "Accessoiries", CollectionItemName = "Accessoiry", AllowNull = false)]
        internal List<ProjectERA.Data.Equipment> Accessoiries { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items serialized</returns>
        internal Int32 Serialize()
        {
            base.Serialize(SerializableAccessoiries.GetFilePath());

            return this.Accessoiries.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items deserialized</returns>
        internal static Int32 Deserialize()
        {
            List<ProjectERA.Data.Equipment> data = null;
            Int32 resultCount = 0;

            SerializableAccessoiries loaded = SerializableDatabaseContent<SerializableAccessoiries>.Deserialize(SerializableAccessoiries.GetFilePath());

            if (loaded == null)
                Logger.Notice("Accessoiries could not be loaded, because an exception was thrown while deserializing.");
            else
                data = loaded.Accessoiries;

            // Load data
            if (data != null)
            {
                foreach (ProjectERA.Data.Equipment accessoiry in data)
                    ContentDatabase.SetEquipmentAccessoiry(accessoiry);

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
            return ContentDatabase.GetFilePath("Accessories");
        }

    }
}
