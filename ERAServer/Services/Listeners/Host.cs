using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using ERAUtils.Enum;
using ERAServer.Data;
using MongoDB.Bson;
using ERAUtils.Logger;
using ERAServer.Data.AI;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ERAServer.Services.Listeners
{
    static class Host
    {
        public static Boolean Process(Servers servers, Clients clients, Registration registration)
        {
                
            //ConsoleKeyInfo k = Console.ReadKey();

            //if (k.Key.CompareTo(ConsoleKey.Escape) == 0)
            //{
            //    return false;
            //}

            String input = /*k.KeyChar +*/ Console.ReadLine();
            String[] args = input.Split(' ');
            String[] options = args.Where(a => a.StartsWith("-")).ToArray();
            String[] command = args.TakeWhile(a => a.StartsWith("-") == false).ToArray();

            Logger.Debug("Command: {1}, Options: {0}", String.Join(" ", options), String.Join(" ", command));

            switch (command[0])
            {
                case "q":
                    Create("data");
                    break;
                case "m":
                    Create("map");
                    break;
                case "create":
                    Create(command.Length > 1 ? command[1] : String.Empty);
                    break;

                case "info":
                    Info(command.Length > 1 ? command[1] : String.Empty, clients, servers, registration);
                    break;
                
                case "exit":
                case "quit":
                case "stop":
                    return false;

                case "help":
                case "/?":
                    Help(command.Length > 1 ? command[1] : String.Empty, command.Skip(2).ToArray());
                    break;

                default:
                    Logger.Info("Geordi doesn't know that command!");
                    Help(command.Length > 1 ? command[1] : String.Empty);
                    break;

            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="clients"></param>
        /// <param name="servers"></param>
        /// <param name="registration"></param>
        private static void Info(String command, Clients clients, Servers servers, Registration registration)
        {
            StringBuilder result = new StringBuilder();
            var format = "{0, -25} {1}\n";
            result.AppendLine("\nService status:");
            result.AppendFormat(format, "Client listener", clients.IsRunning ? "Alive" : "Dead");
            result.AppendFormat(format, "Server listener", servers.IsRunning ? "Alive" : "Dead");
            result.AppendFormat(format, "Registration listener", registration.IsRunning ? "Alive" : "Dead");

            result.AppendLine("\nNetwork status:");
            result.AppendFormat(format, "Hub size", servers.ConnectionCount);
            Logger.Info("\n{0}", result.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public static String Help(String command = null, String[] commands = null, Boolean subroutine = false)
        {
            StringBuilder result = new StringBuilder();
            var format = String.Empty;
            var help = String.Empty;

            switch(command) {
                case "create":
                    format = "{0, -25} {1}\n";

                    if (commands == null || commands.Length == 0)
                        commands = new[] { "" };

                    switch (commands[0])
                    {
                        case "map":
                            help = "Drops all maps and then creates a single debug map.";
                            break;
                        case "data":
                        case "debug":
                            help = "Creates some player data and all hardcoded dbitems.";
                            break;
                        case "":
                        default:
                            result.Append(Help(command, new[] { "map" }, true));
                            result.Append(Help(command, new[] { "data" }, true));
                            help = "Synonym for create data";
                            break;
                    }
                    result.AppendFormat(format, command + " " + String.Join(" ", commands), help);
                    break;
                case "stop":
                case "q":
                case "quit":
                case "exit":
                    format = "{0, -25} {1}\n";
                    result.AppendFormat(format, command, "Terminates server");
                    break;
                case null:
                case "":
                default:
                    format = "{0, -25} {1}\n";
                    result.AppendFormat(format, "stop", "Terminates server");
                    result.AppendFormat(format, "create [data|debug|map]", "Creates some database data");
                    result.AppendFormat(format, "help", "Shows this list of commands");
                    result.AppendFormat(format, "help command", "shows the help for command");
                    break;
            }

            if (!subroutine)
                Logger.Info("\n\nThese commands are available:\n{0}", result.ToString());
            return result.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public static void Create(String option)
        {
            switch (option)
            {
                case "map":
                        if (Map.GetCollection().Exists())
                            //Map.GetCollection().Drop();
                            Map.GetCollection().Remove(MongoDB.Driver.Builders.Query.EQ("_id", new ObjectId(new Byte[] { 78, 26, 78, 76, 106, 239, 98, 26, 32, 188, 2, 83 })));

                        UInt16[][][] data = GetSomeData();

                        SafeModeResult mapRes = Map.Generate(new ObjectId(new Byte[] { 78, 26, 78, 76, 106, 239, 98, 26, 32, 188, 2, 83 }),
                            new ObjectId(new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }),
                            new ObjectId(new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }),
                            "Testing Grounds", MapType.Beach, null, 65, 65, data, 1).Put(SafeMode.True);
                        Logger.Notice(mapRes.Ok ? "Map saved" : mapRes.LastErrorMessage);

                        String passages = "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 15 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 10 12 1 1 1 2 0 4 3 5 2 0 4 15 15 15 10 12 15 0 15 1 1 1 3 5 15 15 15 2 0 4 10 12 15 15 15 15 0 15 3 5 15 15 15 15 15 15 0 0 15 0 15 15 15 15 0 15 15 0 15 0 0 15 15 0 8 0 8 15 15 15 15 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 15 15 0 0 15 15 0 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 64 0 0 0 0 0 0 0 15 0 0 15 1 15 15 1 15 1 15 15 15 0 1 1 1 1 1 1 0 1 15 15 15 0 0 0 0 0 0 1 15 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 15 15 0 0 1 1 15 1 0 0 1 1 1 1 1 1 1 1 1 0 1 8 8 8 8 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 15 15 15 15 15 0 0 0 15 15 15 15 15 0 0 0 15 15 0 15 15 0 0 0 1 1 1 1 0 1 1 1 8 8 8 0 0 8 8 8";
                        String priorities = "5 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 0 0 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 5 5 5 5 5 5 5 5 4 4 4 4 4 4 4 4 3 3 3 3 3 3 3 3 2 2 2 2 2 2 2 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 1 0 1 0 0 0 2 2 1 1 1 1 1 0 1 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 2 2 1 2 0 0 2 2 1 1 0 1 0 0 1 1 1 1 1 1 1 1 1 0 1 0 0 0 0 0 0 0 2 2 2 2 2 0 0 0 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 1 0 1 1 1 0 0 0 0 0 0 0 0";
                        String flags = "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 8 8 8 8 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 8 8 8 8 8 8 8 8 8 8 8 8 8 8 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 9 1 9 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 9 8 8 8 8 8 8 8 0 8 8 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 8 0 0 0 0 0 0 8 8 8 8 0 0 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 8 0 8 8 0 8 8 8 8 0 8 0 8 0 8 8 0 0 8 8 8 0 8 0 8 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 8 0 0 0 0 0 8 8 8 0 0 0 0 0 0 8 8 0 0 0 0 0 0 8 8 0 0 0 0 0 0 8 8 0 0 0 0 0 0 8 8 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";
                        String tags = "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0";

                        Byte[][] dataConverted = new Byte[4][];
                        String[] dataParts = new String[] { passages, priorities, flags, tags };

                        for (Int32 dp = 0; dp < 4; dp++)
                        {
                            String[] partArray = dataParts[dp].Split(' ');
                            dataConverted[dp] = new Byte[partArray.Length];
                            for (Int32 j = 0; j < partArray.Length; j++)
                                dataConverted[dp][j] = Byte.Parse(partArray[j]);
                        }

                        SafeModeResult tilesetRes = Tileset.Generate(new ObjectId(new Byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 }), "Beach", "The-new-Beach",
                            new List<String> { "024-Ocean01", "025-Ocean02", "026-Ocean03", "027-Ocean04", "016-Sa_Shadow01", "duingras copy", "grasmetgras" },
                            null, dataConverted[0], dataConverted[1], dataConverted[2], dataConverted[3], 64 * 8 + 5 * 8, 1).Put(SafeMode.True);
                        Logger.Notice(tilesetRes.Ok ? "Tileset saved" : tilesetRes.LastErrorMessage);

                        if (Currency.GetCollection().Exists())
                            Currency.GetCollection().Drop();

                        SafeModeResult currencyRes = Currency.Generate("Dubloon", "DUB", "1 dubloon", "%i dubloons", "1 dubly", "%i dublies").Put(SafeMode.True);
                        Logger.Notice(currencyRes.Ok ? "Currency saved" : currencyRes.LastErrorMessage);

                        if (Region.GetCollection().Exists())
                            Region.GetCollection().Drop();

                        Planet alvaRes = Planet.Generate("Alva", 5.9736E24, 6371, 149598261);
                        Country lewaRes = Country.Generate(alvaRes, "Lewa", 674843);

                        District dcRes = District.Generate(lewaRes, "Dublith County");
                        District hwRes = District.Generate(lewaRes, "Hawkswood");
                        District vvRes = District.Generate(lewaRes, "Viridia Vale");

                        alvaRes.Put();
                        lewaRes.Put();
                        dcRes.Put();
                        hwRes.Put();
                        vvRes.Put();
                    break;

                case "data":
                case "debug":
                default:

                    Logger.Info("Dropping data");

                    #region Dropping and Pools

                    if (Player.GetCollection().Exists())
                        Player.GetCollection().Drop();

                    if (Interactable.GetCollection().Exists())
                        Interactable.GetCollection().Drop();

                    if (Guild.GetCollection().Exists())
                        Guild.GetCollection().Drop();

                    if (ItemBag.GetCollection().Exists())
                        ItemBag.GetCollection().Drop();

                    if (Dialogue.GetCollection().Exists())
                        Dialogue.GetCollection().Drop();

                    if (DialogueMessage.IAttachment.GetCollection().Exists())
                        DialogueMessage.IAttachment.GetCollection().Drop();

                    if (DataManager.Database.GetCollection("Counters").Exists())
                        DataManager.Database.GetCollection("Counters").Drop();

                    DataManager.Database.GetCollection("Counters").Insert(
                            new BsonDocument(
                                new BsonElement("Type", "Items"),
                                new BsonElement("Count", 0)));

                    DataManager.Database.GetCollection("Counters").Insert(
                            new BsonDocument(
                                new BsonElement("Type", "Equipment"),
                                new BsonElement("Count", 0)));

                    Data.Blueprint.Skill.ClearCollection();
                    Data.Blueprint.Achievement.ClearCollection();
                    Data.Blueprint.BattlerAnimation.ClearCollection();
                    Data.Blueprint.BattlerClass.ClearCollection(); // should be after skill
                    Data.Blueprint.BattlerModifier.ClearCollection();
                    Data.Blueprint.BattlerRace.ClearCollection();
                    Data.Blueprint.Effort.ClearCollection();
                    Data.Blueprint.Equipment.ClearCollection(); // should be after skill
                    Data.Blueprint.Item.ClearCollection();

                    ScriptManager.ClearCollection();

                    Data.Blueprint.Skill.Get(1).ContinueWith((task) =>
                    {
                        task.Result.SetDescription(Data.Blueprint.Description.Generate(String.Join(" ", "Slash is a very basic attack where the user will try to hit his opponent by swinging the stick he is wearing. Your", Data.Blueprint.Description.Equipment.Generate(Data.Blueprint.Description.Equipment.BlockId, 1, "first weapon"), "uses this skill to attack.")));
                    });

                    #endregion

                    Logger.Info("Saving data");

                    #region Items

                    Data.Blueprint.Item item = Data.Blueprint.Item.Generate("Testitem", Data.Blueprint.Description.Empty, String.Empty, 1, ItemFlags.NoConsume);
                    Data.Blueprint.Consumable consumable = Data.Blueprint.Consumable.Generate("Testconsumable", Data.Blueprint.Description.Empty, String.Empty, 5, ItemFlags.None, 10, Data.Blueprint.BattlerConsumable.Generate(0, 0, new List<ObjectId> { ObjectId.Empty }, new List<ObjectId>()));
                    Data.Blueprint.Equipment weapon = Data.Blueprint.Equipment.Generate("Testweapon", Data.Blueprint.Description.Empty, String.Empty, 1, ItemFlags.None, EquipmentType.Weapon, EquipmentPart.ArmLeft, String.Empty, ElementType.None, Data.Blueprint.BattlerValues.Generate(1, 1, 1, 1, 1, 1, 1, 1));

                    if (item.Put(SafeMode.True).Ok)
                    {
                        Logger.Info("Created testitem");

                        if (weapon.Put(SafeMode.True).Ok)
                            Logger.Info("Created testweapon");

                        if (consumable.Put(SafeMode.True).Ok)
                            Logger.Info("Created testconsumable");
                    }

                    #endregion

                    #region Player and Avatars

                    // Put a player and one avatar
                    Player realPlayer = Player.Generate("Derk-Jan", "Password", "derk-jan@karrenbeld.info");
                    SafeModeResult sfr1 = realPlayer.Put(SafeMode.True);
                    Task<Boolean> elevation = realPlayer.Elevate(PermissionGroup.PowerUser);

                    if (sfr1.Ok)
                    {
                        // Wait for creation avatar, putting it and updating the player
                        SafeModeResult sfr4 = Interactable.GenerateAvatar(realPlayer).Put(SafeMode.True);
                        SafeModeResult sfr5 = Interactable.GenerateAvatar(realPlayer).Put(SafeMode.True);
                        SafeModeResult sfr6 = Interactable.GenerateAvatar(realPlayer).Put(SafeMode.True);
                        SafeModeResult sfr2 = realPlayer.Put(SafeMode.True);

                        if (sfr2.Ok)
                        {
                            Int32 ok = (sfr4.Ok ? 1 : 0) + (sfr5.Ok ? 1 : 0) + (sfr6.Ok ? 1 : 0);
                            Logger.Info(realPlayer.Username + " was saved with " + ok + " Avatar(s)");
                        }

                        if (elevation.Result)
                        {
                            Logger.Info(realPlayer.Username + " is now also an PowerUser");
                        }

                        Task<Boolean> mail = realPlayer.ChangeEmail("me@solarsoft.nl");
                        Task<Boolean> link = realPlayer.LinkAccount(123);

                        if (mail.Result && link.Result)
                        {
                            Logger.Info(realPlayer.Username + " mail changed and linked to forum");
                        }

                    #endregion

                        #region Bags

                        Data.ItemBag bag = ItemBag.Generate(realPlayer.AvatarIds.First(), 9);
                        if (bag.Store(InteractableItem.Generate(item)))
                            Logger.Info("Stored testitem");
                        if (bag.Store(InteractableEquipment.Generate(weapon)))
                            Logger.Info("Stored testweapon");
                        if (bag.Store(InteractableEquipment.Generate(weapon)))
                            Logger.Info("Stored another testweapon");
                        if (bag.Store(InteractableConsumable.Generate(consumable)))
                            Logger.Info("Stored consumable");

                        if (bag.Put(SafeMode.True).Ok)
                        {
                            Logger.Info("Created bag with 1 testitem and 2 testweapons and 1 consumable");
                        }

                        Task<ItemBag[]> bags = ItemBag.GetAllForInteractable(realPlayer.AvatarIds.First());

                        Logger.Info("Found " + bags.Result.Length + " bags");
                        Logger.Info("Found the consumable: " + bags.Result[0].HasItem(consumable));
                        Logger.Info("Found " + bags.Result[0].ItemCount(weapon) + " weapons");

                        #endregion

                        Player p = Player.GetBlocking("Derk-Jan");

                        Task<BlockingCollection<Interactable>> task = Task<BlockingCollection<Interactable>>.Factory.StartNew(() =>
                        {
                            BlockingCollection<Interactable> result = new BlockingCollection<Interactable>();
                            Parallel.ForEach(p.AvatarIds, (id) => result.Add(Interactable.GetBlocking(id)));
                            result.CompleteAdding();

                            return result;
                        });

                        Parallel.ForEach<Interactable>(task.Result, (A) => Logger.Info("Got Avatar " + A.Id));

                        #region Guilds
                        Interactable iout;
                        if (task.Result.TryTake(out iout))
                        {
                            Guild guild = Guild.Generate("Awesome Guild", iout);
                            SafeModeResult sfr7 = guild.Put(SafeMode.True);

                            if (sfr7.Ok)
                            {
                                Logger.Info("Guild created");
                                Task<Boolean> b = guild.ElevateMember(iout.Id, GuildPermissionGroup.Member);
                                Logger.Info("Guild founder to member: " + b.Result);

                                InteractableGuildMember member = InteractableGuildMember.Generate(realPlayer.AvatarIds.First());

                                Task<Boolean> pmem = member.Join(guild);
                                Task<Boolean> nmem = guild.AddMember(realPlayer.AvatarIds.First());
                                Task<Boolean> fmem = guild.Founder.Leave(guild);

                                if (pmem.Result)
                                    Logger.Info("A new member is now pending");

                                if (nmem.Result)
                                    Logger.Info("A new member is now member");

                                if (fmem.Result)
                                    Logger.Info("Founder is no longer a member");

                                if (guild.ElevateMember(member, GuildPermissionGroup.Member).Result)
                                    Logger.Info("The pending member is now a member");
                            }
                        }
                    }

                        #endregion

                    #region Dialogues
                    Dialogue welcome = Dialogue.Generate(ObjectId.Empty, realPlayer.Id, "Welcome to ERA");
                    welcome.Put();

                    welcome.AddMessage(DialogueMessage.Generate(ObjectId.Empty, Generators.Werd.Run("Generators/Geordi.txt", 1)[0]));
                    welcome.AddMessage(DialogueMessage.Generate(ObjectId.Empty, "Your position can be seen here."));
                    //welcome.Put();

                    welcome.Pages[0].Contents[0].MarkAsRead(realPlayer.Id);
                    welcome.Pages[0].Contents[2].Attach(DialogueMessage.LocationAttachment.Generate(ObjectId.Empty, Interactable.GetBlocking(realPlayer.AvatarIds.First())));
                    //welcome.Put(); // TODO make attach atomic field update

                    Dialogue[] dialogues = Dialogue.GetBlockingFor(realPlayer.Id);
                    Logger.Info("There are " + dialogues.Length + " dialogues for this player.");
                    foreach (var dialogue in dialogues)
                    {
                        Logger.Info("Dialogue " + dialogue.Id + " was created on " + dialogue.TimeStamp);
                        Logger.Info("Dialogue has " + dialogue.Participants.Count + " participants.");
                        foreach (var participant in dialogue.Participants)
                            Logger.Info(participant + " is a participant");

                        Logger.Info("Dialogue has " + dialogue.Pages.Count + " pages.");
                        foreach (var page in dialogue.Pages)
                        {
                            Logger.Info("Page " + page.Id + " has " + page.Contents.Count + " messages");
                            foreach (var message in page.Contents)
                            {
                                if (message.Read.Contains(realPlayer.Id))
                                {
                                    Logger.Info("--- STATUS READ ---");
                                }

                                Logger.Info("Message " + message.Id + "  was sent by " + message.Sender + " on " + message.TimeStamp);
                                Logger.Info("Message is [" + message.Contents + "]");

                                if (message.Attachment.HasValue)
                                {
                                    Logger.Info("Message has an attachment: " + message.Id);
                                    Logger.Info("Message attachment type: " + (DialogueMessage.IAttachment.GetBlocking(message.Attachment.GetValueOrDefault()).GetType().Name));
                                }
                            }
                        }
                    }

                    Dialogue read = Dialogue.Generate(ObjectId.Empty, realPlayer.Id, "Read dialogue");
                    read.Put();
                    read.Pages[0].Contents[0].MarkAsRead(realPlayer.Id);
                    read.ArchiveDialogue(DialogueMessage.Generate(ObjectId.Empty, "Test message"));

                    String[] messages = Generators.Werd.Run("Generators/Geordi.txt", 2500);
                    List<Task> messageTasks = new List<Task>();

                    foreach (var message in messages)
                    {
                        messageTasks.Add(Task.Factory.StartNew(() => welcome.AddMessage(ObjectId.Empty, message)));
                    }

                    Task.WaitAll(messageTasks.ToArray());

                    dialogues = Dialogue.GetBlockingFor(realPlayer.Id);
                    Logger.Info("There are " + dialogues.Length + " dialogues with unread messages for this player.");
                    foreach (var dialogue in dialogues)
                    {
                        Logger.Info("Dialogue " + dialogue.Id + " was created on " + dialogue.TimeStamp);
                        Logger.Info("Dialogue has " + dialogue.Pages.Count + " pages.");

                        if (dialogue.FollowUp.HasValue)
                            Logger.Info("Dialogue has a follow up dialogue: " + dialogue.FollowUp.GetValueOrDefault());
                    }

                    dialogues = Dialogue.GetBlockingFor(realPlayer.Id, false, false);
                    Logger.Info("There are " + dialogues.Length + " active dialogues for this player.");
                    foreach (var dialogue in dialogues)
                    {
                        Logger.Info("Dialogue " + dialogue.Id + " was created on " + dialogue.TimeStamp);
                        Logger.Info("Dialogue has " + dialogue.Pages.Count + " pages.");

                        if (dialogue.FollowUp.HasValue)
                            Logger.Info("Dialogue has a follow up dialogue: " + dialogue.FollowUp.GetValueOrDefault());
                    }

                    dialogues = Dialogue.GetBlockingFor(realPlayer.Id, true);
                    Logger.Info("There are " + dialogues.Length + " dialogues for this player.");
                    foreach (var dialogue in dialogues)
                    {
                        Logger.Info("Dialogue " + dialogue.Id + " was created on " + dialogue.TimeStamp);
                        Logger.Info("Dialogue has " + dialogue.Pages.Count + " pages.");

                        if (dialogue.FollowUp.HasValue)
                            Logger.Info("Dialogue has a follow up dialogue: " + dialogue.FollowUp.GetValueOrDefault());
                    }
                    #endregion



                    Data.Blueprint.Effort.Generate("First effort", "(i) => i.MapX = 17").Put();
                    Data.Blueprint.Effort.Generate("Another effort", "(i) => { }").Put();

                    Data.Blueprint.Achievement.Generate("Great achievement").Put();
                    Data.Blueprint.Achievement.Generate("Another achievement").Put();
                    break;
            }
        }

         /// <summary>
        /// Read testmap from file
        /// </summary>
        /// <returns></returns>
        private static UInt16[][][] GetSomeData()
        {
            UInt16[][][] data = new ushort[65][][];
            Int32 x = 0;
            Int32 y = 0;
            using (System.IO.StreamReader stream = new System.IO.StreamReader("testmap.txt"))
            {
                while (stream.EndOfStream == false)
                {
                    String line = stream.ReadLine();
                    if (Regex.Match(line, "<Item>$").Success)
                    {
                        data[x] = new UInt16[65][];
                    }
                    else if (Regex.Match(line, "<Item>([0-9]+) ([0-9]+) ([0-9]+)</Item>$").Success)
                    {
                        GroupCollection resultGroups = Regex.Match(line, "<Item>([0-9]+) ([0-9]+) ([0-9]+)").Groups;
                        data[x][y++] = new UInt16[] { UInt16.Parse(resultGroups[1].Value), UInt16.Parse(resultGroups[2].Value), UInt16.Parse(resultGroups[3].Value) };
                    }
                    else if (Regex.Match(line, "</Item>$").Success)
                    {
                        x++;
                        y = 0;
                    }
                }
            }

            return data;
        }
    }
}
