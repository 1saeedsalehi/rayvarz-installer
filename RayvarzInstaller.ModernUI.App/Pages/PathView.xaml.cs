using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.App.Services;
using RayvarzInstaller.ModernUI.Windows;
using RayvarzInstaller.ModernUI.Windows.Controls;
using RayvarzInstaller.ModernUI.Windows.Navigation;

namespace RayvarzInstaller.ModernUI.App.Pages
{

    public partial class PathView : System.Windows.Controls.UserControl, IContent
    {
        private OperationState OperationState;
        private WebDeployHelper webDeployHelper = new WebDeployHelper();
        public PathView()
        {
            InitializeComponent();
            //FillFileAddress();
            FillFileName();
        }


        private void FillDomainsCombo(string selectedItem)
        {
            List<String> domains;
            DomainList.Items.Clear();
            using (ServerManager mgr = new ServerManager())
            {
                domains = mgr.Sites.Select(c => c.Name).ToList();
            }
            //foreach (var domain in domains)
            //{
            //    if (!string.IsNullOrEmpty(selectedItem) && domain == selectedItem)
            //    {
            //        DomainList.Items.Add(new ComboBoxItem { Content = domain, IsSelected = true });
            //    }
            //    DomainList.Items.Add(new ComboBoxItem { Content = domain });
            //}
            for (int i = 0; i < domains.Count; i++)
            {
                var domain = domains[i];
                if (!string.IsNullOrEmpty(selectedItem) && domain == selectedItem)
                {
                    DomainList.Items.Add(new ComboBoxItem { Content = domain, IsSelected = true });
                }
                else
                {
                    if (string.IsNullOrEmpty(selectedItem) && i == 0)
                    {
                        DomainList.Items.Add(new ComboBoxItem { Content = domain, IsSelected = true });
                    }
                    else
                    {
                        DomainList.Items.Add(new ComboBoxItem { Content = domain });
                    }
                }
              
                
            }
        }


        private void ChooseIdpManagementInstallationPathDirectory(object sender, System.Windows.RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "(ادمین) IDP مسیر نصب مدیریت سامانه";
            fbd.ShowNewFolderButton = true;
            var browseDilogResult = fbd.ShowDialog();

            if (!string.IsNullOrEmpty(fbd.SelectedPath))
            {
                IdpManagementInstallationPath.Text = fbd.SelectedPath;
            }

        }
        private void ChooseDirectory(object sender, System.Windows.RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "IDP مسیر نصب برنامه ";
            fbd.ShowNewFolderButton = true;
            var browseDilogResult = fbd.ShowDialog();

            if (!string.IsNullOrEmpty(fbd.SelectedPath))
            {
                IdpInstallationPath.Text = fbd.SelectedPath;
            }

        }

        private void ModernButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {



        }
        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            //occures after navigateTo method
            //read passed data from e.Fragment
            //deserialize data!
            OperationState = JsonConvert.DeserializeObject<OperationState>(e.Fragment);

            if (OperationState.Operation == Operation.Modified)
            {
                ManipulateForm();
                DisableControl();
                FillDomainsCombo(OperationState.Data.DomainName);
            }

            if (OperationState.Operation == Operation.Add)
            {
                if (string.IsNullOrEmpty(OperationState.Data.DomainName))
                {
                    ResetForm();
                    FillFileName();
                    FillDomainsCombo(null);
                }

                EnableControl();
                //FillFileAddress(DomainList.SelectionBoxItem.ToString());
            }
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
            if (OperationState.Operation == Operation.Add)
            {
                GotoNextPageAddMode();
            }

            if (OperationState.Operation == Operation.Modified)
            {
                GotoNextPageModifiedMode();
            }

        }

        private void GotoNextPageAddMode()
        {
            var validationResult = ValidateForm();
            if (validationResult.Errors.Count > 0)
            {
                ModernDialog.ShowMessage(string.Join("\r\n", validationResult.Errors), "", MessageBoxButton.OK);
            }
            else
            {
                OperationState.Data.DomainName = DomainList.SelectionBoxItem.ToString();
                OperationState.Data.IDPFolderName = IdpPathOnIIS.Text.ToLower();
                OperationState.Data.IDPPath = IdpInstallationPath.Text;
                OperationState.Data.AdminFolderName = IISAdminManagementName.Text.ToLower();
                OperationState.Data.AdminPath = IdpManagementInstallationPath.Text;
                OperationState.Data.IDPAddress = IdpServerPath.Text;

                NavigateToNextPage();
            }
        }

        private void GotoNextPageModifiedMode()
        {

            if (string.IsNullOrEmpty(IdpServerPath.Text))
            {
                ModernDialog.ShowMessage("آدرس سرور سامانه IDP خالی می باشد", "", MessageBoxButton.OK);
            }
            else
            {

                OperationState.Data.IDPAddress = IdpServerPath.Text;
                NavigateToNextPage();
            }
        }

        private void NavigateToNextPage()
        {
            var transferData = JsonConvert.SerializeObject(OperationState);
            NavigationCommands.GoToPage.Execute("/Pages/DatabaseView.xaml#" + transferData, null);
        }

        private void GoToPrevPage(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationCommands.GoToPage.Execute("/Pages/Introduction.xaml", null);
        }

        private void FillFileAddress(string sitename)
        {
            var fileAddress = GetPhysicalPath(sitename);
            //string fileAddress = "";
            //try
            //{
            //    ServerManager sm = new ServerManager();
            //    Site site = sm.Sites[0];

            //    fileAddress = site.LogFile.Directory.Replace("%SystemDrive%\\", Path.GetPathRoot(Environment.SystemDirectory)).Replace("logs\\LogFiles", "wwwroot\\");
            //}
            //catch
            //{

            //}
            IdpInstallationPath.Text = !(string.IsNullOrEmpty(fileAddress)) ? fileAddress : "C:\\inetpub\\wwwroot\\";
            IdpManagementInstallationPath.Text = !(string.IsNullOrEmpty(fileAddress)) ? fileAddress : "C:\\inetpub\\wwwroot\\";
            IdpServerPath.Text = "http://localhost";
        }

        private void FillFileName()
        {

            IdpPathOnIIS.Text = "rayvarzsso";
            IISAdminManagementName.Text = "rayvarzssoadmin";

        }


        private void ManipulateForm()
        {
            IdpPathOnIIS.Text = OperationState.Data.IDPFolderName;
            IdpInstallationPath.Text = OperationState.Data.IDPPath;
            IISAdminManagementName.Text = OperationState.Data.AdminFolderName;
            IdpManagementInstallationPath.Text = OperationState.Data.AdminPath;
            IdpServerPath.Text = OperationState.Data.IDPAddress;
        }
        private void DisableControl()
        {
            DomainList.IsEnabled = false;
            IdpPathOnIIS.IsEnabled = false;
            IdpInstallationPath.IsEnabled = false;
            IISAdminManagementName.IsEnabled = false;
            IdpManagementInstallationPath.IsEnabled = false;
            btnIDPAdminPath.IsEnabled = false;
            btnIDPPath.IsEnabled = false;
        }
        private void EnableControl()
        {
            DomainList.IsEnabled = true;
            IdpPathOnIIS.IsEnabled = true;
            IdpInstallationPath.IsEnabled = true;
            IISAdminManagementName.IsEnabled = true;
            IdpManagementInstallationPath.IsEnabled = true;
            btnIDPAdminPath.IsEnabled = true;
            btnIDPPath.IsEnabled = true;
        }


        private void ResetForm()
        {
            IdpPathOnIIS.Text = "";
            IISAdminManagementName.Text = "";
        }

        private ErrorMessages ValidateForm()
        {
            var errorMessages = new ErrorMessages();

            if (DomainList.SelectedItem == null)
            {
                errorMessages.Errors.Add("نام دامین حتما باید انتخاب شود !");
            }

            if (string.IsNullOrEmpty(IdpPathOnIIS.Text))
            {
                errorMessages.Errors.Add("نام سامانه IDP خالی است");
            }

            if (IdpPathOnIIS.Text.Length > 64)
            {
                errorMessages.Errors.Add("نام سامانه IDP نمی تواند بیشتر از 64 کاراکتر باشد");
            }


            if (string.IsNullOrEmpty(IdpInstallationPath.Text))
            {
                errorMessages.Errors.Add("مسیر نصب سامانه IDP خالی است");
            }
            else
            {

                if (Path.IsPathRooted(IdpInstallationPath.Text))
                {
                    string idpPath = Path.GetPathRoot(IdpInstallationPath.Text);
                    if (!Directory.Exists(idpPath))
                    {
                        errorMessages.Errors.Add("مسیر نصب وارد شده برای نصب سامانه IDP موجود نمی باشد");
                    }
                }
                else
                {
                    errorMessages.Errors.Add("مسیر نصب وارد شده برای نصب سامانه IDP معتبر نمی باشد");
                }
            }


            var regexItem = new Regex("^[-a-zA-Z0-9_]*$");
            if (!regexItem.IsMatch(IdpPathOnIIS.Text))
            {
                errorMessages.Errors.Add(" امکان درج کاراکترهای خاص در نام سامانه نمی باشد .");
            }
            else {
                if (!string.IsNullOrEmpty(DomainList.SelectionBoxItem?.ToString()))
                {
                    if (webDeployHelper.ExistVirtualDirectoryV2(DomainList.SelectionBoxItem.ToString(), IdpPathOnIIS.Text))
                    {
                        errorMessages.Errors.Add("برنامه ای با نام سامانه IDP در IIS وجود دارد.");
                    }
                }

            }
            if (string.IsNullOrEmpty(IISAdminManagementName.Text))
            {
                errorMessages.Errors.Add("نام برنامه مدیریت سامانه IDP خالی است");
            }

            if (!regexItem.IsMatch(IISAdminManagementName.Text))
            {
                errorMessages.Errors.Add("امکان درج کاراکترهای خاص در نام سامانه مدیریت نمی باشد .");
            }
            else
            {
                if (!string.IsNullOrEmpty(DomainList.SelectionBoxItem?.ToString()))
                {
                    if (webDeployHelper.ExistVirtualDirectoryV2(DomainList.SelectionBoxItem.ToString(), IISAdminManagementName.Text))
                    {
                        errorMessages.Errors.Add("برنامه ای با نام برنامه مدیریت سامانه IDP در IIS وجود دارد.");
                    }
                }

            }

            if (IISAdminManagementName.Text.Length > 64)
            {
                errorMessages.Errors.Add("نام سامانه مدیریت IDP نمی تواند بیشتر از 64 کاراکتر باشد");
            }


            if (string.IsNullOrEmpty(IdpManagementInstallationPath.Text))
            {
                errorMessages.Errors.Add("مسیر نصب برنامه مدیریت سامانه IDP خالی است");
            }
            else
            {

                if (Path.IsPathRooted(IdpManagementInstallationPath.Text))
                {
                    string adminPath = Path.GetPathRoot(IdpManagementInstallationPath.Text);
                    if (!Directory.Exists(adminPath))
                    {
                        errorMessages.Errors.Add("مسیر نصب وارد شده برای نصب سامانه مدیریت IDP موجود نمی باشد");
                    }
                }
                else
                {
                    errorMessages.Errors.Add("مسیر نصب وارد شده برای نصب سامانه مدیریت IDP معتبر نمی باشد");
                }
            }

            if (string.IsNullOrEmpty(IdpServerPath.Text))
            {
                errorMessages.Errors.Add("آدرس سرور سامانه IDP خالی می باشد");
            }

            if (IdpPathOnIIS.Text == IISAdminManagementName.Text)
            {
                errorMessages.Errors.Add("نام برنامه مدیریت سامانه نمی تواند با نام اصلی برنامه یکی باشد");
            }

           




            if (!string.IsNullOrEmpty(IdpPathOnIIS.Text) && Directory.Exists(GetRealFileAddress(IdpInstallationPath.Text, IdpPathOnIIS.Text)))
            {
                errorMessages.Errors.Add("پوشه ای با این نام در مسیر تعیین شده برای نصب سامانه IDP موجود است");
            }

            if (!string.IsNullOrEmpty(IISAdminManagementName.Text) && Directory.Exists(GetRealFileAddress(IdpManagementInstallationPath.Text, IISAdminManagementName.Text)))
            {
                errorMessages.Errors.Add("پوشه ای با این نام در مسیر تعیین شده برای نصب مدیریت IDP موجود است");
            }

            return errorMessages;
        }

        private string GetRealFileAddress(string path, string name)
        {

            return string.Format("{0}\\{1}", path, name);
        }

        private void DomainList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var items = e.AddedItems;
            if (items.Count > 0)
            {
                string siteName = (items[0] as ComboBoxItem).Content as string;
                FillFileAddress(siteName);
            }
        }

        private string GetPhysicalPath(string siteName)
        {
            var fileAddress = "";
            ServerManager sm = new ServerManager();

            fileAddress = sm.Sites[siteName].Applications["/"].VirtualDirectories["/"].PhysicalPath;
            if (fileAddress.Contains("%SystemDrive%"))
            {
                fileAddress = sm.Sites[siteName].LogFile.Directory.Replace("%SystemDrive%\\", Path.GetPathRoot(Environment.SystemDirectory)).Replace("logs\\LogFiles", "wwwroot\\");
            }

            return fileAddress;
        }
    }
}
