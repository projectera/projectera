using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Threading.Tasks;
using System.Diagnostics;
using ERAServer.Services;

namespace ERAServer.Data
{
    public class Tileset
    {
        /// <summary>
        /// 
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public String AssetName
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonRequired]
        public List<String> AutotileAssetNames
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Boolean> AutotileAnimationFlags
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte[] Passages
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte[] Priorities
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte[] Flags
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Byte[] Tags
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [BsonIgnoreIfDefault]
        public Int32 Tiles
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public UInt32 Version
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public Tileset()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="assetName"></param>
        /// <param name="autotileNames"></param>
        /// <param name="autotileAnimation"></param>
        /// <param name="passages"></param>
        /// <param name="priorities"></param>
        /// <param name="flags"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static Tileset Generate(ObjectId id, String name, String assetName, List<String> autotileNames, List<Boolean> autotileAnimation,
            Byte[] passages, Byte[] priorities, Byte[] flags, Byte[] tags, Int32 tiles, UInt32 version)
        {
            Tileset result = new Tileset();
            result.Id = id;
            result.Name = name;
            result.AssetName = assetName;
            result.AutotileAssetNames = autotileNames;
            result.AutotileAnimationFlags = autotileAnimation;
            result.Passages = passages;
            result.Priorities = priorities;
            result.Flags = flags;
            result.Tags = tags;
            result.Tiles = tiles;
            result.Version = version;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        [Conditional("DEBUG")]
        public void Normalize()
        {
            Byte[] temp1 = Passages;
            Byte[] temp2 = Priorities;
            Byte[] temp3 = Flags;
            Byte[] temp4 = Tags;

            Array.Resize(ref temp2, temp1.Length);
            Array.Resize(ref temp3, temp1.Length);
            Array.Resize(ref temp4, temp1.Length);

            Priorities = temp2;
            Flags = temp3;
            Tags = temp4;

            Put();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="count"></param>
        public void Resize(Int32 count)
        {
            Byte[] temp1 = Passages;
            Byte[] temp2 = Priorities;
            Byte[] temp3 = Flags;
            Byte[] temp4 = Tags;

            Array.Resize(ref temp1, count + 384);
            Array.Resize(ref temp2, count + 384);
            Array.Resize(ref temp3, count + 384);
            Array.Resize(ref temp4, count + 384);

            Passages = temp1;
            Priorities = temp2;
            Flags = temp3;
            Tags = temp4;

            Tiles = count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        [Conditional("DEBUG")]
        public void Normalize(Int32 y)
        {
            Byte[] temp = new Byte[((y) * 256 + 384)];
            Array.Copy(Passages, temp, Math.Min(Passages.Length, temp.Length));
            this.Passages = temp;
            Normalize();
        }

        /// <summary>
        /// Gets a tileset from the db
        /// </summary>
        /// <param name="id">id of tileset to get</param>
        /// <returns></returns>
        internal static Task<Tileset> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets a tileset from the db
        /// </summary>
        /// <param name="name">name of tileset to get</param>
        /// <returns></returns>
        internal static Task<Tileset> Get(String name)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Gets a tileset from the db, blocks while retrieving
        /// </summary>
        /// <param name="id">id of tileset to get</param>
        /// <returns></returns>
        internal static Tileset GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as Tileset;
        }

        /// <summary>
        /// Gets a tileset from the db, blocks while retrieving
        /// </summary>
        /// <param name="name">name of tileset to get</param>
        /// <returns></returns>
        internal static Tileset GetBlocking(String name)
        {
            return GetCollection().FindOneAs<Tileset>(Query.Matches("Name", new BsonRegularExpression("^(?i)" + name + "$"))) as Tileset;
        }

        /// <summary>
        /// Gets the tilesets collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<Tileset> GetCollection()
        {
            return DataManager.Database.GetCollection<Tileset>("Tilesets");
        }

        /// <summary>
        /// Puts a tileset to the db
        /// </summary>
        public void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a tileset to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        internal SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Tileset>(this, safemode);
        }

        /// <summary>
        /// Compute Hashcode
        /// </summary>
        /// <returns>Hashcode</returns>
        public override Int32 GetHashCode()
        {
            Int32 code = 0;

            if (this.AutotileAssetNames != null)
                foreach (String item in this.AutotileAssetNames)
                    code = (code * 3 + item.Length) ^ item.GetHashCode();

            if (this.Passages != null)
                foreach (Byte item in this.Passages)
                    code = (code * 7 + item) ^ item.GetHashCode();

            if (this.Priorities != null)
                foreach (Byte item in this.Priorities)
                    code = (code * 19 + item) ^ item.GetHashCode();

            if (this.Flags != null)
                foreach (Byte item in this.Flags)
                    code = (code * 31 + item) ^ item.GetHashCode();

            if (this.Tags != null)
                foreach (Byte item in this.Tags)
                    code = (code * 61 + item) ^ item.GetHashCode();

            if (this.AssetName != null)
                code = this.AssetName.GetHashCode() ^ (code * 127);

            code = (Int32)(this.Version * 255) ^ code;

            return code;
        }
    }
}
