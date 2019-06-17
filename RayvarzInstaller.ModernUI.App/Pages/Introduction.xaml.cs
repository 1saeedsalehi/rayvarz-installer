using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private PackageResolver PackageResolver;
        
        public Introduction()
        {
            
            InitializeComponent();

            GetPackeInformation();
            var installPathInfos = GetData();
            setPreviousInstallationVersionsGrid(installPathInfos);
            //Bind the DataGrid to the customer data
           
        }

        private void setPreviousInstallationVersionsGrid(ObservableCollection<InstallPathInfo> installPathInfos) {
            if (installPathInfos.Count > 0)
            {
                foreach (var info in installPathInfos)
                {
                    DG1.Items.Add(info);
                }
            }
            else {
                DG1.Visibility = Visibility.Hidden;
                DGTitle.Visibility = Visibility.Hidden;
            }
        }

        private void GetPackeInformation() {
            PackageResolver = new PackageResolver();
            var manifest = PackageResolver.GetPackage();
            txtCurrentVersion.Text = manifest.PackageId;
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
            var installPathInfo = (sender as Button).DataContext as InstallPathInfo;
            var idpSetup = IDPSetupMapper(installPathInfo);

            var operationState = new OperationState
            {
                Data = idpSetup,
                Operation = Operation.Delete,
                Id = installPathInfo.Id
            };
            var serializeSetup = JsonConvert.SerializeObject(operationState);
            //TODO: add navigation sample here!
            var dialogResult = ModernDialog.ShowMessage("قصد حذف این نسخه را دارید ؟", "", MessageBoxButton.YesNo);
            if (dialogResult == MessageBoxResult.Yes)
            {
                //Serialize parametres using json!
                NavigationCommands.GoToPage.Execute("/Pages/ProgressView.xaml#" + serializeSetup, null);
            }
        }

        private void GridUpdate_Clicked(object sender, RoutedEventArgs e)
        {
            var installPathInfo = (sender as Button).DataContext as InstallPathInfo;
            var idpSetup = IDPSetupMapper(installPathInfo);

            var operationState = new OperationState {
                Data = idpSetup , Operation = Operation.Modified , Id = installPathInfo.Id
            };
            //TODO: add navigation sample here!
            var serializeSetup = JsonConvert.SerializeObject(operationState);
            NavigationCommands.GoToPage.Execute("/Pages/PathView.xaml#" + serializeSetup, null);

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
            var operationState = new OperationState {
                Operation = Operation.Add , Data = new IDPSetup()
            };
            var transferData = JsonConvert.SerializeObject(operationState);
            NavigationCommands.GoToPage.Execute("/Pages/PathView.xaml#" + transferData, null);
        }

        private IDPSetup IDPSetupMapper(InstallPathInfo installPathInfo) {
            
            var idpSetup = new IDPSetup();
            idpSetup.DomainName = installPathInfo.Meta["DomainName"];
            idpSetup.IDPPath = installPathInfo.Meta["IDPPath"];
            idpSetup.IDPFolderName = installPathInfo.Meta["IDPFolderName"];
            idpSetup.AdminFolderName = installPathInfo.Meta["AdminFolderName"];
            idpSetup.AdminPath = installPathInfo.Meta["AdminPath"];
            idpSetup.IDPAddress = installPathInfo.Meta["IDPAddress"];
            idpSetup.Servername = installPathInfo.Meta["servername"];
            idpSetup.DatabaseName = installPathInfo.Meta["databasename"];
            idpSetup.CatalogName = installPathInfo.Meta["catalog"];
            idpSetup.Username = installPathInfo.Meta["username"];

            return idpSetup;
        }
    }
}
