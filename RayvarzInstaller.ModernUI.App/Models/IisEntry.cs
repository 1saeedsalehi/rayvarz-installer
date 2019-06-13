
namespace RayvarzInstaller.ModernUI.App.Models
{
    public abstract class IisEntry
    {
        protected bool Equals(IisEntry other)
        {
            return string.Equals(this.Path, other.Path);
        }

        public override int GetHashCode()
        {
            return this.Path != null ? this.Path.GetHashCode() : 0;
        }

        public string Name { get; set; }

        public string Path { get; set; }

        protected IisEntry(string name, string path)
        {
            this.Name = name;
            this.Path = path;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this == obj)
                return true;
            return obj.GetType() == this.GetType() && this.Equals((IisEntry)obj);
        }
    }
}
