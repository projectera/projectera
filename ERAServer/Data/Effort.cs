using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ERAServer.Data
{
    internal class Effort
    {
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        [BsonRequired]
        public ObjectId BlueprintId 
        { 
            get; 
            private set; 
        }

        [BsonRequired]
        public ObjectId Owner
        {
            get;
            private set;
        }

        [BsonRequired]
        public List<Blueprint.Effort.Condition> Completed
        {
            get;
            private set;
        }


    }
}
