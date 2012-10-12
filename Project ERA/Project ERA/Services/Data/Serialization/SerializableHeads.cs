using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using System.IO;

namespace ProjectERA.Services.Data.Serialization
{
    [Serializable]
    public class SerializableHeads : SerializableDatabaseContent<SerializableHeads>
    {
        /// <summary>
        /// Serializable Content
        /// </summary>
        [ContentSerializer(ElementName = "ColorValues", CollectionItemName = "Value", AllowNull = false)]
        public List<Byte> Colors { get; set; }
        [ContentSerializer(ElementName = "ColorNames", CollectionItemName = "Name", AllowNull = false)]
        public List<String> ColorNames { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items serialized</returns>
        internal Int32 Serialize()
        {
            base.Serialize(SerializableColors.GetFilePath());

            return this.Colors.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Number of items deserialized</returns>
        internal static Int32 Deserialize()
        {

            List<String> dataA = null;
            List<Byte> dataB = null;
            Int32 resultCount = 0;

            SerializableColors loaded = SerializableDatabaseContent<SerializableColors>.Deserialize(GetFilePath());

            if (loaded == null)
                Logger.Logger.LogMessage("Colors could not be loaded, because an exception was thrown while deserializing.", Logger.Severity.Notice);
            else
            {
                dataA = loaded.ColorNames;
                dataB = loaded.Colors;
            }

            // Load data
            if (dataA != null && dataB != null)
            {
                for (Int32 i = 0; i < dataA.Count; i++)
                    ContentDatabase.SetColorByte(dataA[i], dataB[i]);
   
                resultCount = dataA.Count;
            }

            return resultCount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static String GetFilePath()
        {
            return ContentDatabase.GetFilePath("Colors");
        }

    }
}
