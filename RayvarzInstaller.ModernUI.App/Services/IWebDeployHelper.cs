using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.Collections.Generic;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public interface IWebDeployHelper
    {
        Version IisVersion { get; }

        List<WebSiteEntry> GetWebSites(string domainName);

        List<RayvarzInstaller.ModernUI.App.Models.VirtualDirectory> GetVirtualDirectories(WebSiteEntry webSite);

        List<IisApplicationPool> GetApplicationPools(string domainName);

        bool CreateApplicationPool(string domainName, string name, string identity, int dotNetMajorVersion);

        bool ConfigApplicationPool(string domainName, string name, string identity, int dotNetMajorVersion);

        bool CreateWebSite(string name, string domain, string ipAddress, int port, string header, string installationPath, string defaultDocument, string applicationPool);

        VirtualDirectory CreateVirtualDirectory(string name, string domain, string installationPath, string defaultDocument, WebSiteEntry website, string applicationPool, bool isNested, bool keepAppPoolAsIs = false);

        VirtualDirectory CreateVirtualDirectory(string name, string installationPath, string applicationPool, bool keepAppPoolAsIs = false);

        bool RemoveVirtualDirectory(string path);

        bool VirtualDirectoryExists(WebSiteEntry webSite, string virtualDirectoryName);

        bool VirtualDirectoryExists(string virtualDirectoryName);

        bool WebDeployServiceVirtualDirectoryExists(WebSiteEntry webSite, string virtualDirectoryName);

        void StopIis();

        void StartIis();

        void RegisterWcf();

        void VsDiagUnregWcf();

        bool RegisterAspNet4ToIIS(OnStateChangedDelegate onStateChanged, ref int stepIndex, int stepCount);

        bool RegisterIisFramework(string iisApplicationPath, int majorVersion);
    }
}
