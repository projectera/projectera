using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ERAServer.Data;
using ERAServer.Data.AI;
using ERAServer.Properties;
using ERAServer.Services;
using ERAServer.Services.Listeners;
using ERAUtils.Enum;
using ERAUtils.Logger;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using System.IO;

namespace ERAServer
{
    /// <summary>
    /// Main entrypoint for this program
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entrypoint
        /// </summary>
        /// <param name="args">Running arguments</param>
        public static void Main(String[] args)
        {
#if SERVICE
            var service = new ProgramService();
            //service.TempStart(args);
            ServiceBase.Run(service);
#else
            // Create Servers

            Severity consoleLog = Severity.Debug;
            Severity textLog = Severity.Verbose;
            Int32 generateAmount = 0;
            Int32 generateAmount2 = 0;


            #region Arguments
            if (args.Length > 0)
            { 
                Logger.Notice("Arguments: " + String.Join(" ", args));

                for (Int32 i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                    {
                        switch (args[i])
                        {
                            case "-console":
                                if (!Enum.TryParse<Severity>(args[i + 1], out consoleLog))
                                {
                                    Logger.Error("-console command does not recognize input " + args[i + 1]);
                                    Logger.Error("Run with -help to display valid input");
                                }
                                break;

                            case "-log":
                                if (!Enum.TryParse<Severity>(args[i + 1], out textLog))
                                {
                                    Logger.Error("-console command does not recognize input " + args[i + 1]);
                                    Logger.Error("Run with -help to display valid input");
                                }
                                break;

                            case "-generate":
                                if (!Int32.TryParse(args[i + 1], out generateAmount))
                                {
                                    Logger.Error("-generate command does not recognize input " + args[i + 1]);
                                    Logger.Error("Run with -help to display valid input");
                                }
                                if (!Int32.TryParse(args[i + 2], out generateAmount2))
                                {
                                    Logger.Error("-generate command does not recognize input " + args[i + 2]);
                                    Logger.Error("Run with -help to display valid input");
                                }
                                break;

                            case "-setup":
                                //autoSetup = true;
                                break;

#if DEBUG
                            case "-debug":
                                //autoDebug = true;
                                break;
#endif

                            case "-help":
                                Logger.Info("Valid commands are:");
                                Logger.Info("-console SEVERITY ");
                                Logger.Info("-log SEVERITY \n");
                                Logger.Info("-generate NUMBER NUMBER\n");
                                Logger.Info("-setup");
                                Logger.Info("Valid values are:");
                                Logger.Info("SEVERITY = [0-255] or None Verbose Debug Info Notice Warning Error Fatal");
                                Logger.Info("NUMBER = any positive Integer");
                                break;
                        }
                    }
                }
            }
            #endregion

            ////////////////////////
            // Logger
            ////////////////////////
            Logger.Initialize(textLog, consoleLog);
            ERAUtils.Environment.MachineName = Settings.Default.ServerName;
            Lidgren.Network.Lobby.NetLobby.KeySize = ERAServer.Properties.Settings.Default.SRP6Keysize;
            Lidgren.Network.Lobby.NetLobby.LogonManager = new LogonManager();

            ////////////////////////
            // Generate
            ////////////////////////
            if (generateAmount > 0)
            {
                String[] ab = Generators.LanguageConfluxer.Run(Path.Combine("Generators", "Celtic-f.txt"), generateAmount);
                String[] ac = Generators.LanguageConfluxer.Run(Path.Combine("Generators", "Celtic-m.txt"), generateAmount);
                //Logger.Info(ab);
                //Logger.Info(ac);

                Logger.Info(String.Join("| ", Generators.LanguageConfluxer.Prop(ab)));
                Logger.Info(String.Join("| ", Generators.LanguageConfluxer.Prop(ac)));
            }

            Logger.Info("");
            if (generateAmount2 > 0)
            {
                String[] ad = Generators.Werd.Run(Path.Combine("Generators", "Geordi.txt"), generateAmount2);
                Logger.Info("Geordi says: {0}", String.Join("\n", ad));
            }

            DataManager.Initialize();
            MapManager.Initialize();
            ScriptManager.Initialize();
            
            Servers server = new Servers();
            Clients clients = new Clients();
            Registration registration = new Registration();

            IronJS.Hosting.CSharp.Context context = new IronJS.Hosting.CSharp.Context();
            Logger.Info("IronJs says: {0}", context.Execute<Double>("var a = Math.random();"));

#endif
        while (Host.Process(server, clients, registration)) { };

        Logger.Notice("Will now stop running server.");

    #if SERVICE
            service.Stop();
    #else
            if (server != null)
                server.IsRunning = false;

            if (clients != null)
                clients.IsRunning = false;

            if (registration != null)
                registration.IsRunning = false;
    #endif
        }

       
    }
}
