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
    internal class SerializableBattlerModifiers : SerializableDatabaseContent<SerializableBattlerModifiers>
    {
        /// <summary>
        /// Serializable Content
        /// </summary>
        [ContentSerializer(ElementName = "BattlerModifiers", CollectionItemName = "Modifier", AllowNull = false)]
        internal List<ProjectERA.Data.BattlerModifier> BattlerModifiers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items serialized</returns>
        internal Int32 Serialize()
        {
            base.Serialize(SerializableBattlerModifiers.GetFilePath());

            return this.BattlerModifiers.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items deserialized</returns>
        internal static Int32 Deserialize()
        {
            List<ProjectERA.Data.BattlerModifier> data = null;
            Int32 resultCount = 0;

            SerializableBattlerModifiers loaded = SerializableDatabaseContent<SerializableBattlerModifiers>.Deserialize(SerializableBattlerModifiers.GetFilePath());

            if (loaded == null)
                Logger.Notice("Battlermodifiers could not be loaded, because an exception was thrown while deserializing.");
            else
                data = loaded.BattlerModifiers;

            // Load data
            if (data != null)
            {
                foreach (ProjectERA.Data.BattlerModifier modifier in data)
                    ContentDatabase.SetBattlerModifier(modifier);

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
            return ContentDatabase.GetFilePath("BattlerModifiers");
        }

    }
}
