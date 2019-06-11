using RayvarzInstaller.ModernUI.Windows;
using RayvarzInstaller.ModernUI.Windows.Navigation;
using System.Windows.Controls;
using System.Windows.Forms;

namespace RayvarzInstaller.ModernUI.App.Pages
{
    /// <summary>
    /// Interaction logic for LayoutBasic.xaml
    /// </summary>
    public partial class PathView : System.Windows.Controls.UserControl , IContent
    {
        public PathView()
        {   
            InitializeComponent();
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

        private void ModernButton_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            //TODO: go to next page and pass data
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
        }
    }
}
