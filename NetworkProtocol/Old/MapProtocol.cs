using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Protocols
{
    /// <summary>
    /// Range: 0-31
    /// </summary>
    public enum MapProtocol : byte
    {
        /// <summary>
        /// 0: Invalid
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 1: Teleport to Map [by teleport or moderation]
        /// </summary>
        TeleportToMap = 1,

        /// <summary>
        /// 2: Player Joined Game
        /// </summary>
        PlayerJoined = 2,

        /// <summary>
        /// 3: Player Left Game
        /// </summary>
        PlayerLeft = 3,

        /// <summary>
        /// 
        /// </summary>
        PlayerInnerMapOn = 4,

        /// <summary>
        /// 
        /// </summary>
        PlayerInnerMapOff = 5,

        /// <summary>
        /// 10: MapData [header for mapdata]
        /// </summary>
        HeaderData = 10,

        /// <summary>
        /// 11: Player Map Data
        /// </summary>
        PlayerMapData = 11,

        /// <summary>
        /// 12: Event Map Data
        /// </summary>
        EventMapData = 12,

        /// <summary>
        /// 13: Monster Map Data
        /// </summary>
        MonsterMapData = 13,

        /// <summary>
        /// 14: Weather Map Data
        /// </summary>
        WeatherMapData = 14,

        /// <summary>
        /// 
        /// </summary>
        MapSettingsFog = 20,

        /// <summary>
        /// 
        /// </summary>
        MapSettingsTone = 21,

        /// <summary>
        /// 
        /// </summary>
        MapSettingsWheather = 22,
    }
}
