using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RayvarzInstaller.ModernUI.App.Models
{
    public class Manifest
    {
        private string _version;
        private string _packageId;
        private Image _logo;

        public RayvarzSystem RayvarzSystem { get; set; }

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

        public List<string> DependencyPackages { get; set; }

        public List<string> CompatiblePackages { get; set; }

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

        public string PackageName
        {
            get
            {
                switch (this.RayvarzSystem)
                {
                    case RayvarzSystem.BPMS:
                        return "سامانه مدیریت فرآیند مدیریت کسب و کار";
                    case RayvarzSystem.EOA:
                        return "اتوماسیون اداری";
                    case RayvarzSystem.SSO:
                        return "لاگین یکپارچه رایورز";
                    default:
                        return (string)null;
                }
            }
        }

        public string Path { get; set; }

        public bool IsChecked { get; set; }

        public Manifest()
        {
            this.DependencyPackages = new List<string>();
            this.CompatiblePackages = new List<string>();
        }

        public bool IsCompatibleTo(Manifest manifest)
        {
            return this.CompatiblePackages.Any<string>((Func<string, bool>)(m => manifest.PackageId == m)) && manifest.CompatiblePackages.Any<string>((Func<string, bool>)(m => m == this.PackageId));
        }
    }
}
