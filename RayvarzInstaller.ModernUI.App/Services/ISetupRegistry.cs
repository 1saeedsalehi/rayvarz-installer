using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.Collections.Generic;


namespace RayvarzInstaller.ModernUI.App.Services
{
    public interface ISetupRegistry
    {
        InstallPathInfo this[Guid key] { get; }

        List<InstallPathInfo> InstallPaths { get; }

        void Add(Manifest manifest, InstallPathInfo info);

        void Remove(Manifest manifest, InstallPathInfo info);

        void Commit();
    }
}
