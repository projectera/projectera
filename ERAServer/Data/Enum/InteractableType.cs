using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    [Flags]
    internal enum InteractableType : ushort
    {
        /// <summary>
        /// Has no properties
        /// </summary>
        None = 0,

        /// <summary>
        /// When has appearance, will have a body with body elements
        /// </summary>
        HasAppearance = (1 << 0),

        /// <summary>
        /// When has movement, will send try move commands and move around
        /// </summary>
        HasMovement = (1 << 1),

        /// <summary>
        /// When has dialogue, will have (pre-defined) dialogue
        /// </summary>
        HasDialogue = (1 << 2),

        /// <summary>
        /// When has shop, will sell certain items (and buy some)
        /// </summary>
        HasShop = (1 << 3),

        /// <summary>
        /// When has trade, will trade 'certain' items 
        /// </summary>
        HasTrading = (1 << 4),

        /// <summary>
        /// When has battler, will be able to fight
        /// </summary>
        /// <remarks>IsMonsterBattlerflag can be set</remarks>
        HasBattler = (1 << 5),

        /// <summary>
        /// When has inventory, will be able to hold items
        /// </summary>
        HasInventory = (1 << 6),

        /// <summary>
        /// When has team, will cooperate with team members
        /// </summary>
        HasTeam = (1 << 7),

        /// <summary>
        /// When has faction, will cooperate with faction members
        /// </summary>
        HasFaction = (1 << 8),

        /// <summary>
        /// When has guild, will cooperate with guild members
        /// </summary>
        HasGuild = (1 << 9),

        /// <summary>
        /// When has effort, will be holding effort values
        /// </summary>
        HasEffort = (1 << 10), //Note or maybe include this in ai functions, not as property flag?

        /// <summary>
        /// When is Monster Battler and HasBattler, Battler will be an MonsterBattler instead of HumanBattler
        /// </summary>
        IsMonsterBattler = (1 << 11),

        /// <summary>
        /// Default flags for avatars
        /// </summary>
        DefaultAvatar = HasAppearance | HasMovement | HasDialogue | HasTrading | HasBattler | HasInventory | HasTeam | HasFaction | HasGuild,

        /// <summary>
        /// Default flags for monsters
        /// </summary>
        DefaultMonster = HasAppearance | HasMovement | HasBattler | HasTeam | IsMonsterBattler, //HasInventory

        /// <summary>
        /// Default flags for traders
        /// </summary>
        DefaultTrader = HasAppearance | HasMovement | HasTrading | HasInventory,

        /// <summary>
        /// Default flags for salesman
        /// </summary>
        /// <remarks>Movement input not automated</remarks>
        DefaultSalesman = HasAppearance | HasMovement | HasShop, //HasInventory

        /// <summary>
        /// Default flags for people
        /// </summary>
        DefaultPeople = HasAppearance | HasMovement | HasDialogue,

        /// <summary>
        /// Default flags for chests
        /// </summary>
        /// <remarks>Movement input not automated</remarks>
        DefaultChest = HasAppearance | HasMovement | HasInventory,

        /// <summary>
        /// Default flags for grass
        /// </summary>
        DefaultGrass = HasAppearance,
    }
}
