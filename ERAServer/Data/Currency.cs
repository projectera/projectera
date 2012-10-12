using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
using MongoDB.Driver;
using ERAServer.Services;

namespace ERAServer.Data
{
    internal class Currency
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
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String PostFix
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String FormatCoin
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String FormatCoins
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String FormatCent
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        public String FormatCents
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="postFix"></param>
        /// <param name="formatCoin"></param>
        /// <param name="formatCoins"></param>
        /// <param name="formatCent"></param>
        /// <param name="formatCents"></param>
        /// <returns></returns>
        public static Currency Generate(String name, String postFix, String formatCoin, String formatCoins, String formatCent, String formatCents)
        {
            Currency result = new Currency();
            result.Id = ObjectId.GenerateNewId();
            result.Name = name;
            result.PostFix = postFix;
            result.FormatCoin = formatCoin;
            result.FormatCoins = formatCoins;
            result.FormatCent = formatCent;
            result.FormatCents = formatCents;

            return result;
        }

        #region Database Get/Put operations
        /// <summary>
        /// Gets a currency from the db
        /// </summary>
        /// <param name="id">id of Guild to get</param>
        /// <returns></returns>
        internal static Task<Currency> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets a currency from the db
        /// </summary>
        /// <param name="username">name of currency to get</param>
        /// <returns></returns>
        internal static Task<Currency> Get(String name)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Gets a currency from the db, blocks while retrieving
        /// </summary>
        /// <param name="id">id of currency to get</param>
        /// <returns></returns>
        internal static Currency GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as Currency;
        }

        /// <summary>
        /// Gets a currency from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">name of currency to get</param>
        /// <returns></returns>
        internal static Currency GetBlocking(String name)
        {
            return GetCollection().FindOneAs<Currency>(Query.Matches("Name", new BsonRegularExpression("^(?i)" + name + "$"))) as Currency;
        }

        /// <summary>
        /// Gets the currency collection
        /// </summary>
        /// <returns></returns>
        internal static MongoCollection<Currency> GetCollection()
        {
            return DataManager.Database.GetCollection<Currency>("Currencies");
        }

        /// <summary>
        /// Puts a currency to the db
        /// </summary>
        internal void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a currency to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Currency>(this, safemode);
        }
        #endregion
    }
}
