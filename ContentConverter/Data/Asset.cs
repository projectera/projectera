using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Driver.Builders;
using System.IO;
using ERAUtils.Enum;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ContentConverter.Data
{
    internal class Asset : ERAServer.Data.Asset
    {
        /// <summary>
        /// Get file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        internal static Asset GetFile(AssetType type, String fileName)
        {
           Asset result = new Asset();

            MongoGridFS gridFs = new MongoGridFS(ERAServer.Services.DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(type), SafeMode.True));
            MongoGridFSFileInfo file = gridFs.FindOne(fileName) ?? gridFs.FindOne(Query.EQ("aliases", fileName));

            if (file == null || !file.Exists)
                return null;

            result.Id = file.Id.AsObjectId;
            result.RemoteFileName = file.Name;
            result.Type = type;
            result.Aliases = file.Aliases;
            result.ServerMD5 = file.MD5;

            return result;
        }

        /// <summary>
        /// Save file
        /// </summary>
        /// <param name="LocalImage">local file path</param>
        internal void SaveFile(String fileName, String previousName)
        {
            MongoGridFS gridFs = new MongoGridFS(ERAServer.Services.DataManager.Database, new MongoGridFSSettings(MongoGridFSSettings.Defaults.ChunkSize, AssetPath.Get(this.Type), SafeMode.True));

            String[] queryable = QueryableByArray;
            String name = RemoteFileName.EndsWith(".png") ? RemoteFileName.Remove(RemoteFileName.LastIndexOf('.')) : RemoteFileName;

            // Get MD5
            String md5Local = String.Empty;

            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open))
                {
                    md5Local = FileMD5(gridFs.Settings, file);
                }
                this.ServerMD5 = md5Local;
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(500);
                SaveFile(fileName, previousName);
                return;
            }

            // Find old file
            MongoGridFSFileInfo updateFile = gridFs.FindOne(previousName);

            List<String> aliases = new List<String>();
            aliases = aliases.Union(this.QueryableByArray).ToList();

            if (updateFile == null) // Loaded file!
            {
                // Filename exists
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(Query.And(Query.EQ("filename", name), Query.NE("md5", md5Local)));
                if (matchedFile != null)
                {
                    MessageBox.Show("There already exists a file with that name but different graphic. Please edit <" + matchedFile.Name + "> instead!", "Name already exists", MessageBoxButtons.OK);
                    return;
                }

                // Created aliases list
                aliases.Remove(name);
                this.Aliases = aliases.Distinct().ToArray();

                // Only update aliases
                matchedFile = gridFs.FindOne(Query.EQ("md5", md5Local));
                if (matchedFile != null)
                {
                    this.RemoteFileName = matchedFile.Name;

                    if (this.RemoteFileName != name)
                    {
                        gridFs.MoveTo(this.RemoteFileName, name);
                        aliases.Remove(name);
                        aliases.Add(this.RemoteFileName);
                        this.Aliases = aliases.Distinct().ToArray();
                    }

                    gridFs.SetAliases(matchedFile, this.Aliases);
                    return;
                }

                // Create new file
                ObjectId id = gridFs.Upload(fileName, name).Id.AsObjectId;
                MongoGridFSFileInfo fileInfo = gridFs.FindOneById(id);

                // Add the aliases
                gridFs.SetAliases(fileInfo, this.Aliases);
                return;
            }

            if (updateFile.Name == name && updateFile.MD5 == md5Local)
            {
                // Only update aliases 
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(name);
                aliases.Remove(matchedFile.Name);
                this.Aliases = aliases.Distinct().ToArray();

                gridFs.SetAliases(matchedFile, this.Aliases);
                return;
            }

            if (updateFile.MD5 == md5Local)
            {
                // only name changed
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(Query.And(Query.EQ("filename", name), Query.NE("md5", md5Local)));
                if (matchedFile != null)
                {
                    MessageBox.Show("There already exists a file with that name but different graphic. Please edit <" + matchedFile.Name + "> instead!", "Name already exists", MessageBoxButtons.OK);
                    return;
                }

                gridFs.MoveTo(previousName, name);
                aliases.Remove(name);
                aliases.Add(previousName);
                this.Aliases = aliases.Distinct().ToArray();

                gridFs.SetAliases(gridFs.FindOne(name), this.Aliases);
                return;
            }

            if (updateFile.Name == name)
            {
                // Delete previous version
                gridFs.Delete(name);

                // only graphic changed
                MongoGridFSFileInfo matchedFile = gridFs.FindOne(Query.EQ("md5", md5Local));
                if (matchedFile != null)
                {
                    aliases = aliases.Union(matchedFile.Aliases).ToList();
                    aliases.Add(matchedFile.Name);

                    // Delete all graphics that equal the new one
                    gridFs.Delete(matchedFile.Name);
                }

                aliases.Remove(name);
                this.Aliases = aliases.Distinct().ToArray();

                MongoGridFSFileInfo newFile = gridFs.Upload(fileName, name);
                gridFs.SetAliases(newFile, this.Aliases);
                return;
            }

            MongoGridFSFileInfo updatedPeekFile = gridFs.FindOne(Query.Or(Query.EQ("filename", name), Query.EQ("md5", md5Local)));
            if (updatedPeekFile != null)
            {
                MessageBox.Show("There already exists a file with that name or that graphic. Please edit <" + updatedPeekFile.Name + "> instead!", "Graphic already exists", MessageBoxButtons.OK);
                return;
            }

            gridFs.Delete(updateFile.Name);

            aliases.Remove(name);
            aliases.Add(updateFile.Name);
            this.Aliases = aliases.Distinct().ToArray();

            MongoGridFSFileInfo newPeekFile = gridFs.Upload(fileName, name);
            gridFs.SetAliases(newPeekFile, this.Aliases);

        }
    }
}