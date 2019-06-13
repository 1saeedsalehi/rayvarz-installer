using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayvarzInstaller.ModernUI.App.Models
{
    public class IisApplicationPool : IisEntry
    {
        public IisApplicationPool(string name, string path)
          : base(name, path)
        {
        }
    }
}
