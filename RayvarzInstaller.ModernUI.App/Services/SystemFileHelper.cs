using ChinhDo.Transactions.FileManager;
using IWshRuntimeLibrary;
using RayvarzInstaller.ModernUI.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;

namespace RayvarzInstaller.ModernUI.App.Services
{
    public class SystemFileHelper : ISystemFileHelper
    {
        private readonly TxFileManager _fileManager;
        private int _stepIndex;
        private int _stepCount;

        public SystemFileHelper()
        {
            this._fileManager = new TxFileManager();
        }

        public int FileCount(string sourcePath)
        {
            string[] directories = Directory.GetDirectories(sourcePath);
            return Directory.GetFiles(sourcePath).Length + ((IEnumerable<string>)directories).Sum<string>((Func<string, int>)(folder => this.FileCount(folder)));
        }

        public void Copy(string sourcePath, string destinationPath, int stepIndex, int stepCount, OnStateChangedDelegtae onStateChanged, bool overwriteIfDifferent)
        {
            this._stepCount = stepCount;
            this._stepIndex = stepIndex;
            this.CopyFolder(sourcePath, destinationPath, overwriteIfDifferent, onStateChanged);
            stepIndex = this._stepIndex;
        }

        private void CopyFolder(string sourcePath, string destinationPath, bool overwriteIfDifferent, OnStateChangedDelegtae onStateChanged)
        {
            string[] directories = Directory.GetDirectories(sourcePath);
            string[] files = Directory.GetFiles(sourcePath);
            if (!Directory.Exists(destinationPath))
                this._fileManager.CreateDirectory(destinationPath);
            foreach (string str in directories)
            {
                string path2 = Path.Combine(destinationPath, new DirectoryInfo(str).Name);
                this.CopyFolder(str, Path.Combine(destinationPath, path2), overwriteIfDifferent, onStateChanged);
            }
            foreach (string str1 in files)
            {
                FileInfo fileInfo1 = new FileInfo(str1)
                {
                    IsReadOnly = false
                };
                onStateChanged(string.Format("Copying {0} - {1}/{2}", (object)fileInfo1.Name, (object)this._stepIndex, (object)this._stepCount));
                string str2 = Path.Combine(destinationPath, fileInfo1.Name);
                if (overwriteIfDifferent && System.IO.File.Exists(str2))
                {
                    FileInfo fileInfo2 = new FileInfo(str2);
                    if (fileInfo2.LastWriteTime != fileInfo1.LastWriteTime || fileInfo2.Length != fileInfo1.Length)
                        this._fileManager.Copy(str1, str2, true);
                    else
                        this._stepIndex = this._stepIndex + 1;
                }
                else
                    this._fileManager.Copy(str1, str2, true);
            }
        }

        public void CopyFile(string sourcePath, string destinationPath, bool overwriteIfDifferent)
        {
            FileInfo fileInfo1 = new FileInfo(sourcePath);
            if (string.IsNullOrEmpty(destinationPath))
                return;
            if (!Directory.Exists(Path.GetDirectoryName(destinationPath)))
                this._fileManager.CreateDirectory(Path.GetDirectoryName(destinationPath));
            if (overwriteIfDifferent && System.IO.File.Exists(destinationPath))
            {
                FileInfo fileInfo2 = new FileInfo(destinationPath);
                if (fileInfo2.LastWriteTime != fileInfo1.LastWriteTime || fileInfo2.Length != fileInfo1.Length)
                    this._fileManager.Copy(sourcePath, destinationPath, true);
                fileInfo2.IsReadOnly = false;
            }
            else
                this._fileManager.Copy(sourcePath, destinationPath, true);
        }

        public void Delete(string folder)
        {
            try
            {
                Directory.Delete(folder, true);
            }
            catch
            {
            }
        }

        public void CreateShortcut(string destinationPath, string sourceFilePath, string description)
        {
            this.CreateShortcut(destinationPath, Path.GetFileNameWithoutExtension(sourceFilePath) + ".lnk", sourceFilePath, description);
        }

        public void CreateShortcut(string destinationPath, string linkName, string sourceFilePath, string description)
        {
            if (!Directory.Exists(destinationPath))
                this._fileManager.CreateDirectory(destinationPath);
            if (System.IO.File.Exists(Path.Combine(destinationPath, linkName)))
                this._fileManager.Delete(Path.Combine(destinationPath, linkName));
            WshShortcut shortcut = (WshShortcut)new WshShellClass().CreateShortcut(Path.Combine(destinationPath, linkName));
            shortcut.Description = description;
            shortcut.TargetPath = sourceFilePath;
            shortcut.WorkingDirectory = Path.GetDirectoryName(sourceFilePath);
            shortcut.Save();
        }

        public void SetFolderPermission(string path, FileSystemAccessRule accessRule)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            DirectorySecurity accessControl = directoryInfo.GetAccessControl();
            accessControl.AddAccessRule(accessRule);
            directoryInfo.SetAccessControl(accessControl);
        }
    }
}
