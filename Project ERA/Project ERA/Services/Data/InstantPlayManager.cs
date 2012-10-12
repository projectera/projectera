using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ProjectERA.Services.Data.Storage;
using System.IO;

namespace ProjectERA.Services.Data
{
    internal static class InstantPlayManager
    {
        /// <summary>
        /// 
        /// </summary>
        private const String FileName = ".instantplay";

        /// <summary>
        /// Saves credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        internal static void Save(Game game, String username, String password)
        {
            FileManager fileManager = game.Services.GetService(typeof(FileManager)) as FileManager;
            IStorageDevice psd = fileManager.GetStorageDevice(FileLocationContainer.Player);
            while (psd.IsReady == false)
            {
                System.Threading.Thread.Sleep(10);
                System.Threading.Thread.MemoryBarrier();
            }

            fileManager.GetStorageDevice(FileLocationContainer.Player).Save(".", FileName, (stream) =>
            {
                StreamWriter sw = new StreamWriter(stream);
                sw.WriteLine(username);
                sw.WriteLine(password);
                sw.Flush();
                sw.Close();
            });
        }

        /// <summary>
        /// Loads credentials
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        internal static Boolean Load(Game game, out String username, out String password)
        {
            FileManager fileManager = game.Services.GetService(typeof(FileManager)) as FileManager;
            IStorageDevice psd = fileManager.GetStorageDevice(FileLocationContainer.Player);
            while (psd.IsReady == false)
            {
                System.Threading.Thread.Sleep(10);
                System.Threading.Thread.MemoryBarrier();
            }

            String _username = String.Empty;
            String _password = String.Empty;

            try
            {
                fileManager.GetStorageDevice(FileLocationContainer.Player).Load(".", FileName, (stream) =>
                {
                    StreamReader sr = new StreamReader(stream);
                    _username = sr.ReadLine();
                    _password = sr.ReadLine();
                    sr.Close();
                });

                username = _username;
                password = _password;
                return true;
            }
            catch (Exception)
            {
                username = null;
                password = null;
                return false;
            }
        }

        /// <summary>
        /// Checks if there's an instant session
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        internal static Boolean Exists(Game game)
        {
            FileManager fileManager = game.Services.GetService(typeof(FileManager)) as FileManager;
            IStorageDevice psd = fileManager.GetStorageDevice(FileLocationContainer.Player);
            while (psd.IsReady == false)
            {
                System.Threading.Thread.Sleep(10);
                System.Threading.Thread.MemoryBarrier();
            }

            return fileManager.GetStorageDevice(FileLocationContainer.Player).FileExists(".", FileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        internal static void Delete(Game game)
        {
            FileManager fileManager = game.Services.GetService(typeof(FileManager)) as FileManager;
            IStorageDevice psd = fileManager.GetStorageDevice(FileLocationContainer.Player);
            while (psd.IsReady == false)
            {
                System.Threading.Thread.Sleep(10);
                System.Threading.Thread.MemoryBarrier();
            }

            fileManager.GetStorageDevice(FileLocationContainer.Player).Delete(".", FileName);
        }
    }
}
