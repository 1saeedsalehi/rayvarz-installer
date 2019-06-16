using System.Collections.Generic;

namespace RayvarzInstaller.ModernUI.App.Models
{
    public class ErrorMessages
    {
        public List<string> Errors { get; set; }

        public ErrorMessages()
        {
            Errors = new List<string>();
        }
    }
}
