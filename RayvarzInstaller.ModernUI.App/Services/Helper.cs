using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public class Helper : IHelper
    {
        public const string ExcelAppId = "{00020812-0000-0000-C000-000000000046}";
        public const string WordAppId = "{00020906-0000-0000-C000-000000000046}";
        public const string AccessAppId = "{73A4C9C1-D68D-11D0-98BF-00A0C90DC8D9}";
        public const string NetworkService = "NT Authority\\Network Service";
        public const string CustomizeLunchAndActivationPermissionCommand = "-al {0} set \"{1}\"  permit";
        public const string CustomizeAccessPermissionCommand = "-aa {0} set \"{1}\"  permit";
        public const string SetInterActiveUserForRunApplicationCommand = "-runas {0} \"Interactive User\"";

        public void RegisterService64(string service64folder)
        {
            Path.Combine(service64folder, "64Bit", "Service64.dll");
            if (Environment.Is64BitOperatingSystem)
            {
                if (this.Enable64Bit)
                    this.Register(service64folder, "64Bit", Environment.SpecialFolder.System);
                else
                    this.Register(service64folder, "32Bit", Environment.SpecialFolder.System);
                this.Register(service64folder, "32Bit", Environment.SpecialFolder.SystemX86);
            }
            else
                this.Register(service64folder, "32Bit", Environment.SpecialFolder.System);
        }

        private void Register(string service64folder, string service64type, Environment.SpecialFolder system)
        {
            string sourceFileName = Path.Combine(service64folder, service64type, "Service64.dll");
            string folderPath = Environment.GetFolderPath(system);
            File.Copy(sourceFileName, Path.Combine(folderPath, "Service64.dll"), true);
            Process.Start(new ProcessStartInfo(Path.Combine(folderPath, "regsvr32.exe"), string.Format("\"{0}\" /s", (object)sourceFileName))
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            }).WaitForExit();
        }

        public bool Enable64Bit
        {
            get
            {
                return Convert.ToBoolean(ConfigurationManager.AppSettings[nameof(Enable64Bit)]);
            }
        }

        public void AddPermissionToOfficeInterop()
        {
            this.SetCustomizeLunchAndActivationPermission("{00020812-0000-0000-C000-000000000046}", "NT Authority\\Network Service");
            this.SetCustomizeAccessPermission("{00020812-0000-0000-C000-000000000046}", "NT Authority\\Network Service");
            this.SetCustomizeConfigurationPermission("{00020812-0000-0000-C000-000000000046}");
            this.SetCustomizeLunchAndActivationPermission("{73A4C9C1-D68D-11D0-98BF-00A0C90DC8D9}", "NT Authority\\Network Service");
            this.SetCustomizeAccessPermission("{73A4C9C1-D68D-11D0-98BF-00A0C90DC8D9}", "NT Authority\\Network Service");
            this.SetCustomizeConfigurationPermission("{73A4C9C1-D68D-11D0-98BF-00A0C90DC8D9}");
            this.SetCustomizeLunchAndActivationPermission("{00020906-0000-0000-C000-000000000046}", "NT Authority\\Network Service");
            this.SetCustomizeAccessPermission("{00020906-0000-0000-C000-000000000046}", "NT Authority\\Network Service");
            this.SetCustomizeConfigurationPermission("{00020906-0000-0000-C000-000000000046}");
        }

        private void SetCustomizeAccessPermission(string appId, string userName)
        {
            this.RunCommandWithDCOMPerm(string.Format("-aa {0} set \"{1}\"  permit", (object)appId, (object)userName));
        }

        private void SetCustomizeLunchAndActivationPermission(string appId, string userName)
        {
            this.RunCommandWithDCOMPerm(string.Format("-al {0} set \"{1}\"  permit", (object)appId, (object)userName));
        }

        private void SetCustomizeConfigurationPermission(string appId)
        {
            this.RunCommandWithDCOMPerm(string.Format("-runas {0} \"Interactive User\"", (object)appId));
        }

        private void RunCommandWithDCOMPerm(string command)
        {
            Process.Start(new ProcessStartInfo("DCOMPerm", " " + command)
            {
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();
        }
    }
}
