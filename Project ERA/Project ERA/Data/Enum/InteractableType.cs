using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectERA.Data.Enum
{
    [Flags]
    public enum InteractableType : short
    {
        None = 0,

        /// <summary>
        /// If set: Name includes tileId
        /// If not set: Name includes overhead
        /// </summary>
        IsTile = (1 << 0),

        /// <summary>
        /// If set: Uses more than one asset
        /// If not set: Uses single asset
        /// </summary>
        MultipleAssets = (1 << 1),

        /// <summary>
        /// If set: accepts interaction with player
        /// </summary>
        HasInteraction = (1 << 2),

        /// <summary>
        /// If set: Has dialogue options
        /// </summary>
        HasDialogue = (1 << 3) | HasInteraction,

        /// <summary>
        /// If set: Can do damage
        /// </summary>
        HasBattler = (1 << 4) | HasInteraction,

        /// <summary>
        /// If set: Can trade
        /// </summary>
        HasTrade = (1 << 5) | HasInteraction,

        /// <summary>
        /// If set: Has key item
        /// </summary>
        HasKeyItem = (1 << 6) | HasInteraction,

        /// <summary>
        /// If set: Has Effort
        /// </summary>
        HasEffort = (1 << 7) | HasInteraction,


        /// <summary>
        /// If set: Is an Avatar
        /// </summary>
        Avatar = (1 << 8) | HasInteraction | HasTrade | HasBattler | HasDialogue

    }
}
