using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Threading.Tasks;
using MongoDB.Driver;
using ERAUtils;
using ERAServer.Data.Blueprint;
using MongoDB.Driver.Builders;
using ERAUtils.Logger;
using IronJS;
using IronJS.Hosting;

namespace ERAServer.Services
{
    internal static class ScriptManager
    {
        private static Dictionary<String, Int32> _counters;
       
        /// <summary>
        /// 
        /// </summary>
        internal static void Initialize()
        {
            _counters = new Dictionary<String, Int32>();
        }

        /// <summary>
        /// Get a script
        /// </summary>
        /// <param name="name">Script name</param>
        /// <returns></returns>
        public static Task<MongoScript> Get(String name)
        {
            return Task<MongoScript>.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Get a script
        /// </summary>
        /// <param name="id">Script id</param>
        /// <returns></returns>
        public static Task<MongoScript> Get(ObjectId id)
        {
            return Task<MongoScript>.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MongoScript GetBlocking(String name)
        {
            GetCollection().EnsureIndex("Name");

            MongoScript result;
            if (!DataManager.Cache.Scripts.TryGetValue(name, out result))
            {
                result = GetCollection().FindOne(Query.EQ("Name", name));

                if (result != null && result.Id != ObjectId.Empty)
                    DataManager.Cache.Scripts.Enqueue(name, result);
                else
                    return null;
            }

            return result;
        }

        /// <summary>
        /// Get Blocking
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static MongoScript GetBlocking(ObjectId id)
        {
            MongoScript result = GetCollection().FindOneById(id);

            if (result != null)
                DataManager.Cache.Scripts.Enqueue(result.Name, result);
            else
                return null;

            return result;
        }

        /// <summary>
        /// Increment script counter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Int32 Increment(String name)
        {
            lock (_counters)
            {
                Int32 value = 0;
                if (!_counters.TryGetValue(name, out value))
                    _counters[name] = value;

                _counters[name]++;
                return value;
            }
        }

        /// <summary>
        /// Gets the items collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<MongoScript> GetCollection()
        {
            return DataManager.Database.GetCollection<MongoScript>("Blueprint.Scripts");
        }

        /// <summary>
        /// Clears collection
        /// </summary>
        public static void ClearCollection()
        {
            ClearCollection(true);
        }

        /// <summary>
        /// Drops collection and recreates content
        /// </summary>
        /// <param name="autoPopulate"></param>
        public static void ClearCollection(Boolean autoPopulate)
        {
            if (GetCollection().Exists())
                GetCollection().Drop();

            GetCollection().CreateIndex("Name");

            if (autoPopulate)
                PopulateCollection();
        }

        /// <summary>
        /// Populates collection with default data 
        /// TODO: load from file?
        /// </summary>
        public static void PopulateCollection()
        {
            String testCode = "function ValidateEmail(addr,man) {\nif (addr == '' && man) {\n   return false;\n}\nif (addr == '') return true;\n" +
            "var invalidChars = '\\/\\'\\ \";:?!()[]\\{\\}^|';\nfor (i=0; i<invalidChars.length; i++) {\n   if (addr.indexOf(invalidChars.charAt(i),0) > -1) {\n      return false;\n   }\n}\n"+
            "for (i=0; i<addr.length; i++) {\n   if (addr.charCodeAt(i)>127) {\n      return false;\n   }\n}\n" +
            "var atPos = addr.indexOf('@',0);\nif (atPos == -1) {\n   return false;\n}\nif (atPos == 0) {\n   return false;\n}\n" +
            "if (addr.indexOf('@', atPos + 1) > - 1) {\n  return false;\n}\nif (addr.indexOf('.', atPos) == -1) {\n   return false;\n}\n" +
            "if (addr.indexOf('@.',0) != -1) {\n   return false;\n}\nif (addr.indexOf('.@',0) != -1){\n   return false;\n}\n" +
            "if (addr.indexOf('..',0) != -1) {\n    return false;\n}\n" +
            "var suffix = addr.substring(addr.lastIndexOf('.')+1);\nif (suffix.length != 2 && suffix != 'com' && suffix != 'net' && suffix != 'org' && suffix != 'edu' && suffix != 'int' && suffix != 'mil' && suffix != 'gov' & suffix != 'arpa' && suffix != 'biz' && suffix != 'aero' && suffix != 'name' && suffix != 'coop' && suffix != 'info' && suffix != 'pro' && suffix != 'museum') {\n   return false;\n}\n" +
            "return true;\n}\n";
            MongoScript.Generate("Email Validation", testCode, typeof(Program)).Put();

            String testCode2 = "var i = 0; function Increment() { i++; }";
            MongoScript.Generate("IncrementTest", testCode2, typeof(Program)).Put();
        }

       
    }
}
