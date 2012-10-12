using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ERAServer.Data.Blueprint
{
    internal class Talent
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public Int32 Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String Description
        {
            get;
            private set;
        }
    }
}
