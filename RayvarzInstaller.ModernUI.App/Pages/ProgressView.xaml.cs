using Newtonsoft.Json;
using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.App.Services;
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
    /// <summary>
    /// Interaction logic for LayoutBasic.xaml
    /// </summary>
    public partial class ProgressView : UserControl , IContent
    {
        private readonly SetupServices setupServices;
        private IDPSetup IDPSetup;
        public ProgressView()
        {
            setupServices = new SetupServices(new SystemFileHelper() , new WebDeployHelper() , new SetupRegistry());
            InitializeComponent();
        }

        public void OnFragmentNavigation(Windows.Navigation.FragmentNavigationEventArgs e)
        {
            IDPSetup = JsonConvert.DeserializeObject<IDPSetup>(e.Fragment);
            setupServices.onStateChanged += SetupServices_onStateChanged;
            setupServices.Install(IDPSetup).ConfigureAwait(false);
        }

        private void SetupServices_onStateChanged(string message)
        {
            idpProgressbar.Value += 0.15;
        }

        public void OnNavigatedFrom(Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        public void OnNavigatedTo(Windows.Navigation.NavigationEventArgs e)
        {
            //e.Content
            //Do Stuff here!
            
        }

        public void OnNavigatingFrom(Windows.Navigation.NavigatingCancelEventArgs e)
        {
            
        }
        
    }
}
