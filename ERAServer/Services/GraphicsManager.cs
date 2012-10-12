using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ERAServer.Data;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;

namespace ERAServer.Services
{
    internal class GraphicsManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Int64 GetFileSize(String path)
        {
            FileInfo fi = new FileInfo(path);
            return fi.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DateTime GetFileTimeUtc(String path)
        {
            FileInfo fi = new FileInfo(path);
            return fi.LastWriteTimeUtc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static Boolean FileExists(String path)
        {
            return File.Exists(path) || DataManager.Database.GridFS.Exists(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        internal static void Upload(String path, Asset meta)
        {
            MongoGridFSFileInfo gridFSFile = DataManager.Database.GridFS.Upload(path);
            DataManager.Database.GridFS.SetMetadata(gridFSFile, meta.ToBson());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        internal static void Download(String path)
        {
            DataManager.Database.GridFS.Download(path);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        internal static void Download(ObjectId id)
        {
            DataManager.Database.GridFS.Download(DataManager.Database.GridFS.FindOneById(id).Name);
        }
    }
}
