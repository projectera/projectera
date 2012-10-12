using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.Threading.Tasks;
using MongoDB.Driver;
using ERAServer.Services;

namespace ERAServer.Data
{
    /// <summary>
    /// 
    /// </summary>
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(Country), typeof(District), typeof(Area), typeof(Planet))]
    public class Region
    {
        /// <summary>
        /// Regions Id
        /// </summary>
        [BsonId]
        public ObjectId Id
        {
            get;
            set;
        }

        /// <summary>
        /// Parent Region
        /// </summary>
        public ObjectId Parent
        {
            get;
            set;
        }

        /// <summary>
        /// Region Name
        /// </summary>
        [BsonRequired]
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Generates a region
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="name">Name</param>
        /// <returns>The region</returns>
        public static Region Generate(Region parent, String name)
        {
            return Generate(new Region(), parent, name);
        }

        /// <summary>
        /// Generates a region into a source
        /// </summary>
        /// <param name="source">Source Region</param>
        /// <param name="parent">Parent</param>
        /// <param name="name">Name</param>
        /// <returns>The region</returns>
        public static Region Generate(Region source, Region parent, String name)
        {
            source.Id = ObjectId.GenerateNewId();
            source.Parent = parent != null ? parent.Id : ObjectId.Empty;
            source.Name = name;

            return source;
        }

        #region Database Get/Put operations
        /// <summary>
        /// Gets a region from the db
        /// </summary>
        /// <param name="id">id of region to get</param>
        /// <returns></returns>
        public static Task<Region> Get(ObjectId id)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(id); });
        }

        /// <summary>
        /// Gets a region from the db
        /// </summary>
        /// <param name="username">name of region to get</param>
        /// <returns></returns>
        public static Task<Region> Get(String name)
        {
            return Task.Factory.StartNew(() => { return GetBlocking(name); });
        }

        /// <summary>
        /// Gets a region from the db, blocks while retrieving
        /// </summary>
        /// <param name="id">id of region to get</param>
        /// <returns></returns>
        public static Region GetBlocking(ObjectId id)
        {
            return GetCollection().FindOneById(id) as Region;
        }

        /// <summary>
        /// Gets a regions from the db, blocks while retrieving
        /// </summary>
        /// <param name="username">name of region to get</param>
        /// <returns></returns>
        public static Region GetBlocking(String name)
        {
            return GetCollection().FindOneAs<Region>(Query.Matches("Name", new BsonRegularExpression("^(?i)" + name + "$"))) as Region;
        }

        /// <summary>
        /// Gets the currency collection
        /// </summary>
        /// <returns></returns>
        public static MongoCollection<Region> GetCollection()
        {
            return DataManager.Database.GetCollection<Region>("Regions");
        }

        /// <summary>
        /// Puts a region to the db
        /// </summary>
        public void Put()
        {
            Put(SafeMode.False);
        }

        /// <summary>
        /// Puts a region to the db
        /// <param name="safemode">Sets the safemode on this query</param>
        /// </summary>
        public SafeModeResult Put(SafeMode safemode)
        {
            return GetCollection().Save<Region>(this, safemode);
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class Planet : Region
    {
        /// <summary>
        /// Planet's Mass
        /// </summary>
        [BsonRequired]
        public Double Mass
        {
            get;
            set;
        }

        /// <summary>
        /// Planet's Radius
        /// </summary>
        [BsonRequired]
        public Double Radius
        {
            get;
            set;
        }

        /// <summary>
        /// Planet's Distance to the Sun
        /// </summary>
        [BsonRequired]
        public Double Distance
        {
            get;
            set;
        }

        /// <summary>
        /// Generates a new Planet
        /// </summary>
        /// <param name="name">Name of the planet</param>
        /// <returns>The new planet</returns>
        public static Planet Generate(String name, Double mass, Double radius, Double distance)
        {
            Planet result = new Planet();
            result.Mass = mass;
            result.Radius = radius;
            result.Distance = distance;

            return (Planet)Region.Generate(result, null, name);

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Country : Region
    {

        // Area Capital
        // ObjectId Capital??

        // Currency currency { get; set; } Enum?
        // ObjectId Currency??

        /// <summary>
        /// Country Area Size
        /// </summary>
        [BsonRequired]
        public Int32 Size
        {
            get;
            set;
        }

        /*public Int32 Population
        {
            get;
            set;
        }*/

        /// <summary>
        /// Generates a new Country
        /// </summary>
        /// <param name="parent">Planet of the country</param>
        /// <param name="name">Name of the country</param>
        /// <returns>The new country</returns>
        public static Country Generate(Planet parent, String name, Int32 size)
        {
            Country result = new Country();
            result.Size = size;

            return (Country)Region.Generate(result, parent, name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class District : Region
    {
        // Climate climate { get; set; } Enum ??
        //  

        /// <summary>
        /// Generates a new District
        /// </summary>
        /// <param name="parent">Country in which the district is located</param>
        /// <param name="name">Name of the district</param>
        /// <returns>The new district</returns>
        internal static District Generate(Country parent, String name)
        {
            return (District)Region.Generate(new District(), parent, name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Area : Region
    {
        // RegionType enum

        /// <summary>
        /// Generates a new Area
        /// </summary>
        /// <param name="parent">District in which the area is located</param>
        /// <param name="name">Name of the area</param>
        /// <returns>The new area</returns>
        internal static Area Generate(District parent, String name)
        {
            return (Area)Region.Generate(new Area(), parent, name);
        }
    }
}
