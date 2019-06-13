using RayvarzInstaller.ModernUI.App.Models;
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
    public partial class DatabaseView : UserControl
    {
        public DatabaseView()
        {
            var idpSetup = new IDPSetup
            {
                CatalogName = CatalogList.SelectedValue.ToString(),
                DatabaseName = DbName.Text,
                Username = DbUSerName.Text,
                Password = DbPassword.Password,
                Servername = DbServer.Text
            };
            InitializeComponent();
        }
    }
}
