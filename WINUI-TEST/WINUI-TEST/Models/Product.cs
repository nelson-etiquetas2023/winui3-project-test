using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml.Media;

namespace WINUI_TEST.Models
{
    public sealed class Product : INotifyPropertyChanged
    {
        private string _productName = "";
        private string _category = "";
        private double _price;
        private double _cost;
        private string _codeBar = "";
        private string _productId = "";
        private bool _isActive = true;
        private bool _trackSerial;
        private bool _trackLot;
        private string _unit = "pieza";
        private double _stock;
        private string _sku = "";
        private string _description = "";

        public string ProductId
        {
            get => _productId;
            set { if (_productId != value) { _productId = value; OnPropertyChanged(); } }
        }

        public string Sku
        {
            get => _sku;
            set { if (_sku != value) { _sku = value; OnPropertyChanged(); } }
        }

        public string Description
        {
            get => _description;
            set { if (_description != value) { _description = value; OnPropertyChanged(); } }
        }

        public string ProductName
        {
            get => _productName;
            set { if (_productName != value) { _productName = value; OnPropertyChanged(); } }
        }

        public string Category
        {
            get => _category;
            set { if (_category != value) { _category = value; OnPropertyChanged(); } }
        }

        public double Price
        {
            get => _price;
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PriceText));
                }
            }
        }

        public double Cost
        {
            get => _cost;
            set
            {
                if (_cost != value)
                {
                    _cost = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CostText));
                }
            }
        }

        public string PriceText => _price.ToString("N2", CultureInfo.InvariantCulture);

        public string CostText => _cost.ToString("N2", CultureInfo.InvariantCulture);

        public string CodeBar
        {
            get => _codeBar;
            set { if (_codeBar != value) { _codeBar = value; OnPropertyChanged(); } }
        }

        public bool IsActive
        {
            get => _isActive;
            set { if (_isActive != value) { _isActive = value; OnPropertyChanged(); } }
        }

        public bool TrackSerial
        {
            get => _trackSerial;
            set { if (_trackSerial != value) { _trackSerial = value; OnPropertyChanged(); } }
        }

        public bool TrackLot
        {
            get => _trackLot;
            set { if (_trackLot != value) { _trackLot = value; OnPropertyChanged(); } }
        }

        public string Unit
        {
            get => _unit;
            set { if (_unit != value) { _unit = value; OnPropertyChanged(); } }
        }

        public double Stock
        {
            get => _stock;
            set { if (_stock != value) { _stock = value; OnPropertyChanged(); } }
        }

        public ImageSource? ImageSource { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}