using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public class SetupRegistry : ISetupRegistry
    {
        private readonly string DefaultRegistryPath = string.Format("{0}\\Rayvarz\\Registry\\", (object)Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
        private const string DefaultRegistryFileName = "registry.json";
        private string _registryPath;
        private Dictionary<Guid, InstallPathInfo> _installPaths;

        private string RegistryFilePath
        {
            get
            {
                return this._registryPath;
            }
        }

        public List<InstallPathInfo> InstallPaths
        {
            get
            {
                return this._installPaths.Select<KeyValuePair<Guid, InstallPathInfo>, InstallPathInfo>((Func<KeyValuePair<Guid, InstallPathInfo>, InstallPathInfo>)(item => item.Value)).OrderBy<InstallPathInfo, string>((Func<InstallPathInfo, string>)(item => item.PackageId)).ToList<InstallPathInfo>();
            }
        }

        //[DefaultConstructor]
        public SetupRegistry()
        {
            this._registryPath = string.Format("{0}{1}", (object)this.DefaultRegistryPath, (object)"registry.json");
            this._installPaths = new Dictionary<Guid, InstallPathInfo>();
            this.CreateDefaultDirectory();
            this.LoadRegistry();
        }

        public SetupRegistry(string jsonPath)
        {
            this._registryPath = jsonPath;
            this._installPaths = new Dictionary<Guid, InstallPathInfo>();
            this.CreateDefaultDirectory();
            this.LoadRegistry();
        }

        public InstallPathInfo this[Guid key]
        {
            get
            {
                if (!this._installPaths.ContainsKey(key))
                    return (InstallPathInfo)null;
                return this._installPaths[key];
            }
        }

        private void CreateDefaultDirectory()
        {
            if (Directory.Exists(this.DefaultRegistryPath))
                return;
            Directory.CreateDirectory(this.DefaultRegistryPath);
        }

        private void LoadRegistry()
        {
            if (!File.Exists(this.RegistryFilePath))
                return;
            this._installPaths = JsonConvert.DeserializeObject<Dictionary<Guid, InstallPathInfo>>(File.ReadAllText(this.RegistryFilePath));
        }

        public void Add(Manifest manifest, InstallPathInfo info)
        {
            info.PackageId = manifest.PackageId;
            info.System = manifest.RayvarzSystem;
            info.Id = Guid.NewGuid();
            this._installPaths.Add(info.Id, info);
        }

        public void Remove(Manifest manifest, InstallPathInfo info)
        {
            info.PackageId = manifest.PackageId;
            this._installPaths.Remove(info.Id);
        }

        public void Commit()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            JsonConvert.DefaultSettings = (Func<JsonSerializerSettings>)(() => settings);
            this.CreateDefaultDirectory();
            File.WriteAllText(this.RegistryFilePath, JsonConvert.SerializeObject((object)this._installPaths));
        }
    }
}
