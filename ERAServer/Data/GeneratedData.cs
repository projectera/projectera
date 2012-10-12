using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using ERAServer.Services;
using MongoDB.Driver.Builders;

namespace ERAServer.Data
{
    public class GeneratedData
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String Group
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String Data
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public Int32 Count
        {
            get;
            private set;
        }

        /// <summary>
        /// Generated
        /// </summary>
        [BsonIgnore]
        public DateTime TimeStamp
        {
            get
            {
                return Id.CreationTime;
            }
        }


        /// <summary>
        /// Generates a new Dialogue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal static GeneratedData Generate(String group, String data)
        {
            GeneratedData match = GetCollection().FindOne(Query.And(
                Query.Matches("Group", BsonRegularExpression.Create(new System.Text.RegularExpressions.Regex(group, System.Text.RegularExpressions.RegexOptions.IgnoreCase))), 
                Query.Matches("Data", BsonRegularExpression.Create(new System.Text.RegularExpressions.Regex(data, System.Text.RegularExpressions.RegexOptions.IgnoreCase))))
                );

            if (match != null)
                return match;

            GeneratedData result = new GeneratedData();
            result.Id = ObjectId.GenerateNewId();
            result.Group = group;
            result.Data = data;
            result.Count = 0;

            return result;
        }

        //internal static GeneratedData Fetch(String group)
        //{

        //}

        /// <summary>
        /// 
        /// </summary>
        internal void Attach()
        {
            GetCollection().Update(Query.And(
                    Query.EQ("Id", this.Id),
                    Query.EQ("Group", this.Group),
                    Query.EQ("Data", this.Data)
                ),
                Update.Inc("Count", 1),
                UpdateFlags.Upsert);
        }

        /// <summary>
        /// 
        /// </summary>
        internal void DeAttach()
        {
            GetCollection().Update(Query.And(
                   Query.EQ("Id", this.Id),
                   Query.EQ("Group", this.Group),
                   Query.EQ("Data", this.Data)
               ),
               Update.Inc("Count", -1),
               UpdateFlags.None);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sm"></param>
        /// <returns></returns>
        public void Put()
        {
            GetCollection().Save<GeneratedData>(this);
        }

        /// <summary>
        /// Clears collection
        /// </summary>
        internal static void ClearCollection()
        {
            ClearCollection(true);
        }

        /// <summary>
        /// Drops collection and recreates content
        /// </summary>
        /// <param name="autoPopulate"></param>
        internal static void ClearCollection(Boolean autoPopulate)
        {
            if (GetCollection().Exists())
                GetCollection().Drop();
            GetCollection().CreateIndex("Group", "Data");
            GetCollection().CreateIndex("Id", "Group", "Data");

            if (autoPopulate)
                PopulateCollection();
        }

        /// <summary>
        /// 
        /// </summary>
        internal static void PopulateCollection()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static Task<GeneratedData> Get(ObjectId id)
        {
            return Task<GeneratedData>.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static GeneratedData GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<GeneratedData> GetCollection()
        {
            return DataManager.Database.GetCollection<GeneratedData>("Generated");
        }

    }
}
