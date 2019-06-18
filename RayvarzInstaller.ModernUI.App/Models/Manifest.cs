using System.Drawing;
using System.IO;
using System.Text;


namespace RayvarzInstaller.ModernUI.App.Models
{
    public class Manifest
    {
        private string _version;
        private string _packageId;
        private Image _logo;
        public string PackageId
        {
            get
            {
                return this._packageId;
            }
            set
            {
                this._packageId = value.ToUpper();
            }
        }

        public string Version
        {
            get
            {
                return this._version;
            }
            set
            {
                this._version = value.ToUpper();
            }
        }

        public Image Logo
        {
            get
            {
                if (this._logo != null || !File.Exists(System.IO.Path.Combine(this.Path, "logo.png")))
                    return this._logo;
                this._logo = Image.FromFile(System.IO.Path.Combine(this.Path, "logo.png"));
                return this._logo;
            }
        }

        public string Readme
        {
            get
            {
                string path = System.IO.Path.Combine(this.Path, "readme.txt");
                return File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : string.Empty;
            }
        }

        public string Path { get; set; }

        public bool IsChecked { get; set; }

        public Manifest()
        {

        }
    }
}
