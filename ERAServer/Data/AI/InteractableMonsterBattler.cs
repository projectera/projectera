using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using ERAUtils;

namespace ERAServer.Data.AI
{
    [Serializable]
    internal class InteractableMonsterBattler : InteractableBattler, IResetable
    {
        /// <summary>
        /// Id of monster type this interactable is based on
        /// </summary>
        public ObjectId BlueprintId
        {
            get;
            private set;
        }

        internal void SetBlueprintId(ObjectId type)
        {
            this.BlueprintId = type;

            // TODO: Fetch data/or something
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static InteractableMonsterBattler Generate()
        {
            InteractableMonsterBattler result = Pool<InteractableMonsterBattler>.Fetch();
            InteractableBattler.Generate(result);

            //TODO: Random Initialization (can be undone by set blueprint id?)

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public new void Clear()
        {
            base.Clear();
        }
    }
}
