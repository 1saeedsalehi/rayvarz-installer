using System;


namespace RayvarzInstaller.ModernUI.App.Services
{
    public interface IHelper
    {
        void RegisterService64(string service64folder);

        bool Enable64Bit { get; }

        void AddPermissionToOfficeInterop();
    }
}
