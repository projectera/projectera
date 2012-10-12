using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAUtils.Enum;
using System.Threading.Tasks;
using ERAUtils;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.IO;
using System.Xml;

namespace ContentConverter.Data
{
    internal static class Map
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static ERAServer.Data.Map Generate(Int32 id, UInt16 width, UInt16 height, Int32 tilesetId, String name, String fog, Byte fogOpacity,
            String panorama, UInt16[][][] data)
        {
            ERAServer.Data.Map result = new ERAServer.Data.Map();
            result.Id = new ObjectId(new MongoObjectId(id).ToByteArray());
            result.TilesetId = new ObjectId(new MongoObjectId(tilesetId).ToByteArray());
            result.RegionId = ObjectId.Empty;
            result.Name = name;
            result.Type = MapType.NotSpecified;
            result.Settings = new ERAServer.Data.Map.MapSettings();
            result.Settings.FogAssetName = fog;
            result.Settings.FogOpacity = fogOpacity;
            result.Settings.PanormaAssetName = panorama;
            result.Width = width;
            result.Height = height;
            result.Data = data;
            result.Version = 1;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static ERAServer.Data.Map Generate(UInt16[][][] data)
        {
            ERAServer.Data.Map result = new ERAServer.Data.Map();
            result.Id = ObjectId.GenerateNewId();
            result.TilesetId = ObjectId.Empty;
            result.RegionId = ObjectId.Empty;
            result.Name = String.Empty;
            result.Type = MapType.NotSpecified;
            result.Settings = new ERAServer.Data.Map.MapSettings();
            result.Width = (UInt16)data.Length;
            result.Height = result.Width > 0 ? (UInt16)data[0].Length : (UInt16)0;
            result.Data = data;
            result.Version = 1;

            return result;
        }

        /// <summary>
        /// Loads a tileset from XML file
        /// </summary>
        /// <param name="stream">file stream</param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static ERAServer.Data.Map FromFile(Stream stream, String path)
        {
            Int32 id = 0, tilesetId = 0;
            UInt16 width = 0, height = 0;
            String name = String.Empty, panorama = String.Empty, fog = String.Empty, mapdata = String.Empty;
            Byte fogOpacity = 0;

            using (XmlTextReader reader = new XmlTextReader(stream))
            {
                try
                {
                    Boolean settingsSub = false;
                    Boolean fogSub = false;
                    Boolean panoramaSub = false;

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            switch (reader.Name)
                            {
                                case "settings":
                                    settingsSub = false;
                                    break;
                                case "fog":
                                    fogSub = false;
                                    break;
                                case "panorama":
                                    fogSub = false;
                                    break;
                            }

                            continue;
                        }

                        if (settingsSub)
                        {
                            if (fogSub)
                            {
                                switch (reader.Name)
                                {
                                    case "graphic":
                                        if (reader.Read())
                                            fog = reader.Value;
                                        break;
                                    case "opacity":
                                        if (reader.Read())
                                            fogOpacity = Byte.Parse(reader.Value);
                                        break;
                                }
                            }
                            else if (panoramaSub)
                            {
                                switch (reader.Name)
                                {
                                    case "graphic":
                                        if (reader.Read())
                                            panorama = reader.Value;
                                        break;
                                }
                            }
                            else
                            {
                                switch (reader.Name)
                                {
                                    case "fog":
                                        fogSub = true;
                                        break;
                                    case "panorama":
                                        panoramaSub = true;
                                        break;
                                }
                            }

                        }
                        else
                        {

                            switch (reader.Name)
                            {
                                case "id":
                                    if (reader.Read())
                                        id = Int32.Parse(reader.Value);
                                    break;
                                case "width":
                                    if (reader.Read())
                                        width = UInt16.Parse(reader.Value);
                                    break;
                                case "height":
                                    if (reader.Read())
                                        height = UInt16.Parse(reader.Value);
                                    break;
                                case "tilesetId":
                                    if (reader.Read())
                                        tilesetId = Int32.Parse(reader.Value);
                                    break;
                                case "name":
                                    if (reader.Read())
                                        name = reader.Value;
                                    break;
                                case "mapDataFile":
                                    if (reader.Read())
                                        mapdata = reader.Value;
                                    break;
                                case "settings":
                                    settingsSub = true;
                                    break;
                            }
                        }
                    }
                }
                catch (XmlException)
                {

                }
            }

            if (mapdata == String.Empty)
                return null;

            return Map.Generate(id, width, height, tilesetId, name, fog, fogOpacity, panorama, GetDataFromFile(width, height, path + "/" + mapdata));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static UInt16[][][] GetDataFromFile(Int32 width, Int32 height, String filename)
        {
            UInt16[][][] result = new UInt16[width][][];

            try
            {
                using (Stream openFileStream = File.OpenRead(filename))
                {
                    using (StreamReader reader = new StreamReader(openFileStream))
                    {
                        for (UInt16 x = 0; x < width; x++)
                        {
                            result[x] = new UInt16[height][];
                            for (UInt16 y = 0; y < height; y++)
                            {
                                result[x][y] = new UInt16[3];

                                String[] tiles = reader.ReadLine().Split(',');

                                result[x][y][0] = UInt16.Parse(tiles[0]);
                                result[x][y][1] = UInt16.Parse(tiles[1]);
                                result[x][y][2] = UInt16.Parse(tiles[2]);
                            }
                        }
                    }
                }
            }
            catch (IOException)
            {
                throw new InvalidDataException("File does not contain width * heigth * layers of data");
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="width"></param>
        /// <param name="heigth"></param>
        /// <param name="data"></param>
        internal static void SetData(ERAServer.Data.Map map, UInt16 width, UInt16 heigth, UInt16[][][] data)
        {
            map.Width = width;
            map.Height = heigth;
            map.Data = data;
        }
    }
}
