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
    internal class SerializableBattlerRaces : SerializableDatabaseContent<SerializableBattlerRaces>
    {
        /// <summary>
        /// Serializable Content
        /// </summary>
        [ContentSerializer(ElementName = "BattlerRaces", CollectionItemName = "BattlerRace", AllowNull = false)]
        internal List<ProjectERA.Data.BattlerRace> BattlerRaces { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items serialized</returns>
        internal Int32 Serialize()
        {
            base.Serialize(SerializableBattlerRaces.GetFilePath());

            return this.BattlerRaces.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items deserialized</returns>
        internal static Int32 Deserialize()
        {
            List<ProjectERA.Data.BattlerRace> data = null;
            Int32 resultCount = 0;

            SerializableBattlerRaces loaded = SerializableDatabaseContent<SerializableBattlerRaces>.Deserialize(SerializableBattlerRaces.GetFilePath());

            if (loaded == null)
                Logger.Notice("BattlerRaces could not be loaded, because an exception was thrown while deserializing.");
            else
                data = loaded.BattlerRaces;

            // Load data
            if (data != null)
            {
                foreach (ProjectERA.Data.BattlerRace BattlerRace in data)
                    ContentDatabase.SetBattlerRace(BattlerRace);

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
            return ContentDatabase.GetFilePath("BattlerRaces");
        }

    }
}
