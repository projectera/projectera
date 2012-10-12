using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver.Builders;

namespace ContentConverter.Data
{
    internal static class Region
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        internal static String[] GetList(String type, String search)
        {
            return ERAServer.Data.Region.GetCollection().FindAs<ERAServer.Data.Region>(
                Query.And(Query.EQ("_t", type), Query.Matches("Name", ".*" + search + ".*")))
                .Select(a => a.Name)
                .OrderBy(a => a)
                .ToArray();
        }
    }
}
