using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.App.Services;
using RayvarzInstaller.ModernUI.Windows;
using RayvarzInstaller.ModernUI.Windows.Controls;
using RayvarzInstaller.ModernUI.Windows.Navigation;

namespace RayvarzInstaller.ModernUI.App.Pages
{
    public class Customer
    {
        public string Title { get; set; }
        public string Version { get; set; }
    }
    /// <summary>
    /// Interaction logic for Introduction.xaml
    /// </summary>
    public partial class Introduction : UserControl , IContent
    {
        
        public Introduction()
        {
            
            InitializeComponent();
            ObservableCollection<InstallPathInfo> custdata = GetData();

            //Bind the DataGrid to the customer data
            DG1.DataContext = custdata;
        }

        private ObservableCollection<InstallPathInfo> GetData()
        {
            var setupRegistry = new SetupRegistry();
            var paths = setupRegistry.InstallPaths;
            var pathInfoCollection = new ObservableCollection<InstallPathInfo>();
            paths.ForEach(path =>
            {
                pathInfoCollection.Add(path);
            });
            return pathInfoCollection;
        }

        private void GridRemove_Clicked(object sender, RoutedEventArgs e)
        {
            var model = (sender as Button).DataContext as Customer;
            //TODO: add navigation sample here!
            var dialogResult = ModernDialog.ShowMessage("Are you sure?", "Confirmation", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                //Serialize parametres using json!
                NavigationCommands.GoToPage.Execute("/Pages/PathView.xaml#Name=saeed&Family=Salehi", null);
            }
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
            //occures after navigateTo method
            //read passed data from e.Fragment
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
            //Serialize parametres using json!
            NavigationCommands.GoToPage.Execute("/Pages/PathView.xaml", null);
        }

        private void GoToPrevPage(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationCommands.GoToPage.Execute("/Pages/Introduction.xaml", null);
        }
    }
}
