using Microsoft.Web.Administration;
using Microsoft.Win32;
using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Globalization;
using System.IO;
using System.Linq;


namespace RayvarzInstaller.ModernUI.App.Services
{
    public class WebDeployHelper : IWebDeployHelper
    {
        private Version _iisVersion;
        private IHelper _helper;
        private const string RootPath = "IIS://127.0.0.1/W3SVC/1/Root";

        public WebDeployHelper()
        {
            this._helper = new Helper() ;
        }

        public Version IisVersion
        {
            get
            {
                if (this._iisVersion != (Version)null)
                    return this._iisVersion;
                this._iisVersion = new Version(0, 0);
                using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\InetStp", false))
                {
                    if (registryKey != null)
                    {
                        int major = (int)registryKey.GetValue("MajorVersion", (object)-1);
                        int minor = (int)registryKey.GetValue("MinorVersion", (object)-1);
                        if (major != -1 && minor != -1)
                            this._iisVersion = new Version(major, minor);
                    }
                }
                return this._iisVersion;
            }
        }

        private void AssignApplicationToPool(string domain, string applicaitonPath, string appPoolName, int majorVersion)
        {
            if (this.GetApplicationPools(domain).Any<IisApplicationPool>((Func<IisApplicationPool, bool>)(p => p.Name == appPoolName)))
                this.ConfigApplicationPool(domain, appPoolName, "", majorVersion);
            else
                this.CreateApplicationPool(domain, appPoolName, "", majorVersion);
            using (DirectoryEntry directoryEntry = new DirectoryEntry(applicaitonPath))
            {
                if (!directoryEntry.SchemaClassName.ToLower().Contains("virtualdir"))
                    return;
                object[] objArray = new object[3]
                {
          (object) 0,
          (object) appPoolName,
          (object) true
                };
                directoryEntry.Invoke("AppCreate3", objArray);
                directoryEntry.Properties["AppIsolated"][0] = (object)"2";
            }
        }

        public bool ConfigApplicationPool(string domainName, string name, string identity, int dotNetMajorVersion)
        {
            if (this.IisVersion.Major < 7)
                return this.IisVersion.Major >= 6;
            ServerManager serverManager = new ServerManager();
            ApplicationPool applicationPool = serverManager.ApplicationPools[name];
            applicationPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
            applicationPool.Enable32BitAppOnWin64 = !this._helper.Enable64Bit;
            applicationPool.ManagedRuntimeVersion = string.Format("v{0}.0", (object)dotNetMajorVersion);
            applicationPool.ProcessModel.LoadUserProfile = true;
            applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;
            serverManager.CommitChanges();
            return true;
        }

        public bool CreateApplicationPool(string domainName, string name, string identity, int dotNetMajorVersion)
        {
            if (this.IisVersion.Major >= 7)
            {
                ServerManager serverManager = new ServerManager();
                ApplicationPool applicationPool = serverManager.ApplicationPools.Add(name);
                applicationPool.ManagedPipelineMode = ManagedPipelineMode.Integrated;
                applicationPool.Enable32BitAppOnWin64 = !this._helper.Enable64Bit;
                applicationPool.ManagedRuntimeVersion = string.Format("v{0}.0", (object)dotNetMajorVersion);
                applicationPool.ProcessModel.LoadUserProfile = true;
                applicationPool.ProcessModel.IdentityType = ProcessModelIdentityType.NetworkService;
                serverManager.CommitChanges();
                return true;
            }
            if (this.IisVersion.Major < 6)
                return false;
            using (DirectoryEntry directoryEntry = new DirectoryEntry("IIS://" + domainName + "/W3SVC/AppPools"))
                directoryEntry.Children.Add(name, "IIsApplicationPool").CommitChanges();
            return true;
        }

        

        public bool CreateWebSite(string name, string domain, string ipAddress, int port, string header, string installationPath, string defaultDocument, string applicationPool)
        {
            string path = string.Format("IIS://{0}/W3SVC", (object)domain);
            int nextAvailableSiteId = this.GetNextAvailableSiteId(domain);
            DirectoryEntry directoryEntry1 = new DirectoryEntry(path);
            string schemaClassName = directoryEntry1.SchemaClassName;
            object[] objArray = new object[1]
            {
        (object) string.Format("{0}:{1}:{2}", (object) ipAddress, (object) port, (object) header)
            };
            DirectoryEntry directoryEntry2 = directoryEntry1.Children.Add(name, schemaClassName.Replace("Service", "Server"));
            directoryEntry2.Properties["ServerComment"][0] = (object)name;
            directoryEntry2.Properties["ServerBindings"].Value = (object)objArray;
            directoryEntry2.Invoke("Put", (object)"ServerAutoStart", (object)1);
            directoryEntry2.Invoke("Put", (object)"ServerSize", (object)1);
            directoryEntry2.CommitChanges();
            DirectoryEntry directoryEntry3 = directoryEntry2.Children.Add("Root", "IIsWebVirtualDir");
            directoryEntry3.Properties["Path"][0] = (object)installationPath;
            if (!string.IsNullOrEmpty(defaultDocument))
            {
                directoryEntry3.Properties["EnableDefaultDoc"][0] = (object)true;
                directoryEntry3.Properties["DefaultDoc"].Value = (object)"default.aspx";
            }
            directoryEntry3.Properties["AppIsolated"][0] = (object)2;
            directoryEntry3.Properties["AccessRead"][0] = (object)true;
            directoryEntry3.Properties["AccessWrite"][0] = (object)false;
            directoryEntry3.Properties["AccessScript"][0] = (object)true;
            directoryEntry3.Properties["AccessFlags"].Value = (object)513;
            directoryEntry3.Properties["AppRoot"][0] = (object)("/LM/W3SVC/" + nextAvailableSiteId.ToString((IFormatProvider)CultureInfo.InvariantCulture) + "/Root");
            if (this.IisVersion.Major >= 6)
                directoryEntry3.Properties["AppPoolId"].Value = (object)applicationPool;
            directoryEntry3.Properties["AuthNTLM"][0] = (object)true;
            directoryEntry3.Properties["AuthAnonymous"][0] = (object)true;
            directoryEntry3.CommitChanges();
            return true;
        }

        public List<IisApplicationPool> GetApplicationPools(string domainName)
        {
            if (this.IisVersion.Major < 6)
                return (List<IisApplicationPool>)null;
            List<IisApplicationPool> iisApplicationPoolList = new List<IisApplicationPool>();
            using (DirectoryEntry directoryEntry = new DirectoryEntry("IIS://" + domainName + "/W3SVC/AppPools"))
            {
                directoryEntry.RefreshCache();
                foreach (DirectoryEntry child in directoryEntry.Children)
                {
                    try
                    {
                        iisApplicationPoolList.Add(new IisApplicationPool(child.Name, child.Path));
                    }
                    catch
                    {
                    }
                }
            }
            return iisApplicationPoolList;
        }

        private int GetNextAvailableSiteId(string domain)
        {
            int num = 101;
            foreach (DirectoryEntry child in new DirectoryEntry(string.Format("IIS://{0}/W3SVC", (object)domain)).Children)
            {
                try
                {
                    if (child.SchemaClassName.ToLower() == "iiswebserver")
                    {
                        int int32 = Convert.ToInt32(child.Name);
                        if (int32 >= num)
                            num = int32 + 1;
                    }
                }
                catch
                {
                }
            }
            return num;
        }

        public List<RayvarzInstaller.ModernUI.App.Models.VirtualDirectory> GetVirtualDirectories(WebSiteEntry webSite)
        {
            string path = webSite.Path + "/Root";
            var virtualDirectoryList = new List<RayvarzInstaller.ModernUI.App.Models.VirtualDirectory>();
            using (DirectoryEntry directoryEntry = new DirectoryEntry(path))
            {
                directoryEntry.RefreshCache();
                foreach (DirectoryEntry child in directoryEntry.Children)
                {
                    try
                    {
                        if (child.SchemaClassName.ToLower() == "iiswebvirtualdir")
                            virtualDirectoryList.Add(new RayvarzInstaller.ModernUI.App.Models.VirtualDirectory(child.Name, child.Path));
                    }
                    catch
                    {
                    }
                }
            }
            return virtualDirectoryList;
        }

        public List<WebSiteEntry> GetWebSites(string domainName)
        {
            string path = "IIS://" + domainName + "/W3SVC";
            List<WebSiteEntry> webSiteEntryList = new List<WebSiteEntry>();
            using (DirectoryEntry directoryEntry = new DirectoryEntry(path))
            {
                directoryEntry.RefreshCache();
                foreach (DirectoryEntry child in directoryEntry.Children)
                {
                    try
                    {
                        if (child.Properties["ServerComment"] != null && child.Properties["ServerComment"].Value != null)
                            webSiteEntryList.Add(new WebSiteEntry(child.Properties["ServerComment"].Value.ToString(), child.Path));
                    }
                    catch
                    {
                    }
                }
            }
            return webSiteEntryList;
        }

        public bool RegisterAspNet4ToIIS(OnStateChangedDelegate onStateChanged, ref int stepIndex, int stepCount)
        {
            if (onStateChanged != null)
                onStateChanged(new StateInfo("Registering Aspnet 4.0 to IIS", stepIndex, stepCount));
            string dotNetVersionPath = this.GetDotNetVersionPath(4);
            Process.Start(new ProcessStartInfo(Path.Combine(dotNetVersionPath, "aspnet_regiis"), " -iru")
            {
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();
            Process.Start(new ProcessStartInfo(Path.Combine(dotNetVersionPath, "aspnet_regiis"), " -ga \"NT Authority\\Network Service\"")
            {
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();
            return true;
        }

        public bool RegisterIisFramework(string iisApplicationPath, int majorVersion)
        {
            string dotNetVersionPath = this.GetDotNetVersionPath(majorVersion);
            if (string.IsNullOrWhiteSpace(dotNetVersionPath))
                return false;
            string str = iisApplicationPath.Substring(iisApplicationPath.ToLower().IndexOf("w3svc"));
            Process.Start(new ProcessStartInfo(Path.Combine(dotNetVersionPath, "aspnet_regiis"), string.Format("-s {0}", (object)str))
            {
                WindowStyle = ProcessWindowStyle.Hidden
            }).WaitForExit();
            return true;
        }

        public void RegisterWcf()
        {
            string str = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Microsoft.Net", !Environment.Is64BitOperatingSystem || !this._helper.Enable64Bit ? "Framework" : "Framework64", "v4.0", "Windows Communication Foundation", "ServiceModelReg.exe");
            if (!File.Exists(str))
                return;
            Process.Start(new ProcessStartInfo(str, "-r -y")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            }).WaitForExit();
        }

        public bool RemoveVirtualDirectory(string path)
        {
            bool flag = false;
            try
            {
                using (DirectoryEntry directoryEntry = new DirectoryEntry(path.Substring(0, path.LastIndexOf("/"))))
                {
                    directoryEntry.RefreshCache();
                    DirectoryEntry entry = new DirectoryEntry(path);
                    directoryEntry.Children.Remove(entry);
                    directoryEntry.CommitChanges();
                    flag = true;
                }
            }
            catch
            {
            }
            return flag;
        }

        public void StartIis()
        {
            try
            {
                Process.Start(new ProcessStartInfo("iisreset.exe", "/start")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }).WaitForExit();
            }
            catch (FileNotFoundException ex)
            {
            }
            catch (Win32Exception ex)
            {
            }
        }

        public void StopIis()
        {
            try
            {
                Process.Start(new ProcessStartInfo("iisreset.exe", "/stop")
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                }).WaitForExit();
            }
            catch (FileNotFoundException ex)
            {
            }
            catch (Win32Exception ex)
            {
            }
        }

        public bool VirtualDirectoryExists(string virtualDirectoryName)
        {
            try
            {
                using (DirectoryEntry directoryEntry = new DirectoryEntry("IIS://127.0.0.1/W3SVC/1/Root/" + virtualDirectoryName))
                {
                    if (directoryEntry.SchemaClassName.ToLower().StartsWith("iisweb"))
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public bool VirtualDirectoryExists(WebSiteEntry webSite, string virtualDirectoryName)
        {
            try
            {
                using (DirectoryEntry directoryEntry = new DirectoryEntry(webSite.Path + "/Root/" + virtualDirectoryName))
                {
                    if (directoryEntry.SchemaClassName.ToLower().StartsWith("iisweb"))
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }

        public void VsDiagUnregWcf()
        {
            string str = Path.Combine(Environment.CurrentDirectory, "source", "assemblies", "vsdiag_regwcf.exe");
            if (!File.Exists(str))
                return;
            Process.Start(new ProcessStartInfo(str, "-u")
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
            }).WaitForExit();
        }

        public bool WebDeployServiceVirtualDirectoryExists(WebSiteEntry webSite, string virtualDirectoryName)
        {
            try
            {
                using (DirectoryEntry directoryEntry = new DirectoryEntry(webSite.Path + "/Root/" + virtualDirectoryName + "/DeployWebService"))
                {
                    if (directoryEntry.SchemaClassName.ToLower().StartsWith("iisweb"))
                        return true;
                }
            }
            catch
            {
            }
            return false;
        }

        private string GetDotNetVersionPath(int majorVersion)
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\.NetFramework", false);
            if (registryKey != null)
            {
                string str1 = registryKey.GetValue("InstallRoot").ToString();
                using (IEnumerator<string> enumerator = ((IEnumerable<string>)Directory.GetDirectories(str1)).Where<string>((Func<string, bool>)(directory => directory.Substring(directory.LastIndexOf("\\") + 1).StartsWith("v" + majorVersion.ToString()))).GetEnumerator())
                {
                    if (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        string str2 = Path.Combine(str1, current);
                        if (Environment.Is64BitOperatingSystem && this._helper.Enable64Bit)
                            str2 = str2.Replace("Framework", "Framework64");
                        return str2;
                    }
                }
            }
            return string.Empty;
        }
       
        public Models.VirtualDirectory CreateVirtualDirectory
            (string name, string installationPath, string applicationPool, bool keepAppPoolAsIs = false)
        {
            return this.CreateVirtualDirectory(name, "localhost", installationPath, "Default.aspx", new WebSiteEntry(name, string.Empty), applicationPool, true , keepAppPoolAsIs);
        }

        public Models.VirtualDirectory CreateVirtualDirectory
            (string name, string domain, string installationPath, 
            string defaultDocument, WebSiteEntry website, 
            string applicationPool, bool isNested, bool keepAppPoolAsIs = false)
        {
            string str = "IIS://127.0.0.1/W3SVC/1/Root" + (isNested ? string.Empty : "/Root");
            using (DirectoryEntry directoryEntry1 = new DirectoryEntry("IIS://127.0.0.1/W3SVC/1/Root"))
            {
                directoryEntry1.RefreshCache();
                using (DirectoryEntry directoryEntry2 = directoryEntry1.Children.Add(name, "IISWebVirtualDir"))
                {
                    directoryEntry2.Properties["Path"].Insert(0, (object)installationPath);
                    directoryEntry2.CommitChanges();
                    directoryEntry1.CommitChanges();
                    directoryEntry2.Invoke("AppCreate", (object)true);
                    directoryEntry2.Properties["AuthFlags"].Value = (object)1;
                    directoryEntry2.Properties["AppFriendlyName"].Value = (object)name;
                    if (!string.IsNullOrEmpty(defaultDocument))
                        directoryEntry2.Properties["DefaultDoc"].Value = (object)"default.aspx";
                    directoryEntry2.CommitChanges();
                    directoryEntry1.CommitChanges();
                    directoryEntry2.Close();
                }
                directoryEntry1.Close();
            }
            if (this.IisVersion.Major >= 6)
                this.AssignApplicationToPool(domain, str + "/" + name, applicationPool, 4);
            if (this.IisVersion.Major >= 7 && !isNested)
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    ConfigurationSection section = serverManager.GetApplicationHostConfiguration().GetSection("system.webServer/security/authentication/anonymousAuthentication", website.Name);
                    section["enabled"] = (object)true;
                    section["userName"] = (object)"";
                    section["password"] = (object)"";
                    serverManager.CommitChanges();
                }
                this.RegisterWcf();
            }
            return new RayvarzInstaller.ModernUI.App.Models.VirtualDirectory(name, str + "/" + name);
        }
    }
}
