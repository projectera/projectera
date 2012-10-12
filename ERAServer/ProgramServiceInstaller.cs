using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Install;
using System.ServiceProcess;
using System.ComponentModel;

namespace ERAServer
{
    [RunInstaller(true)]
    public class ProgramServiceInstaller : Installer
    {
        public ProgramServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            processInstaller.Password = null;
            processInstaller.Username = null;
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "ERA Server";
            serviceInstaller.DisplayName = "Epos of Realms and Alliances Server";
            serviceInstaller.Description = "Daemon of the ERA Server";

            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}
