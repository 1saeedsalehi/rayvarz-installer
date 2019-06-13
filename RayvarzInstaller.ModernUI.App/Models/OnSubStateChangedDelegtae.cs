using System;

namespace RayvarzInstaller.ModernUI.App.Models
{

    public delegate void OnStartedDelegate(string message);
    public delegate void OnCompletedDelegate(string message);
    public delegate void OnCancelDelegate(string message);
    public delegate void OnStateChangedDelegtae(string message);
    public delegate void OnErrorDelegate(Exception exception);

}
