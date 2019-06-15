using System;
using System.Collections.Generic;


namespace RayvarzInstaller.ModernUI.App.Models
{
    public class InstallPathInfo
    {
        public InstallPathInfo()
        {
            Meta = new Dictionary<string, string>();
        }
        public Guid Id { get; set; }
        public string PackageId { get; set; }
        public RayvarzSystem System { get; set; }
        public bool IsWeb { get; set; }
        public string PhysicalPath { get; set; }
        public Dictionary<string, string> Meta { get; set; }
    }
}
