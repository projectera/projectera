using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERAServer.Data.Enum
{
    [Flags]
    public enum ActionRestriction : byte
    {
        None = 0,

        NoSkill = (1 << 0),     // No skill usage
        NoAttack = (1 << 1),    // No attack
        NoMovement = (1 << 2),  // No movement
        NoItem = (1 << 3),      // No item usage
        NoTrade = (1 << 4),     // No trading

        NoAction = NoSkill | NoAttack | NoMovement | NoItem | NoTrade,

        AttackAllies = (1 << 6), //
        AttackEnemies = (1 << 7) //

    }
}
