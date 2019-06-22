using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public class CommandLineService
    {
        private readonly SetupServices setupServices;
        private WebDeployHelper webDeployHelper = new WebDeployHelper();
        public CommandLineService()
        {
            this.setupServices = new SetupServices(new SystemFileHelper(),
                new WebDeployHelper(), new SetupRegistry(), new PackageResolver());
            setupServices.onStateChanged += SetupServices_onStateChanged;
            setupServices.OnComleted += SetupServices_OnComleted;
        }
        public async Task<bool> CommandLineInstall(string jsonConfig)
        {
            var result = SimpleJSON.JSON.Parse(jsonConfig);

            var idpSetup = new IDPSetup
            {
                DomainName = result["DomainName"].Value,
                IDPFolderName = result["IDPSiteName"].Value,
                IDPPath = result["IDPSitePath"].Value,
                AdminPath = result["AdminSitePath"].Value,
                AdminFolderName = result["AdminSiteName"].Value?.ToString().ToLower(),
                IDPAddress = result["IDPServerAddress"],
                DatabaseName = result["DbName"].Value,
                Password = result["DbPassword"].Value,
                Servername = result["DbServername"].Value,
                Username = result["DbUsername"].Value,
                CatalogName = result["DbCatalogName"].Value,

            };

            if (InstallParamaetersValidation(idpSetup))
            {
                Console.WriteLine("Installation Start");
                try
                {
                    setupServices.Install(idpSetup, CommandTypeEnum.Cli);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " / " + ex.StackTrace);
                    return await Task.FromResult(false);
                }
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        public async Task<bool> CommandLineUpdate(string jsonConfig)
        {
            var result = SimpleJSON.JSON.Parse(jsonConfig);

            var idpSetup = new IDPSetup
            {
                DomainName = result["DomainName"].Value,
                IDPFolderName = result["IDPSiteName"].Value,
                IDPPath = result["IDPSitePath"].Value,
                AdminPath = result["AdminSitePath"].Value,
                AdminFolderName = result["AdminSiteName"].Value?.ToString().ToLower(),
                IDPAddress = result["IDPServerAddress"],
                DatabaseName = result["DbName"].Value,
                Password = result["DbPassword"].Value,
                Servername = result["DbServername"].Value,
                Username = result["DbUsername"].Value,
                CatalogName = result["DbCatalogName"].Value,

            };

            if (ParamaetersValidation(idpSetup))
            {
                Console.WriteLine("Upgrading start...");
                try
                {
                    setupServices.Update(idpSetup, Guid.Empty, CommandTypeEnum.Cli);
                    return await Task.FromResult(true);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message + " / " + ex.StackTrace);
                    return await Task.FromResult(false);
                }

            }

            return await Task.FromResult(false);
        }

        public async Task<bool> CommandLineDelete(string jsonConfig)
        {
            var result = SimpleJSON.JSON.Parse(jsonConfig);

            var idpSetup = new IDPSetup
            {
                DomainName = result["DomainName"].Value,
                IDPFolderName = result["IDPSiteName"].Value,
                IDPPath = result["IDPSitePath"].Value,
                AdminPath = result["AdminSitePath"].Value,
                AdminFolderName = result["AdminSiteName"].Value?.ToString().ToLower(),
                IDPAddress = result["IDPServerAddress"],
                DatabaseName = result["DbName"].Value,
                Password = result["DbPassword"].Value,
                Servername = result["DbServername"].Value,
                Username = result["DbUsername"].Value,
                CatalogName = result["DbCatalogName"].Value,

            };

            if (ParamaetersValidation(idpSetup))
            {
                Console.WriteLine("Deleting Start");
                try
                {
                    setupServices.Delete(idpSetup, Guid.Empty, CommandTypeEnum.Cli);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " / " + ex.StackTrace);
                    return await Task.FromResult(false);
                }
                return await Task.FromResult(true);
            }

            return await Task.FromResult(false);
        }

        private void SetupServices_OnComleted(string message)
        {
            Console.WriteLine(message);
        }

        private void SetupServices_onStateChanged(string message)
        {
            Console.WriteLine(message);
        }

        public bool InstallParamaetersValidation(IDPSetup iDPSetup)
        {
            var result = true;
            result = ParamaetersValidation(iDPSetup);

            if (!string.IsNullOrEmpty(iDPSetup.DomainName))
            {

                if (webDeployHelper.ExistVirtualDirectoryV2(iDPSetup.DomainName, iDPSetup.IDPFolderName))
                {
                    result = false;
                    Console.Error.WriteLine("IDPFolderName directory already exists !!!");
                }

                if (webDeployHelper.ExistVirtualDirectoryV2(iDPSetup.DomainName, iDPSetup.AdminFolderName))
                {
                    result = false;
                    Console.Error.WriteLine("AdminFolderName directory already exists !!!");
                }

            }

            if (Directory.Exists(GetRealFileAddress(iDPSetup.IDPPath, iDPSetup.IDPFolderName)))
            {
                result = false;
                Console.WriteLine("IDPFolderName folder already exists !!!");
            }

            if (Directory.Exists(GetRealFileAddress(iDPSetup.AdminPath, iDPSetup.AdminFolderName)))
            {
                result = false;
                Console.Error.WriteLine("AdminFolderName folder already exists !!!");
            }
            return result;
        }

        public bool ParamaetersValidation(IDPSetup iDPSetup)
        {
            var result = true;
            if (string.IsNullOrEmpty(iDPSetup.DomainName))
            {
                result = false;
                Console.Error.WriteLine("DomainName is require !!!");
            }
            if (string.IsNullOrEmpty(iDPSetup.IDPAddress))
            {
                result = false;
                Console.Error.WriteLine("IDPAddress is require !!!");
            }
            if (string.IsNullOrEmpty(iDPSetup.IDPPath))
            {
                result = false;
                Console.Error.WriteLine("IDPPath is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.AdminFolderName))
            {
                result = false;
                Console.Error.WriteLine("AdminFolderName is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.AdminPath))
            {
                result = false;
                Console.Error.WriteLine("AdminPath is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.IDPAddress))
            {
                result = false;
                Console.Error.WriteLine("IDPAddress is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.CatalogName))
            {
                result = false;
                Console.Error.WriteLine("نام کاتالوگ حتما باید انتخاب شود !");
            }

            if (string.IsNullOrEmpty(iDPSetup.Username))
            {
                result = false;
                Console.Error.WriteLine("نام کاربری خالی است");
            }

            if (string.IsNullOrEmpty(iDPSetup.Password))
            {
                result = false;
                Console.Error.WriteLine("کلمه عبور خالی است");
            }

            if (string.IsNullOrEmpty(iDPSetup.DatabaseName))
            {
                result = false;
                Console.Error.WriteLine("نام پایگاه داده خالی است");
            }

            if (string.IsNullOrEmpty(iDPSetup.Servername))
            {
                result = false;
                Console.Error.WriteLine("آدرس سرور پایگاه داده خالی است");
            }

            if (iDPSetup.IDPFolderName == iDPSetup.AdminFolderName)
            {
                result = false;
                Console.Error.WriteLine("IDPFolderName could not equal to AdminFolderName !!!");
            }

            return result;
        }

        private string GetRealFileAddress(string path, string name)
        {
            return string.Format("{0}\\{1}", path, name);
        }


    }
}
