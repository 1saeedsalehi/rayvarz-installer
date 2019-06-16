using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.Windows;
using RayvarzInstaller.ModernUI.Windows.Controls;
using RayvarzInstaller.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RayvarzInstaller.ModernUI.App.Pages
{
    
    public partial class DatabaseView : UserControl,IContent
    {
        private OperationState OperationState;
        
        public DatabaseView()
        {   
            InitializeComponent();
            FillCatalog(null);
        }

        private void FillCatalog(string selectedItem)
        {
            string[] items = { "EOFFICE", "BPMS", "ARPG", "RSM" };
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(selectedItem) && item == selectedItem)
                {
                    CatalogList.Items.Add(new ComboBoxItem { Content = item, IsSelected = true });
                }
                CatalogList.Items.Add(new ComboBoxItem { Content = item});
            }
        }

        public void OnFragmentNavigation(Windows.Navigation.FragmentNavigationEventArgs e)
        {
            //occures after navigateTo method
            //read passed data from e.Fragment
            //deserialize data!
            OperationState = JsonConvert.DeserializeObject<OperationState>(e.Fragment);
            ManipulateForm();
            
        }

        public void OnNavigatedFrom(Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatedTo(Windows.Navigation.NavigationEventArgs e)
        {
            //occures when navigated to this page!
            //e.NavigationType can be New,Back,Refresh
        }

        public void OnNavigatingFrom(Windows.Navigation.NavigatingCancelEventArgs e)
        {
            //occures before navigating from this page to another 
            //write validation logic here
            //set e.Cancel = true; if you want to prevent navigating
        }
        private void GotoNextPage(object sender, System.Windows.RoutedEventArgs e)
        {
            var validationResult = ValidateForm();

            if (validationResult.Errors.Count() > 0)
            {
                ModernDialog.ShowMessage(string.Join("\r\n", validationResult.Errors), "", MessageBoxButton.OK);
            }
            else
            {
                OperationState.Data.DatabaseName = DbName.Text;
                OperationState.Data.Username = DbUSerName.Text;
                OperationState.Data.Password = DbPassword.Password;
                OperationState.Data.Servername = DbServer.Text;
                OperationState.Data.CatalogName = CatalogList.SelectionBoxItem.ToString();
                var dataTransfer = JsonConvert.SerializeObject(OperationState);
                //Serialize parametres using json!
                NavigationCommands.GoToPage.Execute("/Pages/ProgressView.xaml#" + dataTransfer, null);
            }
        }

        private void GoToPrevPage(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationCommands.GoToPage.Execute("/Pages/PathView.xaml", null);
        }

        private void CheckDb_Clicked(object sender, RoutedEventArgs e)
        {
            var result = ValidateConnection();

            if (result)
            {
                ModernDialog.ShowMessage("ارتباط با پایگاه داده با موفقیت برقرار شد", "", MessageBoxButton.OK);
            }
            else
            {

                ModernDialog.ShowMessage("ارتباط با پایگاه داده برقرار نشد", "", MessageBoxButton.OK);
            }

        }

        private void ManipulateForm() {
            DbName.Text = OperationState.Data.DatabaseName;
            DbUSerName.Text = OperationState.Data.Username ;
            DbServer.Text = OperationState.Data.Servername;
            FillCatalog(OperationState.Data.CatalogName);

        }

        public bool ValidateConnection()
        {
            var connectionString = $"Data Source={DbServer.Text};Initial Catalog={DbName.Text}; Persist Security Info=True; User ID={ DbUSerName.Text};Password={DbPassword.Password};";

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private ErrorMessages ValidateForm()
        {
            var errorMessages = new ErrorMessages();

            if (CatalogList.SelectedItem == null)
            {
                errorMessages.Errors.Add("نام کاتالوگ حتما باید انتخاب شود !");
            }

            if (string.IsNullOrEmpty(DbUSerName.Text))
            {
                errorMessages.Errors.Add("نام کاربری خالی است");
            }

            if (string.IsNullOrEmpty(DbName.Text))
            {
                errorMessages.Errors.Add("نام پایگاه داده خالی است");
            }

            if (string.IsNullOrEmpty(DbServer.Text))
            {
                errorMessages.Errors.Add("آدرس سرور پایگاه داده خالی است");
            }

            if (string.IsNullOrEmpty(DbPassword.Password))
            {
                errorMessages.Errors.Add("کلمه عبور خالی است");
            }
            return errorMessages;
        }
    }
}
