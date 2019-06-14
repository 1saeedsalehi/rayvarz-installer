using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.Windows;
using RayvarzInstaller.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
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
        private IDPSetup IDPSetup;
        public DatabaseView()
        {
            
            InitializeComponent();
        }

        public void OnFragmentNavigation(Windows.Navigation.FragmentNavigationEventArgs e)
        {
            //occures after navigateTo method
            //read passed data from e.Fragment
            //deserialize data!
            var f = e.Fragment;
            IDPSetup = JsonConvert.DeserializeObject<IDPSetup>(e.Fragment);
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
            IDPSetup.DatabaseName = DbName.Text;
            IDPSetup.Username = DbUSerName.Text;
            IDPSetup.Password = DbPassword.Password;
            IDPSetup.Servername = DbServer.Text;
            IDPSetup.CatalogName = CatalogList.SelectionBoxItem.ToString();
            var data = JsonConvert.SerializeObject(IDPSetup);
            //Serialize parametres using json!
            NavigationCommands.GoToPage.Execute("/Pages/ProgressView.xaml#" + data, null);
        }

        private void GoToPrevPage(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationCommands.GoToPage.Execute("/Pages/PathView.xaml", null);
        }
    }
}
