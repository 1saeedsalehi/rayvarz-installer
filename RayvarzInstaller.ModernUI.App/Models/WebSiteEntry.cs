using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayvarzInstaller.ModernUI.App.Models
{
    public class WebSiteEntry : IisEntry
    {
        public WebSiteEntry(string name, string path)
          : base(name, path)
        {
        }
    }
}
