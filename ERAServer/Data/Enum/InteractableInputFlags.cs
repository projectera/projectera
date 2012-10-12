using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    [Flags]
    internal enum InteractableInputFlags : ushort
    {
        /// <summary>
        /// All components automated
        /// </summary>
        None = 0,

        /// <summary>
        /// When appearance input, will not automate appearance
        /// </summary>
        Appearance = (1 << 0),

        /// <summary>
        /// When movement input, will not automate movement
        /// </summary>
        Movement = (1 << 1),

        /// <summary>
        /// When dialogue input, will not automate dialogue
        /// </summary>
        Dialogue = (1 << 2),

        /// <summary>
        /// When shop input, will not automate shop
        /// </summary>
        Shop = (1 << 3),

        /// <summary>
        /// When trade input, will not automate trading
        /// </summary>
        Trading = (1 << 4),

        /// <summary>
        /// When battler input, will not automate battler
        /// </summary>
        Battler = (1 << 5),

        /// <summary>
        /// When inventory input, will not automate inventory
        /// </summary>
        /// <remarks>Nothing to automate here</remarks>
        Inventory = (1 << 6),

        /// <summary>
        /// When team input, will not automate team
        /// </summary>
        Team = (1 << 7),

        /// <summary>
        /// When faction input, will not automate faction
        /// </summary>
        Faction = (1 << 8),

        /// <summary>
        /// When guild input, will not automate guild
        /// </summary>
        Guild = (1 << 9),

        /// <summary>
        /// When effort, will not automate effort
        /// </summary>
        /// <remarks>Efforts are fully automated</remarks>
        Effort = (1 << 10),

        /// <summary>
        /// Default values for avatars (fully controlled)
        /// </summary>
        DefaultAvatar = Appearance | Movement | Dialogue | Shop | Trading | Battler | Team | Faction | Guild | Effort | Inventory, 

        /// <summary>
        /// Default values for monsters (fully automated)
        /// </summary>
        DefaultMonster = None,

        /// <summary>
        /// Default values for traders (fully automated)
        /// </summary>
        DefaultTrader = None,

        /// <summary>
        /// Default values for salesmen (fully automated but movement controlled)
        /// </summary>
        DefaultSalesman = Movement,

        /// <summary>
        /// Default values for people (fully automated)
        /// </summary>
        DefaultPeople = None,

        /// <summary>
        /// Default values for chests (fully automated, but movement controlled)
        /// </summary>
        DefaultChest = Movement,

        /// <summary>
        /// Default values for grass (fully automated)
        /// </summary>
        DefaultGrass = None,
    }
}
