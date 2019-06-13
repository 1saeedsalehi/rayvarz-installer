using RayvarzInstaller.ModernUI.App.Models;
using RayvarzInstaller.ModernUI.App.Services;
using RayvarzInstaller.ModernUI.Windows.Controls;
using RayvarzInstaller.ModernUI.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class Customer
    {
        public string Title { get; set; }
        public string Version { get; set; }
    }
    /// <summary>
    /// Interaction logic for Introduction.xaml
    /// </summary>
    public partial class Introduction : UserControl
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
                //do stuff
            }

        }
    }
}
