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
    internal class SerializableBattlerClasses : SerializableDatabaseContent<SerializableBattlerClasses>
    {
        /// <summary>
        /// Serializable Content
        /// </summary>
        [ContentSerializer(ElementName = "BattlerClasses", CollectionItemName = "BattlerClass", AllowNull = false)]
        internal List<ProjectERA.Data.BattlerClass> BattlerClasses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items serialized</returns>
        internal Int32 Serialize()
        {
            base.Serialize(SerializableBattlerClasses.GetFilePath());

            return this.BattlerClasses.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items deserialized</returns>
        internal static Int32 Deserialize()
        {
            List<ProjectERA.Data.BattlerClass> data = null;
            Int32 resultCount = 0;

            SerializableBattlerClasses loaded = SerializableDatabaseContent<SerializableBattlerClasses>.Deserialize(SerializableBattlerClasses.GetFilePath());

            if (loaded == null)
                Logger.Notice("BattlerClasses could not be loaded, because an exception was thrown while deserializing.");
            else
                data = loaded.BattlerClasses;

            // Load data
            if (data != null)
            {
                foreach (ProjectERA.Data.BattlerClass battlerClass in data)
                    ContentDatabase.SetBattlerClass(battlerClass);

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
            return ContentDatabase.GetFilePath("BattlerClasses");
        }

    }
}
