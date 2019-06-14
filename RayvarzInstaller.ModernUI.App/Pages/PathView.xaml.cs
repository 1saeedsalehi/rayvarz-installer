﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.Windows;
using RayvarzInstaller.ModernUI.Windows.Navigation;

namespace RayvarzInstaller.ModernUI.App.Pages
{
   
    public partial class PathView : System.Windows.Controls.UserControl, IContent
    {
        public PathView()
        {
            InitializeComponent();
            FillFileAddress();
        }

        private void ChooseIdpManagementInstallationPathDirectory(object sender, System.Windows.RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "IDP مسیر نصب سامانه";
            fbd.ShowNewFolderButton = true;
            var browseDilogResult = fbd.ShowDialog();

            IdpManagementInstallationPath.Text = fbd.SelectedPath;
        }
        private void ChooseDirectory(object sender, System.Windows.RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "(ادمین) IDP مسیر نصب برنامه مدیریت سامانه";
            fbd.ShowNewFolderButton = true;
            var browseDilogResult = fbd.ShowDialog();

            IdpInstallationPath.Text = fbd.SelectedPath;
        }

        private void ModernButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            


        }
        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            //occures after navigateTo method
            //read passed data from e.Fragment
            //deserialize data!
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
            //occures when navigated to this page!
            //e.NavigationType can be New,Back,Refresh
            
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //occures before navigating from this page to another 
            //write validation logic here
            //set e.Cancel = true; if you want to prevent navigating
        }

        private void GotoNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            var idpSetting = new IDPSetup
            {
                IDPFolderName = IdpPathOnIIS.Text,
                IDPPath = IdpInstallationPath.Text,
                AdminFolderName = IISAdminManagementName.Text,
                AdminPath = IdpManagementInstallationPath.Text,
                IDPAddress = IdpServerPath.Text,
            };
            var transferData = JsonConvert.SerializeObject(idpSetting);
            //Serialize parametres using json!
            NavigationCommands.GoToPage.Execute("/Pages/DatabaseView.xaml#"+transferData,null); 
        }

        private void GoToPrevPage(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationCommands.GoToPage.Execute("/Pages/Introduction.xaml", null);
        }

        private void FillFileAddress()
        {
            string fileAddress = "";
            try
            {
                ServerManager sm = new ServerManager();
                Site site = sm.Sites[0];

                fileAddress = site.LogFile.Directory.Replace("%SystemDrive%\\", Path.GetPathRoot(Environment.SystemDirectory)).Replace("logs\\LogFiles", "wwwroot\\");
            }
            catch
            {

            }
            IdpInstallationPath.Text = !(string.IsNullOrEmpty(fileAddress)) ? fileAddress : "C:\\inetpub\\wwwroot\\";
            IdpManagementInstallationPath.Text = !(string.IsNullOrEmpty(fileAddress)) ? fileAddress : "C:\\inetpub\\wwwroot\\";
            IdpServerPath.Text = "http://localhost";
        }
    }
}
