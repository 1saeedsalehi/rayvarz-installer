using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.Data.SqlClient;
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
            else
            {
                if (iDPSetup.IDPAddress.Length > 64)
                {
                    result = false;
                    Console.Error.WriteLine("IDPAddress cannot be more than 64 characters !!!");
                }
            }

            if (string.IsNullOrEmpty(iDPSetup.IDPPath))
            {
                result = false;
                Console.Error.WriteLine("IDPPath is require !!!");
            }
            else
            {
                if (Path.IsPathRooted(iDPSetup.IDPPath))
                {
                    string idpPath = Path.GetPathRoot(iDPSetup.IDPPath);
                    if (!Directory.Exists(idpPath))
                    {
                        result = false;
                        Console.Error.WriteLine("IDPPath does not exist !!!");
                    }
                }
                else
                {
                    result = false;
                    Console.Error.WriteLine("IDPPath is not valid !!!");
                }
            }


            if (string.IsNullOrEmpty(iDPSetup.AdminFolderName))
            {
                result = false;
                Console.Error.WriteLine("AdminFolderName is require !!!");
            }
            else {
                if (iDPSetup.AdminFolderName.Length > 64)
                {
                    result = false;
                    Console.Error.WriteLine("AdminFolderName cannot be more than 64 characters !!!");
                }
            }

            if (string.IsNullOrEmpty(iDPSetup.AdminPath))
            {
                result = false;
                Console.Error.WriteLine("AdminPath is require !!!");
            }
            else
            {
                if (Path.IsPathRooted(iDPSetup.AdminPath))
                {
                    string adminPath = Path.GetPathRoot(iDPSetup.AdminPath);
                    if (!Directory.Exists(adminPath))
                    {
                        result = false;
                        Console.Error.WriteLine("AdminPath does not exist !!!");
                    }
                }
                else
                {
                    result = false;
                    Console.Error.WriteLine("AdminPath is not valid !!!");
                }
            }

            if (string.IsNullOrEmpty(iDPSetup.IDPAddress))
            {
                result = false;
                Console.Error.WriteLine("IDPAddress is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.CatalogName))
            {
                result = false;
                Console.Error.WriteLine("Catalog name is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.Username))
            {
                result = false;
                Console.Error.WriteLine("Username is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.Password))
            {
                result = false;
                Console.Error.WriteLine("Password is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.DatabaseName))
            {
                result = false;
                Console.Error.WriteLine("Database name is require !!!");
            }

            if (string.IsNullOrEmpty(iDPSetup.Servername))
            {
                result = false;
                Console.Error.WriteLine("DbServer address is require !!!");
            }

            if (iDPSetup.IDPFolderName == iDPSetup.AdminFolderName)
            {
                result = false;
                Console.Error.WriteLine("IDPFolderName could not equal to AdminFolderName !!!");
            }

            if (!ValidateConnection(iDPSetup.Servername, iDPSetup.DatabaseName, iDPSetup.Username, iDPSetup.Password))
            {
                result = false;
                Console.Error.WriteLine("Database connection string error !!!");
            }

            return result;
        }

        public bool ValidateConnection(string DbServer, string DbName, string DbUSerName, string DbPassword)
        {
            var connectionString = $"Data Source={DbServer};Initial Catalog={DbName}; Persist Security Info=True; User ID={ DbUSerName};Password={DbPassword};";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string GetRealFileAddress(string path, string name)
        {
            return string.Format("{0}\\{1}", path, name);
        }


    }
}
