using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MongoDB.Driver;

namespace ContentConverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ERAServer.Services.DataManager.Server = MongoServer.Create("mongodb://pegu.maxmaton.nl");
            ERAServer.Services.DataManager.Database = ERAServer.Services.DataManager.Server.GetDatabase("era");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new StartupForm());
        }
    }
}
