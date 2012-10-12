using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Threading.Tasks;
using System.Diagnostics;
using ERAUtils;
using System.IO;
using System.Xml;

namespace ContentConverter.Data
{
    internal static class Tileset
    {
       /// <summary>
       /// 
       /// </summary>
       /// <param name="id"></param>
       /// <param name="name"></param>
       /// <param name="assetName"></param>
       /// <param name="autotileNames"></param>
       /// <param name="passages"></param>
       /// <param name="priorities"></param>
       /// <param name="flags"></param>
       /// <param name="tags"></param>
       /// <returns></returns>
        internal static ERAServer.Data.Tileset Generate(Int32 id, String name, String assetName, List<String> autotileNames,
            Byte[] passages, Byte[] priorities, Byte[] flags, Byte[] tags, Int32 tiles)
        {
            
            return ERAServer.Data.Tileset.Generate(new ObjectId(new MongoObjectId(id).ToByteArray()), 
                name, assetName, autotileNames, null, passages, priorities, flags, tags, tiles, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="assetName"></param>
        /// <param name="autotileNames"></param>
        /// <param name="autotileAnimation"></param>
        /// <param name="passages"></param>
        /// <param name="priorities"></param>
        /// <param name="flags"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        internal static ERAServer.Data.Tileset Generate(ObjectId id, String name, String assetName, List<String> autotileNames, List<Boolean> autotileAnimation,
            Byte[] passages, Byte[] priorities, Byte[] flags, Byte[] tags, Int32 tiles, UInt32 version)
        {
           return ERAServer.Data.Tileset.Generate(id, name, assetName, autotileNames, 
                autotileAnimation, passages, priorities, flags, tags, tiles, 0);
        }

        /// <summary>
        /// Loads a tileset from XML file
        /// </summary>
        /// <param name="stream">file stream</param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static ERAServer.Data.Tileset FromFile(Stream stream, String path)
        {
            Int32 id = 0;
            UInt16 tiles = 0;
            String name = String.Empty, tsdata = String.Empty, assetName = String.Empty;
            List<String> autotiles = new List<String>();

            using (XmlTextReader reader = new XmlTextReader(stream))
            {
                try
                {  
                    Boolean autoTilesSub = false;

                        while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            switch (reader.Name)
                            {
                                case "autotiles":
                                    autoTilesSub = false;
                                    break;
                            }

                            continue;
                        }

                        switch (reader.Name)
                        {
                            case "id":
                                if (reader.Read())
                                    id = Int32.Parse(reader.Value);
                                break;
                            case "size":
                                if (reader.Read())
                                    tiles = UInt16.Parse(reader.Value);
                                break;
                            case "name":
                                if (reader.Read())
                                    if (autoTilesSub)
                                        autotiles.Add(reader.Value);
                                    else
                                        name = reader.Value;
                                break;
                            case "graphic":
                                if (reader.Read())
                                    assetName = String.IsNullOrWhiteSpace(assetName) ? reader.Value : assetName;
                                break;
                            case "autotiles":
                                autoTilesSub = true;
                                break;
                            case "tilesetDataFile":
                                if (reader.Read())
                                    tsdata = reader.Value;
                                break;

                        }
                    }
                }
                catch (XmlException)
                {

                }
            }

            if (tsdata == String.Empty)
                return null;

            Byte[] priorities, passages, flags, tags;
            passages = new Byte[tiles];
            priorities = new Byte[tiles];
            flags = new Byte[tiles];
            tags = new Byte[tiles];

            using (Stream openFileStream = File.OpenRead(path + "/" + tsdata))
            {
                using (StreamReader reader = new StreamReader(openFileStream))
                {
                    // Passages
                    String[] line = reader.ReadLine().Split(' ');
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        passages[x] = Byte.Parse(line[x]);
                    }

                    // Priorities
                    line = reader.ReadLine().Split(' ');
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        priorities[x] = Byte.Parse(line[x]);
                    }

                    // Flags
                    line = reader.ReadLine().Split(' ');
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        flags[x] = Byte.Parse(line[x]);
                    }

                    // Tags
                    for (UInt16 x = 0; x < tiles; x++)
                    {
                        tags[x] = 0;
                    }
                }
            }
            
            return Tileset.Generate(id, name, assetName, autotiles, passages, priorities, flags, tags, (Int32)(tiles-384));
        }
    }
}
