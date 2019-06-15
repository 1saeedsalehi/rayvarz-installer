using System;

namespace RayvarzInstaller.ModernUI.App.Models
{
    public class OperationState
    {
        public Guid Id { get; set; }

        public Operation Operation { get; set; }

        public IDPSetup Data { get; set; }

    }

    public enum Operation {
        Add,
        Modified,
        Delete
    }
}
