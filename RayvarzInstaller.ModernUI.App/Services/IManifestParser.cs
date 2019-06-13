using RayvarzInstaller.ModernUI.App.Models;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public interface IManifestParser
    {
        Manifest Parse(string path);
    }
}
