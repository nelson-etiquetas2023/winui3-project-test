using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;

namespace WINUI_TEST.Models
{
    public abstract class CatalogEntity : INotifyPropertyChanged
    {
        private string _name = "";
        private string _description = "";
        private bool _isActive = true;

        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        public string Description
        {
            get => _description;
            set { if (_description != value) { _description = value; OnPropertyChanged(); } }
        }

        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(); } }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class ProductUnit : CatalogEntity
    {
        private string _abbreviation = "";

        public string Abbreviation
        {
            get => _abbreviation;
            set { if (_abbreviation != value) { _abbreviation = value; OnPropertyChanged(); } }
        }
    }

    public sealed class ProductCategory : CatalogEntity
    {
    }

    public sealed class Warehouse : CatalogEntity
    {
        private string _location = "";

        public string Location
        {
            get => _location;
            set { if (_location != value) { _location = value; OnPropertyChanged(); } }
        }
    }

    public sealed class WarehouseLocation : CatalogEntity
    {
    }

    public sealed class CompanyInfo : INotifyPropertyChanged
    {
        private string _name = "";
        private string _address = "";
        private string _itbis = "";
        private ImageSource? _logo;

        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        public string Address
        {
            get => _address;
            set { if (_address != value) { _address = value; OnPropertyChanged(); } }
        }

        public string Itbis
        {
            get => _itbis;
            set { if (_itbis != value) { _itbis = value; OnPropertyChanged(); } }
        }

        public ImageSource? Logo
        {
            get => _logo;
            set { if (!Equals(_logo, value)) { _logo = value; OnPropertyChanged(); } }
        }

        public bool HasLogo => Logo != null;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}