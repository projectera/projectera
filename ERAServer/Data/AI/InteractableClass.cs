using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ERAServer.Data.AI
{
    internal class InteractableClass
    {
        /// <summary>
        /// Database (Blueprint) Class Id
        /// </summary>
        [BsonRequired]
        public Int32 BlueprintId
        {
            get;
            private set;
        }

        /*/// <summary>
        /// Interactable id this belongs too
        /// </summary>
        public ObjectId InteractableId
        {
            get;
            protected set;
        }*/

        // TODO chosen talents
        // TODO points etc


        internal static InteractableClass Generate(int classId)
        {
            //throw new NotImplementedException();
            InteractableClass result = new InteractableClass();
            result.BlueprintId = classId;

            return result;
        }
    }
}
