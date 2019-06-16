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
        }
        public async Task<bool> CommandLineInstall(string jsonConfig)
        {
            var result = SimpleJSON.JSON.Parse(jsonConfig);

            var idpSetup = new IDPSetup
            {
                IDPFolderName = result["IDPSiteName"].Value,
                IDPPath = result["IDPSitePath"].Value,
                AdminPath = result["AdminSitePath"].Value,
                AdminFolderName = result["AdminSiteName"].Value?.ToString().ToLower(),
                IDPAddress = result["IDPServerAddress"],
                DatabaseName = result["DbName"].Value,
                Password = result["DbPassword"].Value,
                Servername = result["DbServername"].Value,
                Username = result["DbUsername"].Value,
                CatalogName = result["DbCatalog"].Value,

            };

            if (InstallParamaetersValidation(idpSetup))
            {
                try
                {
                    setupServices.Install(idpSetup);
                    return await Task.FromResult(true);
                }
                catch (Exception ex)
                {
                    return await Task.FromResult(false);
                }
            }

            return await Task.FromResult(false);
        }

        public bool InstallParamaetersValidation(IDPSetup iDPSetup)
        {
            var result = true;
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

            if (iDPSetup.IDPFolderName == iDPSetup.AdminFolderName)
            {
                result = false;
                Console.Error.WriteLine("IDPFolderName could not equal to AdminFolderName !!!");
            }

            if (webDeployHelper.VirtualDirectoryExists(iDPSetup.IDPFolderName))
            {
                result = false;
                Console.Error.WriteLine("IDPFolderName directory already exists !!!");
            }

            if (webDeployHelper.VirtualDirectoryExists(iDPSetup.AdminFolderName))
            {
                result = false;
                Console.Error.WriteLine("AdminFolderName directory already exists !!!");
            }

            if (Directory.Exists(GetRealFileAddress(iDPSetup.IDPPath, iDPSetup.IDPFolderName)))
            {
                result = false;
                Console.Error.WriteLine("IDPFolderName folder already exists !!!");
            }

            if (Directory.Exists(GetRealFileAddress(iDPSetup.AdminPath, iDPSetup.AdminFolderName)))
            {
                result = false;
                Console.Error.WriteLine("AdminFolderName folder already exists !!!");
            }
            return result;
        }

        private string GetRealFileAddress(string path, string name)
        {

            return string.Format("{0}\\{1}", path, name);
        }


    }
}
