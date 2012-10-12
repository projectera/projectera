using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERAServer.Services;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using IronJS;
using MongoDB.Driver;
using System.IO;

namespace ERAServer.Data.Blueprint
{
    /// <summary>
    /// Responsible for holding the code of a package script
    /// </summary>
    internal class MongoScript
    {
        /// <summary>
        /// Script id
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Script base type
        /// </summary>
        [BsonIgnore]
        public Type Type
        {
            get { return Type.GetType(TypeName);  }
            private set { this.TypeName = value.FullName; }
        }

        [BsonRequired]
        public String TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Script name
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Javascript code
        /// </summary>
        [BsonRequired]
        public String Code
        {
            get;
            private set;
        }


        /// <summary>
        /// Generates a script
        /// </summary>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        internal static MongoScript Generate(String name, String code, Type type = null)
        {
            MongoScript result = new MongoScript();
            result.Id = ObjectId.GenerateNewId();
            result.Name = name;
            result.Code = code;
            result.Type = type ?? typeof(Program);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal MongoScript Copy()
        {
            MongoScript result = Generate(this.Name, this.Code, this.Type);
            result.Id = this.Id;

            return result;
        }

        /// <summary>
        /// Puts a item to the db
        /// </summary>
        internal void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts an item to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return ScriptManager.GetCollection().Save<MongoScript>(this, safemode);
        }
    }
}
