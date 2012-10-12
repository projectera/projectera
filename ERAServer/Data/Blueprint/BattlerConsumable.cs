using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace ERAServer.Data.Blueprint
{
    internal struct BattlerConsumable
    {
        /// <summary>
        /// Percentage of hp it heals NOTE: needs level influence?
        /// </summary>
        public Double HealthPoints;

        /// <summary>
        /// Percentage of concentration it restores 
        /// </summary>
        public Double Concentration;

        /// <summary>
        /// 0: remove 1: inflict states and buffs
        /// </summary>
        public Dictionary<ObjectId, Boolean> StatesModified;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hpMod"></param>
        /// <param name="cMod"></param>
        /// <param name="statesRemoved"></param>
        /// <param name="statesInflicted"></param>
        /// <returns></returns>
        internal static BattlerConsumable Generate(Double hpMod, Double cMod, List<ObjectId> statesRemoved, List<ObjectId> statesInflicted)
        {
            BattlerConsumable result = new BattlerConsumable();

            result.HealthPoints = hpMod;
            result.Concentration = cMod;

            result.StatesModified = new Dictionary<ObjectId, Boolean>();

            if (statesRemoved != null)
                foreach (ObjectId removed in statesRemoved)
                    result.StatesModified.Add(removed, false);
            if (statesInflicted != null)
                foreach (ObjectId inflicted in statesInflicted)
                    result.StatesModified.Add(inflicted, true);

            return result;
        }
    }
}
