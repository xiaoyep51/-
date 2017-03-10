using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelper
{
    public class WinServiceHelper
    {
        public string ServiceName { get; set; }
        public string InstallPath { get; set; }
        public string UninstallPath { get; set; }

        public WinServiceHelper(string serviceName, string installPath = null, string uninstallPath = null)
        {
            this.ServiceName = serviceName;
            this.InstallPath = installPath;
            this.UninstallPath = uninstallPath;
        }

        public void StartService()
        {
            ServiceController serviceController = new ServiceController(ServiceName);
            if (serviceController == null)
            {
                return;
            }
            serviceController.Start();
        }

        public void StopService()
        {
            ServiceController serviceController = new ServiceController(ServiceName);
            if (serviceController == null)
            {
                return;
            }
            if (serviceController.CanStop)
            {
                serviceController.Stop();
            }
        }

        public void PauseService()
        {
            ServiceController serviceController = new ServiceController(ServiceName);
            if (serviceController == null)
            {
                return;
            }
            if (serviceController.CanPauseAndContinue)
            {
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Pause();
                }
                else if (serviceController.Status == ServiceControllerStatus.Paused)
                {
                    serviceController.Continue();
                }
            }
        }

        public bool IsServiceInstall()
        {
            ServiceController[] Services = ServiceController.GetServices();
            for (int i = 0; i < Services.Length; i++)
            {
                if (Services[i].DisplayName.ToString() == ServiceName)
                {
                    return true;
                }
            }
            return false;
        }

        public ServiceControllerStatus CheckServiceStatus()
        {
            ServiceController serviceController = new ServiceController(ServiceName);
            return serviceController.Status;
        }

        public void InstallService(Action installing, EventHandler installed)
        {
            if (string.IsNullOrWhiteSpace(InstallPath))
            {
                return;
            }

            string CurrentDirectory = System.Environment.CurrentDirectory;
            System.Environment.CurrentDirectory = CurrentDirectory;

            if (installing != null)
            {
                installing.Invoke();
            }

            Process process = new Process();
            process.StartInfo.FileName = InstallPath;
            process.EnableRaisingEvents = true;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;

            if (installed != null)
            {
                process.Exited += installed;
            }

            process.Start();
            process.WaitForExit();
            System.Environment.CurrentDirectory = CurrentDirectory;
        }

        public void UninstallService(Action installing, EventHandler installed)
        {
            if (string.IsNullOrWhiteSpace(UninstallPath))
            {
                return;
            }

            string CurrentDirectory = System.Environment.CurrentDirectory;
            System.Environment.CurrentDirectory = CurrentDirectory;

            if (installing != null)
            {
                installing.Invoke();
            }

            Process process = new Process();
            process.StartInfo.FileName = UninstallPath;
            process.EnableRaisingEvents = true;
            process.StartInfo.CreateNoWindow = false;
            process.StartInfo.UseShellExecute = true;

            if (installed != null)
            {
                process.Exited += installed;
            }

            process.Start();
            process.WaitForExit();
            System.Environment.CurrentDirectory = CurrentDirectory;
        }
    }
}
