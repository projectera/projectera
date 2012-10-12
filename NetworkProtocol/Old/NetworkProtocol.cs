using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectERA.Protocols
{
    /// <summary>
    /// 
    /// Range: 0-31
    /// </summary>
    public enum NetworkProtocol : byte
    {
        /// <summary>
        /// 0: Invalid Byte
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// 1: Disconnect from Game
        /// </summary>
        Disconnect = 1,
        
        /// <summary>
        /// 2: Server Message
        /// </summary>
        ServerMessage = 2,
        
        /// <summary>
        /// 3: Next Protocol > Update
        /// </summary>
        Update = 3,

        /// <summary>
        /// 4: Next Protocol > Moderation
        /// </summary>
        Moderation = 4,

        /// <summary>
        /// 5: Next Protocol > Player
        /// </summary>
        Player = 5,
        
        /// <summary>
        /// 6: Next Protcol > Character
        /// </summary>
        Character = 6,

        /// <summary>
        /// 7: Next Protocol > Battle
        /// </summary>
        Battle = 7,

        /// <summary>
        /// 8: Next Protocol > Team
        /// </summary>
        Team = 8,

        /// <summary>
        /// 9: Next Protocol > Guild
        /// </summary>
        Guild = 9,
        
        /// <summary>
        /// 10: Next Protocol > Faction
        /// </summary>
        Faction = 10,

        /// <summary>
        /// 11: Next Protocol > Chat
        /// </summary>
        Chat = 11,

        /// <summary>
        /// 12: Next Protocol > Trade
        /// </summary>
        Trade = 12,
        
        /// <summary>
        /// 13: Next Protocol > Shop
        /// </summary>
        Shop = 13,

        /// <summary>
        /// 20: Next Protocol > Map
        /// </summary>
        Map = 20,

        /// <summary>
        /// 31: Next Protocol > DataIntegrity
        /// </summary>
        DataIntegrity = 31,
        Max = ProjectERA.Protocols.ProtocolConstants.NetworkMaxValue
    }


    /// <summary>
    /// 
    /// Range: 0-15
    /// </summary>
    [Flags]
    public enum PermissionGroup : byte
    {
        None = 0,               // 0000
        BetaTester = 1 << 0,    // 0001
        GameObserver = 1 << 1,  // 0010
        GameMaster = 1 << 2,    // 0100
        God = 1 << 3,           // 1000
    }

    /// <summary>
    /// 
    /// </summary>
    public enum NetworkProtocolPermissions : byte
    {
        ServerMessage = (PermissionGroup.GameObserver | PermissionGroup.GameMaster | PermissionGroup.God),
        Moderation = (PermissionGroup.GameObserver | PermissionGroup.GameMaster | PermissionGroup.God),
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ProtocolConstants
    {
        public const Int32 NetworkMaxValue = 255;       // byte

        public const Int32 UpdateMaxValue = 7;          // 3 bit
        public const Int32 ModerationMaxValue = 31;     // 5 bit
        public const Int32 PlayerMaxValue = 15;         // 4 bit
        public const Int32 CharacterMaxValue = 31;      // 5 bit

        public const Int32 BattleMaxValue = 254;
        public const Int32 TeamMaxValue = 254;
        public const Int32 GuildMaxValue = 254;
        public const Int32 FactionMaxValue = 254;
        public const Int32 ChatMaxValue = 254;
        public const Int32 TradeMaxValue = 254;
        public const Int32 ShopMaxValue = 254;
            
        public const Int32 MapMaxValue = 31;            // 5 bit

        public const Int32 DataIntegrityMaxValue = 7;   // 3 bit
    }
}
