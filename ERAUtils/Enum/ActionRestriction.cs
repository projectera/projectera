using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAUtils.Enum
{
    [Flags]
    public enum ActionRestriction : byte
    {
        None = 0,

        NoAction = (1 << 0),    // 
        NoSkill = (1 << 1),     // No skill usage
        NoAttack = (1 << 2),    // No attack
        NoMovement = (1 << 3),  // No movement
        NoItem = (1 << 4),      // No item usage
        NoTrade = (1 << 5),     // No trading

        All = NoAction | NoSkill | NoAttack | NoMovement | NoItem | NoTrade,

        AttackAllies = (1 << 6), //
        AttackEnemies = (1 << 7) //

    }
}
