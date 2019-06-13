using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public interface ISystemFileHelper
    {
        void Copy(string sourcePath, string destinationPath, int stepIndex, int stepCount, OnStateChangedDelegtae onStateChanged, bool overwriteIfDifferent);
        void CopyFile(string sourcePath, string destinationPath, bool overwriteIfDifferent);
        void CreateShortcut(string destinationPath, string sourceFilePath, string description);
        void CreateShortcut(string destinationPath, string linkName, string sourceFilePath, string description);
        void Delete(string folder);
        int FileCount(string sourcePath);
        void SetFolderPermission(string path, FileSystemAccessRule accessRule);
    }
}
