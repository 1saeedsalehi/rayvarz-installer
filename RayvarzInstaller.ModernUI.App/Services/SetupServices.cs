using System.IO;
using System.Threading.Tasks;
using Rayvarz.Systems.Properties.Connections;
using Rayvarz.Systems.Properties;
using Rayvarz.Systems;
using RayvarzInstaller.ModernUI.App.Models;
using Microsoft.Web.Administration;
using System.Security.AccessControl;
using System;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public class SetupServices
    {
        private readonly SystemFileHelper systemFileHelper;
        private readonly WebDeployHelper webDeployHelper;
        private readonly SetupRegistry setupRegistry;
        private readonly PackageResolver packageResolver;

        public event OnStateChangedDelegtae onStateChanged;
        public event OnCancelDelegate OnCanceled;
        public event OnErrorDelegate OnError;
        public event OnCompletedDelegate OnComleted;
        private Manifest _manifest;

        //public event OnSubStateChangedDelegtae OnSubStateChanged;

        public SetupServices(
            SystemFileHelper systemFileHelper,
            WebDeployHelper webDeployHelper,
            SetupRegistry setupRegistry,
            PackageResolver packageResolver)
        {
            this.systemFileHelper = systemFileHelper;
            this.webDeployHelper = webDeployHelper;
            this.setupRegistry = setupRegistry;
            this.packageResolver = packageResolver;
        }
        public void Update(IDPSetup setupConfig , Guid packageId)
        {
            var realFileAddress = GetRealFileAddress(setupConfig.IDPPath, setupConfig.IDPFolderName);
            var realAdminAddress = GetRealFileAddress(setupConfig.AdminPath, setupConfig.AdminFolderName);

            //DeleteVirtualDirectories(realFileAddress, realAdminAddress);
            DeleteFiles(realFileAddress, realAdminAddress);

            PopulateJsonConfiguration(setupConfig.CatalogName);
            PopulateAdminJsonConfiguration(setupConfig.CatalogName, setupConfig.IDPFolderName, setupConfig.IDPAddress);
            CopyFile(realFileAddress);
            CopyAdminFile(realAdminAddress);

            // populate html
            PopulateHtmlIndex(realAdminAddress, setupConfig.AdminFolderName);

            //Set Settings
            onStateChanged?.Invoke("ذخیره تنظیمات");
            //var fileAddress = GetRealFileAddress(_userControl.txtFileAddress.Text, _userControl.txtIISName.Text);
            SetSiteConnection(
                realFileAddress,
                setupConfig.DatabaseName,
                setupConfig.Password,
                setupConfig.Servername,
                setupConfig.Username,
                setupConfig.CatalogName
                );

            SetSiteConnection(
                realAdminAddress,
                setupConfig.DatabaseName,
                setupConfig.Password,
                setupConfig.Servername,
                setupConfig.Username,
                setupConfig.CatalogName
                );

            var manifest = packageResolver.GetPackage();
            var installationPath = setupRegistry[packageId];
            setupRegistry.Remove(manifest, installationPath);

            SaveSetupRegistry(setupConfig, manifest);

            OnComleted("End");

        }

        public void Delete(IDPSetup setupConfig, Guid packageId)
        {
            var realFileAddress = GetRealFileAddress(setupConfig.IDPPath, setupConfig.IDPFolderName);
            var realAdminAddress = GetRealFileAddress(setupConfig.AdminPath, setupConfig.AdminFolderName);
            StopIIS();
            DeleteVirtualDirectories(realFileAddress, realAdminAddress);
            DeleteFiles(realFileAddress, realAdminAddress);

            var manifest = packageResolver.GetPackage();
            var installationPath = setupRegistry[packageId];
            setupRegistry.Remove(manifest, installationPath);
            setupRegistry.Commit();
            StartIIS();
            OnComleted("End");

        }

        public void Install(IDPSetup setupConfig)
        {
            var manifest = packageResolver.GetPackage();

            var realFileAddress = GetRealFileAddress(setupConfig.IDPPath, setupConfig.IDPFolderName);
            var realAdminAddress = GetRealFileAddress(setupConfig.AdminPath, setupConfig.AdminFolderName);

            onStateChanged?.Invoke("ذخیره تنظیمات");

            PopulateJsonConfiguration(setupConfig.CatalogName);
            PopulateAdminJsonConfiguration(setupConfig.CatalogName, setupConfig.IDPFolderName, setupConfig.IDPAddress);


            // start install


            onStateChanged?.Invoke("نصب وب IDP");

            //Install Programs
            //OnSubStateChanged?.Invoke("نصب نرم افزارهای جانبی");
            //CheckDotNetCoreInstalled();//InstallApplications();

            // copying files
            onStateChanged?.Invoke("کپی فایل ها");
            CopyFile(realFileAddress);
            CopyAdminFile(realAdminAddress);

            // populate html
            PopulateHtmlIndex(realAdminAddress, setupConfig.AdminFolderName);


            //Set Settings
            onStateChanged?.Invoke("ذخیره تنظیمات");
            //var fileAddress = GetRealFileAddress(_userControl.txtFileAddress.Text, _userControl.txtIISName.Text);
            SetSiteConnection(
                realFileAddress,
                setupConfig.DatabaseName,
                setupConfig.Password,
                setupConfig.Servername,
                setupConfig.Username,
                setupConfig.CatalogName
                );

            SetSiteConnection(
                realAdminAddress,
                setupConfig.DatabaseName,
                setupConfig.Password,
                setupConfig.Servername,
                setupConfig.Username,
                setupConfig.CatalogName
                );


            StopIIS();

            // create application pool and app in iis
            var virtualDir = MakeVirtualDirectory(setupConfig.IDPFolderName, realFileAddress);

            // MakeWebSite(setupConfig.AdminFolderName, realAdminAddress, setupConfig.AdminPort);
            var adminVirtualDir = MakeVirtualDirectory(setupConfig.AdminFolderName, realAdminAddress);

            onStateChanged?.Invoke("ذخیره تنظیمات");

            SaveSetupRegistry(setupConfig, manifest);
            //SaveAdminSetupRegistry(setupConfig.AdminFolderName, setupConfig.AdminPath, realAdminAddress);

            StartIIS();

            OnComleted("End");

            //await Task.FromResult(true);
        }

        public void PopulateJsonConfiguration(string catalog)
        {
            var path = $"{Directory.GetCurrentDirectory()}\\Packages\\IDP\\Web\\appsettings.json";
            string json = File.ReadAllText(path);
            var parseJson = SimpleJSON.JSON.Parse(json);
            //parseJson["Catalog"] = _userControl.comboBox1.SelectedItem?.ToString() ?? "EOFFICE";
            parseJson["Catalog"] = catalog ?? "EOFFICE";

            var result = parseJson.ToString();

            File.WriteAllText(path, result);

        }

        public void PopulateAdminJsonConfiguration(string catalog, string authorityAddress, string IDPAddress)
        {
            var path = $"{Directory.GetCurrentDirectory()}\\Packages\\IDP\\admin\\appsettings.json";
            string json = File.ReadAllText(path);
            var parseJson = SimpleJSON.JSON.Parse(json);
            //parseJson["Catalog"] = _userControl.comboBox1.SelectedItem?.ToString() ?? "EOFFICE";
            parseJson["Catalog"] = catalog ?? "EOFFICE";
            parseJson["Authority"] = $"{IDPAddress}/{authorityAddress}";
            var result = parseJson.ToString();

            File.WriteAllText(path, result);

        }

        public void PopulateHtmlIndex(string realAdminAddress, string adminFolderName)
        {

            var path = $"{realAdminAddress}\\ClientApp\\dist\\index.html";

            // var path = $"{Directory.GetCurrentDirectory()}\\Packages\\IDP\\admin\\ClientApp\\dist\\index.html";
            string html = File.ReadAllText(path);

            html = html.Replace("''//baseurl", $"'/{adminFolderName}'");
            html = html.Replace("<base href=\"/\">", $"<base href=\"/{adminFolderName}/\">");

            File.WriteAllText(path, html);

        }

        private void CopyFile(string realFileAddress)
        {
            //_container.LogHelper.Info("Start copying files.");
            systemFileHelper.Copy($"{Directory.GetCurrentDirectory()}\\Packages\\IDP\\Web",
                realFileAddress, 0,
                systemFileHelper.FileCount($"{Directory.GetCurrentDirectory()}\\Packages\\IDP\\Web"),
                onStateChanged, true);

        }

        private void CopyAdminFile(string realFileAddress)
        {
            //_container.LogHelper.Info("Start copying files.");

            var adminIISAddress = $"{Directory.GetCurrentDirectory()}\\Packages\\IDP\\admin";

            systemFileHelper.Copy(adminIISAddress,
                realFileAddress, 0,
               systemFileHelper.FileCount(adminIISAddress),
                onStateChanged, true);

        }

        private void SetSiteConnection(string address, string dbName, string pwd, string server, string userId, string catalog = "")
        {
            var cp = new CnProperties
            {
                Database = dbName,
                Owner = "ray",
                Pwd = pwd,
                Server = server,
                userId = userId
            };

            address = $"{address}\\Connection";

            ConnectionsManager cmBase = new
                ConnectionsManager(GetCatalog(catalog), XmlPathType.Manual, address);
            cmBase.SetConnection(cp, cmBase.Encrypt(pwd));
        }

        private Catalog GetCatalog(string catalog)
        {
            //var result = _userControl.comboBox1.SelectedItem?.ToString();

            switch (catalog)
            {
                case "RSM":
                    return Catalog.RSM;
                case "EOFFICE":
                    return Catalog.EOFFICE;
                case "ARPG":
                    return Catalog.ARPG;
                case "BPMS":
                    return Catalog.BPMS;
                case null:
                    return Catalog.EOFFICE;
            }

            return Catalog.EOFFICE;

        }

        private Models.VirtualDirectory MakeVirtualDirectory(string iisName, string realFileAddress)
        {
            onStateChanged?.Invoke("ساخت سایت در IIS");

            //_container.LogHelper.Info("Start making app pool and website in IIS.");



            var virtualDir = webDeployHelper.CreateVirtualDirectory(
               iisName, realFileAddress, iisName);
            SetPermissions(realFileAddress);


            using (ServerManager serverManager = new ServerManager())
            {
                ApplicationPool appPool = serverManager.ApplicationPools[iisName];
                appPool.ManagedRuntimeVersion = "";
                serverManager.CommitChanges();
            }

            return virtualDir;
        }

        public void SetPermissions(string address)
        {
            FileSystemRights Rights = FileSystemRights.FullControl; ;

            bool modified;
            var none = new InheritanceFlags();
            none = InheritanceFlags.None;

            //set on dir itself
            var accessRule = new FileSystemAccessRule("IIS_IUSRS", Rights, none, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            var dInfo = new DirectoryInfo(address);
            var dSecurity = dInfo.GetAccessControl();
            dSecurity.ModifyAccessRule(AccessControlModification.Set, accessRule, out modified);

            //Always allow objects to inherit on a directory 
            var iFlags = new InheritanceFlags();
            iFlags = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;

            //Add Access rule for the inheritance
            var accessRule2 = new FileSystemAccessRule("IIS_IUSRS", Rights, iFlags, PropagationFlags.InheritOnly, AccessControlType.Allow);
            dSecurity.ModifyAccessRule(AccessControlModification.Add, accessRule2, out modified);

            dInfo.SetAccessControl(dSecurity);
        }

        private void SaveSetupRegistry(
            IDPSetup idpSetup, Manifest manifest)
        {
            var setupPath = new InstallPathInfo
            {
                PackageId = manifest.PackageId,
                PhysicalPath = idpSetup.IDPPath,
                IsWeb = true,
            };

            setupPath.Meta.Add("IDPFolderName", idpSetup.IDPFolderName);
            setupPath.Meta.Add("IDPPath", idpSetup.IDPPath);
            setupPath.Meta.Add("AdminFolderName", idpSetup.AdminFolderName);
            setupPath.Meta.Add("AdminPath", idpSetup.AdminPath);
            setupPath.Meta.Add("IDPAddress", idpSetup.IDPAddress);
            setupPath.Meta.Add("catalog", idpSetup.CatalogName);
            setupPath.Meta.Add("databasename", idpSetup.DatabaseName);
            setupPath.Meta.Add("username", idpSetup.Username);
            //setupPath.Meta.Add("password", idpSetup.Password);
            setupPath.Meta.Add("servername", idpSetup.Servername);

            //setupPath.Meta.Add("ApplicationName", virtualDir.Name);
            //setupPath.Meta.Add("ApplicationPath", virtualDir.Path);
            //setupPath.Meta.Add("ApplicationNameAdmin", adminSiteName);
            //setupPath.Meta.Add("ApplicationPathAdmin", adminPath);
            //setupPath.Meta.Add("PhysicalPathAdmin", realFileAddressadmin);
            setupRegistry.Add(manifest, setupPath);
            setupRegistry.Commit();
        }

        private void StartIIS()
        {
            webDeployHelper.StartIis();
        }

        private void StopIIS()
        {
            webDeployHelper.StopIis();
        }


        private string GetRealFileAddress(string fileAddress, string iisName)
        {
            //while (fileAddress.EndsWith("\\"))
            //{
            //    fileAddress = fileAddress.Remove(fileAddress.Length - 1);
            //}

            return string.Format("{0}{1}", fileAddress, iisName);
        }

        private void DeleteVirtualDirectories(string realFileAddress , string realAdminAddress) {
            
            webDeployHelper.RemoveVirtualDirectory(realFileAddress);
            webDeployHelper.RemoveVirtualDirectory(realAdminAddress);
        }

        private void DeleteFiles(string realFileAddress, string realAdminAddress)
        {
            systemFileHelper.Delete(realFileAddress);
            systemFileHelper.Delete(realAdminAddress);
        }
    }


}
