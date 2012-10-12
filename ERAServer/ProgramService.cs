using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using ERAUtils.Logger;
using ERAServer.Properties;
using ERAServer.Services.Listeners;
using ERAServer.Services;

namespace ERAServer
{
    public class ProgramService : ServiceBase
    {
        static Servers server;
        static Clients clients;
        static Registration registration;

        /// <summary>
        /// 
        /// </summary>
        public ProgramService()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void TempStart(String[] args)
        {
            OnStart(args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            // Create Servers
            Logger.Initialize(Severity.Verbose, Severity.Debug);
            ERAUtils.Environment.MachineName = Settings.Default.ServerName;

            String[] ab = Generators.LanguageConfluxer.Run("Generators/Celtic-f.txt", 50);
            String[] ac = Generators.LanguageConfluxer.Run("Generators/Celtic-m.txt", 50);
            Logger.Info(ab);
            Logger.Info(ac);
            String[] ad = Generators.Werd.Run("Generators/Geordi.txt", 1);
            Logger.Info(ad);

            DataManager.Initialize();
            MapManager.Initialize();

            server = new Servers();
            clients = new Clients();
            registration = new Registration();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            base.OnStop();

            Logger.Notice("Will now stop running server.");
            if (server != null)
                server.IsRunning = false;

            if (clients != null)
                clients.IsRunning = false;

            if (registration != null)
                registration.IsRunning = false;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();

            Logger.Notice("Will now stop running server.");
            server.IsRunning = false;
            clients.IsRunning = false;
            registration.IsRunning = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="powerStatus"></param>
        /// <returns></returns>
        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            switch (powerStatus)
            {
                case PowerBroadcastStatus.ResumeSuspend:
                case PowerBroadcastStatus.ResumeAutomatic:
                    OnStart(null);
                    break;

                case PowerBroadcastStatus.QuerySuspend:
                case PowerBroadcastStatus.Suspend:
                    OnStop();
                    break;
            }

            return base.OnPowerEvent(powerStatus);
        }

        /// <summary>
        /// 
        /// </summary>
        public Int32 ConnectionCount 
        { 
            get 
            {
                if (server == null)
                    return 0;
                return server.ConnectionCount; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // ProgramService
            // 
            this.CanShutdown = true;
            this.CanHandlePowerEvent = true;
            
            this.ServiceName = "ERA Service";

        }
    }
}
