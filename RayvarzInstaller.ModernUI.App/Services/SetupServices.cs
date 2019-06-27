using System.IO;
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
        public void Update(IDPSetup setupConfig, Guid packageId , CommandTypeEnum commandType)
        {
            var realFileAddress = GetRealFileAddress(setupConfig.IDPPath, setupConfig.IDPFolderName);
            var realAdminAddress = GetRealFileAddress(setupConfig.AdminPath, setupConfig.AdminFolderName);

            //DeleteVirtualDirectories(realFileAddress, realAdminAddress);
            DeleteFiles(realFileAddress, realAdminAddress);
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke(" بروزرسانی تنظیمات مربوط به AppSettings");
            else
                onStateChanged?.Invoke("Appsetting configuration...");
            
            PopulateJsonConfiguration(setupConfig.CatalogName);
            PopulateAdminJsonConfiguration(setupConfig.CatalogName, setupConfig.IDPFolderName, setupConfig.IDPAddress);
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("بروزرسانی فایل ها");
            else
                onStateChanged?.Invoke("Update files...");
            CopyFile(realFileAddress);
            CopyAdminFile(realAdminAddress);
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("بروزرسانی تنظیمات پنل ادمین");
            else
                onStateChanged?.Invoke("Update admin panel configuration...");
            
            // populate html
            PopulateHtmlIndex(realAdminAddress, setupConfig.AdminFolderName);

            //Set Settings
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("بروزرسانی تنظیمات دیتابیس");
            else
                onStateChanged?.Invoke("Update databse configuration...");

            
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
            InstallPathInfo installPathInfo = null;
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke(" بروزرسانی تنظیمات در رجیستری");
            else
                onStateChanged?.Invoke("Update registry configuration...");
            var manifest = packageResolver.GetPackage();
            if (packageId != Guid.Empty)
            {
                installPathInfo = setupRegistry[packageId];
           }
            else {
                installPathInfo = setupRegistry.GetInstallPathByIISName(setupConfig.IDPFolderName);
            }
            if (installPathInfo != null)
            {
                setupRegistry.Remove(manifest, installPathInfo);
                SaveSetupRegistry(setupConfig, manifest);
            }
            if (commandType == CommandTypeEnum.Gui)
                OnComleted("عملیات بروزرسانی نسخه مورد نظر با موفقیت پایان پذیرفت");
            else
                onStateChanged?.Invoke("Goodluck...");

           
        }

        public void Delete(IDPSetup setupConfig, Guid packageId , CommandTypeEnum commandType)
        {
            var realFileAddress = GetRealFileAddress(setupConfig.IDPPath, setupConfig.IDPFolderName);
            var realAdminAddress = GetRealFileAddress(setupConfig.AdminPath, setupConfig.AdminFolderName);
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("حذف دایرکتوری ها");
            else
                onStateChanged?.Invoke("Remove directories...");
            StopIIS();


            DeleteVirtualDirectoriesV2(setupConfig.IDPFolderName, setupConfig.DomainName);
            DeleteVirtualDirectoriesV2(setupConfig.AdminFolderName, setupConfig.DomainName);
            DeleteApplicationPools(setupConfig.IDPFolderName, setupConfig.AdminFolderName);


            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("حذف فایل ها");
            else
                onStateChanged?.Invoke("Remove files...");
          
            DeleteFiles(realFileAddress, realAdminAddress);
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke(" ذخیره تنظیمات در رجیستری");
            else
                onStateChanged?.Invoke("Registry configuration...");
            var manifest = packageResolver.GetPackage();
            InstallPathInfo installPathInfo = null;
            if (packageId != Guid.Empty)
            {
                installPathInfo = setupRegistry[packageId];
            }
            else {
                installPathInfo = setupRegistry.GetInstallPathByIISName(setupConfig.IDPFolderName);
            }

            if (installPathInfo != null)
            {
                setupRegistry.Remove(manifest, installPathInfo);
                setupRegistry.Commit();
            }

            StartIIS();
            if (commandType == CommandTypeEnum.Gui)
                OnComleted("عملیات حذف نسخه مورد نظر با موفقیت پایان پذیرفت");
            else
                onStateChanged?.Invoke("Goodluck...");
        }

        public void Install(IDPSetup setupConfig, CommandTypeEnum commandType)
        {
            var manifest = packageResolver.GetPackage();

            var realFileAddress = GetRealFileAddress(setupConfig.IDPPath, setupConfig.IDPFolderName);
            var realAdminAddress = GetRealFileAddress(setupConfig.AdminPath, setupConfig.AdminFolderName);

            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke(" ذخیره تنظیمات مربوط به AppSettings");
            else
                onStateChanged?.Invoke("Appsetting proccessing.");

            PopulateJsonConfiguration(setupConfig.CatalogName);
            PopulateAdminJsonConfiguration(setupConfig.CatalogName, setupConfig.IDPFolderName, setupConfig.IDPAddress);


            //Install Programs
            //OnSubStateChanged?.Invoke("نصب نرم افزارهای جانبی");
            //CheckDotNetCoreInstalled();//InstallApplications();

            // copying files
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("کپی فایل ها");
            else
                onStateChanged?.Invoke("Copy files...");

            CopyFile(realFileAddress);
            CopyAdminFile(realAdminAddress);

            // populate html
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("ثبت تنظیمات پنل ادمین");
            else
                onStateChanged?.Invoke("Admin configuration...");
            PopulateHtmlIndex(realAdminAddress, setupConfig.AdminFolderName);


            //Set Settings
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("ایجاد تنظیمات دیتابیس");
            else
                onStateChanged?.Invoke("Databse configuration...");
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
            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke("ساخت دایرکتوری ها");
            else
                onStateChanged?.Invoke("Virtualdirectory configuration");
            // create application pool and app in iis
            var virtualDir = MakeVirtualDirectoryV2(setupConfig.DomainName, setupConfig.IDPFolderName, realFileAddress);
            // MakeWebSite(setupConfig.AdminFolderName, realAdminAddress, setupConfig.AdminPort);
            var adminVirtualDir = MakeVirtualDirectoryV2(setupConfig.DomainName, setupConfig.AdminFolderName, realAdminAddress);

            if (commandType == CommandTypeEnum.Gui)
                onStateChanged?.Invoke(" ذخیره تنظیمات در رجیستری");
            else
                onStateChanged?.Invoke("Registry configuration");

            SaveSetupRegistry(setupConfig, manifest);
            //SaveAdminSetupRegistry(setupConfig.AdminFolderName, setupConfig.AdminPath, realAdminAddress);

            StartIIS();

            if (commandType == CommandTypeEnum.Gui)
                OnComleted(" عملیات نصب نسخه مورد نظر با موفقیت پایان پذیرفت");
            else
                OnComleted("Goodluck!");
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

        private bool MakeVirtualDirectoryV2(string domainName, string iisName, string realFileAddress)
        {
            webDeployHelper.CreateDotNetCoreAppPool(iisName);
            //_container.LogHelper.Info("Start making app pool and website in IIS.");
            var result = webDeployHelper.CreateVirtualDirectoryV2(domainName, iisName, realFileAddress);
            SetPermissions(realFileAddress);

            return result;
        }

        private void DeleteApplicationPools(string realFileAddress, string realAdminAddress)
        {
            webDeployHelper.DeleteApplicationPoolForDotnetCore(realFileAddress);
            webDeployHelper.DeleteApplicationPoolForDotnetCore(realAdminAddress);
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
                IISName = idpSetup.IDPFolderName
            };

            setupPath.Meta.Add("DomainName", idpSetup.DomainName);
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

        private void DeleteVirtualDirectories(string realFileAddress, string realAdminAddress)
        {

            webDeployHelper.RemoveVirtualDirectory(realFileAddress);
            webDeployHelper.RemoveVirtualDirectory(realAdminAddress);
        }

        private void DeleteVirtualDirectoriesV2(string appName, string domainName)
        {
            webDeployHelper.DeleteVirtualDirectoryV2(domainName, appName);
        }

        private void DeleteFiles(string realFileAddress, string realAdminAddress)
        {
            systemFileHelper.Delete(realFileAddress);
            systemFileHelper.Delete(realAdminAddress);
        }
    }


}
