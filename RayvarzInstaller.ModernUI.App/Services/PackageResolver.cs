using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.App.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public class PackageResolver
    {
        private IManifestParser _manifestParser;
        public PackageResolver()
        {
            _manifestParser = new ManifestParser();
        }
        public Manifest GetPackage()
        {
            var manifest = 
                ((IEnumerable<string>)Directory.GetDirectories("./Packages/")).ToList()
                .Where(d => File.Exists(string.Format("{0}/manifest.json", d)))
                .Select(d => _manifestParser.Parse(string.Format("{0}/manifest.json", d)))
                .OrderBy(d => d.Version).FirstOrDefault();
           // this.ValidatePackages(list);
            return manifest;
        }

        private void ValidatePackages(List<Manifest> manifests)
        {
            var source = manifests.GroupBy(c => c.PackageId).Select(n => new
            {
                Version = n.Key,
                Count = n.Count()
            }).Where(n => n.Count > 1);
            if (source.Count() > 0)
                throw new DuplicatePackageException(string.Join(",", source.Select(c => c.Version).ToArray<string>()));
        }

    }
}
