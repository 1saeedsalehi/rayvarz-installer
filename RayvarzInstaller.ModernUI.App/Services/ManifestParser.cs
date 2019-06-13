using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using System.IO;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public class ManifestParser : IManifestParser
    {
        public Manifest Parse(string path)
        {
            Manifest manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(path));
            manifest.Path = Path.GetDirectoryName(path);
            return manifest;
        }
    }
}
