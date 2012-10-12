using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectERA.Services.Data;
using Lidgren.Network;
using ERAUtils;
using ERAUtils.Logger;

namespace ProjectERA.Services.Network.Protocols
{
    internal partial class Map : Protocol
    {
        /// <summary>
        /// Unpacks map data from a message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal static MapData UnpackMapData(NetIncomingMessage msg)
        {
            Data.MapData result = new Data.MapData();
            result.MapId = (MongoObjectId)msg.ReadBytes(12);

            Logger.Info("Unpacking mapdata for " + result.MapId);

            result.TilesetId = (MongoObjectId)msg.ReadBytes(12);
            result.RegionId = (MongoObjectId)msg.ReadBytes(12);
            result.Name = msg.ReadString();
            result.MapType = (Data.MapType)msg.ReadInt32();
            result.MapSettings = new Data.MapSettings();
            result.MapSettings.FogAssetName = msg.ReadString();
            result.MapSettings.FogOpacity = msg.ReadByte();
            result.MapSettings.PanormaAssetName = msg.ReadString();
            if (String.IsNullOrWhiteSpace(result.MapSettings.FogAssetName))
                result.MapSettings.FogAssetName = null;
            if (String.IsNullOrWhiteSpace(result.MapSettings.PanormaAssetName))
                result.MapSettings.PanormaAssetName = null;
            result.Width = msg.ReadUInt16();
            result.Height = msg.ReadUInt16();

            UInt16[][][] data = new UInt16[result.Width][][];
            for (Int32 i = 0; i < result.Width; i++)
            {
                data[i] = new UInt16[result.Height][];
                for (Int32 j = 0; j < result.Height; j++)
                {
                    data[i][j] = new UInt16[3];

                    data[i][j][0] = msg.ReadUInt16();
                    data[i][j][1] = msg.ReadUInt16();
                    data[i][j][2] = msg.ReadUInt16();
                }
            }

            result.TileData = data;

            result.Version = msg.ReadUInt32();

            return result;
        }

        /// <summary>
        /// Unpacks tileset data from a message
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal static TilesetData UnpackTilesetData(NetIncomingMessage msg)
        {
            Data.TilesetData result = new Data.TilesetData();
            result.TilesetId = (MongoObjectId)msg.ReadBytes(12);

            Logger.Info("Unpacking tilesetdata for " + result.TilesetId);

            result.Name = msg.ReadString();
            result.AssetName = msg.ReadString();
            result.Tiles = msg.ReadInt32();
            Int32 autotileCount = msg.ReadByte();
            result.AutotileAssetNames = new List<String>(autotileCount);
            for (Int32 autotileAssetI = 0; autotileAssetI < autotileCount; autotileAssetI++)
                result.AutotileAssetNames.Add(msg.ReadString());
            result.AutotileAnimationFlags = new List<Boolean>(autotileCount);
            Byte autotileBitField = msg.ReadByte();
            for (Int32 autotileAssetI = 0; autotileAssetI < autotileCount; autotileAssetI++)
                result.AutotileAnimationFlags.Add((autotileBitField & (1 << autotileAssetI)) == (1 << autotileAssetI));

            Int32 tilesetDataLength = msg.ReadInt32();
            result.Passages = new Byte[tilesetDataLength];
            result.Priorities = new Byte[tilesetDataLength];
            result.Flags = new Byte[tilesetDataLength];
            result.Tags = new Byte[tilesetDataLength];
            for (Int32 tilesetDataI = 0; tilesetDataI < tilesetDataLength; tilesetDataI++)
            {
                result.Passages[tilesetDataI] = msg.ReadByte();
                result.Priorities[tilesetDataI] = msg.ReadByte();
                result.Flags[tilesetDataI] = msg.ReadByte();
                result.Tags[tilesetDataI] = msg.ReadByte();
            }

            result.Version = msg.ReadUInt32();
            
            return result;
        }

        /// <summary>
        /// Unpacks validation data from a message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="mapValid"></param>
        /// <param name="tilesetValid"></param>
        internal static MongoObjectId UnpackValidation(NetIncomingMessage msg, out Int32 validationField)
        {
            MongoObjectId key = (MongoObjectId)msg.ReadBytes(12);
            validationField = msg.ReadInt32();

            return key;
        }

        /// <summary>
        /// Notificates
        /// </summary>
        /// <param name="p"></param>
        internal static void Notificate(String notification)
        {
            OnNotification.Invoke(notification, EventArgs.Empty);
        }
    }
}
