using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WINUI_TEST.Models;

namespace WINUI_TEST
{
    public sealed partial class ProductsPage : Page
    {
        private readonly List<Product> _allProducts;
        private readonly ObservableCollection<Product> _visibleProducts;
        private bool _isNew;
        private static readonly string[] Categories = { "Etiquetas", "Film", "Cintas", "Insumos", "Empaque" };

        public ProductsPage()
        {
            InitializeComponent();

            ProductBreadcrumb.ItemsSource = new System.Collections.Generic.List<object> { "Productos" };

            _allProducts = new List<Product>
            {
                new Product { ProductId = "P-0001", ProductName = "Etiqueta térmica 40x20", Category = "Etiquetas", Price = 85.50, Cost = 45.00, CodeBar = "7750001000011" },
                new Product { ProductId = "P-0002", ProductName = "Film termocontraíble 12\"", Category = "Film", Price = 320.00, Cost = 210.75, CodeBar = "7750001000028" },
                new Product { ProductId = "P-0003", ProductName = "Cinta adhesiva 48mm", Category = "Cintas", Price = 25.90, Cost = 12.40, CodeBar = "7750001000035" },
                new Product { ProductId = "P-0004", ProductName = "Toner barcode CR70", Category = "Insumos", Price = 410.00, Cost = 350.00, CodeBar = "7750001000042" },
                new Product { ProductId = "P-0005", ProductName = "Totem de cartón 60x40", Category = "Empaque", Price = 95.00, Cost = 60.00, CodeBar = "7750001000059" },
                new Product { ProductId = "P-0006", ProductName = "Etiqueta RFID UHF", Category = "Etiquetas", Price = 145.00, Cost = 88.00, CodeBar = "7750001000066" },
                new Product { ProductId = "P-0007", ProductName = "Film stretch negro 20\"", Category = "Film", Price = 280.00, Cost = 190.25, CodeBar = "7750001000073" },
                new Product { ProductId = "P-0008", ProductName = "Cinta de margen 24mm", Category = "Cintas", Price = 18.75, Cost = 9.80, CodeBar = "7750001000080" },
                new Product { ProductId = "P-0009", ProductName = "Thermal ribbon 110mm", Category = "Insumos", Price = 265.00, Cost = 195.00, CodeBar = "7750001000097" },
                new Product { ProductId = "P-0010", ProductName = "Caja corrugada 40x30x30", Category = "Empaque", Price = 12.50, Cost = 7.20, CodeBar = "7750001000103" },
                new Product { ProductId = "P-0011", ProductName = "Etiqueta térmica 102x152", Category = "Etiquetas", Price = 195.00, Cost = 120.00, CodeBar = "7750001000110" },
                new Product { ProductId = "P-0012", ProductName = "Film adherente 18\"", Category = "Film", Price = 340.00, Cost = 235.50, CodeBar = "7750001000127" },
                new Product { ProductId = "P-0013", ProductName = "Cinta doble cara 12mm", Category = "Cintas", Price = 22.40, Cost = 11.30, CodeBar = "7750001000134" },
                new Product { ProductId = "P-0014", ProductName = "Ribbon resina 60mm", Category = "Insumos", Price = 310.50, Cost = 240.00, CodeBar = "7750001000141" },
                new Product { ProductId = "P-0015", ProductName = "Caja cartón 30x20x10", Category = "Empaque", Price = 8.90, Cost = 4.80, CodeBar = "7750001000158" },
                new Product { ProductId = "P-0016", ProductName = "Etiqueta kraft 50x30", Category = "Etiquetas", Price = 76.00, Cost = 41.50, CodeBar = "7750001000165" },
                new Product { ProductId = "P-0017", ProductName = "Film PE 30\" clear", Category = "Film", Price = 415.00, Cost = 288.75, CodeBar = "7750001000172" },
                new Product { ProductId = "P-0018", ProductName = "Cinta masquing 18mm", Category = "Cintas", Price = 14.60, Cost = 7.90, CodeBar = "7750001000189" },
                new Product { ProductId = "P-0019", ProductName = "Tinta inkjet negra 18ml", Category = "Insumos", Price = 58.00, Cost = 36.40, CodeBar = "7750001000196" },
                new Product { ProductId = "P-0020", ProductName = "Burbuja wrap 50m", Category = "Empaque", Price = 185.00, Cost = 122.00, CodeBar = "7750001000202" },
                new Product { ProductId = "P-0021", ProductName = "Etiqueta poliéster 40x25", Category = "Etiquetas", Price = 132.00, Cost = 84.00, CodeBar = "7750001000219" },
                new Product { ProductId = "P-0022", ProductName = "Film shade 12\" blanco", Category = "Film", Price = 268.00, Cost = 179.90, CodeBar = "7750001000226" },
                new Product { ProductId = "P-0023", ProductName = "Cinta filipack 36mm", Category = "Cintas", Price = 19.30, Cost = 10.10, CodeBar = "7750001000233" },
                new Product { ProductId = "P-0024", ProductName = "Cartón sólido 2mm", Category = "Insumos", Price = 47.50, Cost = 28.70, CodeBar = "7750001000240" },
                new Product { ProductId = "P-0025", ProductName = "Stretch manual 40m", Category = "Empaque", Price = 98.80, Cost = 62.00, CodeBar = "7750001000257" }
            };

            for (int i = 0; i < _allProducts.Count; i++)
            {
                _allProducts[i].Sku = $"SKU-{i + 1:D4}";
                _allProducts[i].Description = "Descripción de referencia del producto";
            }

            _visibleProducts = new ObservableCollection<Product>(_allProducts);
            ProductsList.ItemsSource = _visibleProducts;
            DetailCategoryBox.ItemsSource = Categories;

            UpdateTotals();

            if (_visibleProducts.Count > 0)
            {
                ProductsList.SelectedItem = _visibleProducts[0];
            }
        }

        private void ProductSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ApplyFilter(sender.Text);
        }

        private void ApplyFilter(string? text)
        {
            var query = (text ?? "").Trim();

            _visibleProducts.Clear();
            foreach (var product in _allProducts)
            {
                if (query.Length == 0 || Matches(product, query))
                {
                    _visibleProducts.Add(product);
                }
            }

            UpdateTotals();
        }

        private static bool Matches(Product product, string query)
        {
            return product.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase)
                || product.ProductId.Contains(query, StringComparison.OrdinalIgnoreCase)
                || product.Sku.Contains(query, StringComparison.OrdinalIgnoreCase)
                || product.CodeBar.Contains(query, StringComparison.OrdinalIgnoreCase)
                || product.Category.Contains(query, StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateTotals()
        {
            ProductsCountText.Text = $"{_visibleProducts.Count} productos";
            TotalValueText.Text = $"Valor total: {Sum(_visibleProducts.Select(p => p.Price)):N2}";
        }

        private void ProductsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsList.SelectedItem is not Product product)
            {
                DetailPlaceholder.Visibility = Visibility.Visible;
                DetailFields.Visibility = Visibility.Collapsed;
                return;
            }

            _isNew = false;
            SaveButton.Content = "Guardar";
            LoadDetail(product);
        }

        private void LoadDetail(Product product)
        {
            DetailPlaceholder.Visibility = Visibility.Collapsed;
            DetailFields.Visibility = Visibility.Visible;

            DetailIdBox.Text = product.ProductId;
            DetailNameBox.Text = product.ProductName;
            DetailCodeBarBox.Text = product.CodeBar;
            DetailSkuBox.Text = product.Sku;
            DetailDescriptionBox.Text = product.Description;
            DetailCategoryBox.SelectedItem = product.Category;
            DetailPriceBox.Value = product.Price;
            DetailCostBox.Value = product.Cost;
            DetailUnitBox.Text = product.Unit;
            DetailStockBox.Value = product.Stock;
            DetailActiveSwitch.IsOn = product.IsActive;
            DetailSerialCheck.IsChecked = product.TrackSerial;
            DetailLotCheck.IsChecked = product.TrackLot;

            if (product.ImageSource != null)
            {
                DetailPhotoImage.Source = product.ImageSource;
                DetailPhotoImage.Visibility = Visibility.Visible;
                DetailPhotoPlaceholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                DetailPhotoImage.Source = null;
                DetailPhotoImage.Visibility = Visibility.Collapsed;
                DetailPhotoPlaceholder.Visibility = Visibility.Visible;
            }

            SetDetailEditable(false);
        }

        private void SetDetailEditable(bool editable)
        {
            DetailIdBox.IsEnabled = true;
            DetailNameBox.IsEnabled = true;
            DetailCodeBarBox.IsEnabled = true;
            DetailSkuBox.IsEnabled = true;
            DetailDescriptionBox.IsEnabled = true;
            DetailUnitBox.IsEnabled = true;

            DetailIdBox.IsReadOnly = !editable;
            DetailNameBox.IsReadOnly = !editable;
            DetailCodeBarBox.IsReadOnly = !editable;
            DetailSkuBox.IsReadOnly = !editable;
            DetailDescriptionBox.IsReadOnly = !editable;
            DetailUnitBox.IsReadOnly = !editable;

            DetailCategoryBox.IsEnabled = editable;
            DetailPriceBox.IsEnabled = editable;
            DetailCostBox.IsEnabled = editable;
            DetailStockBox.IsEnabled = editable;
            DetailActiveSwitch.IsEnabled = editable;
            DetailSerialCheck.IsEnabled = editable;
            DetailLotCheck.IsEnabled = editable;
            SaveButton.IsEnabled = editable;
        }

        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            _isNew = true;
            SaveButton.Content = "Guardar nuevo";

            DetailPlaceholder.Visibility = Visibility.Collapsed;
            DetailFields.Visibility = Visibility.Visible;
            SetDetailEditable(true);

            DetailIdBox.Text = NextId();
            DetailNameBox.Text = "";
            DetailCodeBarBox.Text = "";
            DetailSkuBox.Text = "";
            DetailDescriptionBox.Text = "";
            DetailCategoryBox.SelectedItem = null;
            DetailPriceBox.Value = 0;
            DetailCostBox.Value = 0;
            DetailUnitBox.Text = "pieza";
            DetailStockBox.Value = 0;
            DetailActiveSwitch.IsOn = true;
            DetailSerialCheck.IsChecked = false;
            DetailLotCheck.IsChecked = false;

            DetailPhotoImage.Source = null;
            DetailPhotoImage.Visibility = Visibility.Collapsed;
            DetailPhotoPlaceholder.Visibility = Visibility.Visible;

            DetailNameBox.Focus(FocusState.Programmatic);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is not Product product)
            {
                return;
            }

            _isNew = false;
            SaveButton.Content = "Guardar";
            LoadDetail(product);
            SetDetailEditable(true);
            DetailNameBox.Focus(FocusState.Programmatic);
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            await ShowInfoDialog(
                "Importar productos",
                "La importación de productos desde archivo está en construcción. Próximamente podrás importar desde CSV o Excel.");
        }

        private void DuplicateButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is not Product source)
            {
                return;
            }

            var copy = new Product
            {
                ProductId = NextId(),
                ProductName = source.ProductName + " (copia)",
Category = source.Category,
                    CodeBar = source.CodeBar,
                    Sku = source.Sku + "-C",
                    Description = source.Description,
                    Price = source.Price,
                Cost = source.Cost,
                Unit = source.Unit,
                Stock = source.Stock,
                IsActive = source.IsActive,
                TrackSerial = source.TrackSerial,
                TrackLot = source.TrackLot,
                ImageSource = source.ImageSource
            };

            _allProducts.Add(copy);
            ApplyFilter(ProductSearch.Text);
            ProductsList.SelectedItem = copy;
            UpdateTotals();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is not Product product)
            {
                return;
            }

            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = "Eliminar producto",
                Content = $"¿Eliminar \"{product.ProductName}\"? Esta acción no se puede deshacer.",
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            _allProducts.Remove(product);
            _visibleProducts.Remove(product);
            ProductsList.SelectedItem = null;
            UpdateTotals();
        }

        private string NextId()
        {
            int max = 0;
            foreach (var product in _allProducts)
            {
                if (product.ProductId.StartsWith("P-", StringComparison.Ordinal)
                    && int.TryParse(product.ProductId.Substring(2), out int value))
                {
                    max = Math.Max(max, value);
                }
            }

            return $"P-{max + 1:D4}";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isNew)
            {
                var product = new Product
                {
                    ProductId = string.IsNullOrWhiteSpace(DetailIdBox.Text) ? NextId() : DetailIdBox.Text.Trim(),
                    ProductName = DetailNameBox.Text,
                    CodeBar = DetailCodeBarBox.Text,
                    Sku = DetailSkuBox.Text,
                    Description = DetailDescriptionBox.Text,
                    Category = DetailCategoryBox.SelectedItem as string ?? "",
                    Price = DetailPriceBox.Value,
                    Cost = DetailCostBox.Value,
                    Unit = DetailUnitBox.Text,
                    Stock = DetailStockBox.Value,
                    IsActive = DetailActiveSwitch.IsOn,
                    TrackSerial = DetailSerialCheck.IsChecked == true,
                    TrackLot = DetailLotCheck.IsChecked == true
                };

                _allProducts.Add(product);
                ApplyFilter(ProductSearch.Text);
                ProductsList.SelectedItem = product;

                _isNew = false;
                SaveButton.Content = "Guardar";
            }
            else if (ProductsList.SelectedItem is Product selected)
            {
                selected.ProductId = DetailIdBox.Text;
                selected.ProductName = DetailNameBox.Text;
                selected.CodeBar = DetailCodeBarBox.Text;
                selected.Sku = DetailSkuBox.Text;
                selected.Description = DetailDescriptionBox.Text;
                selected.Category = DetailCategoryBox.SelectedItem as string ?? selected.Category;
                selected.Price = DetailPriceBox.Value;
                selected.Cost = DetailCostBox.Value;
                selected.Unit = DetailUnitBox.Text;
                selected.Stock = DetailStockBox.Value;
                selected.IsActive = DetailActiveSwitch.IsOn;
                selected.TrackSerial = DetailSerialCheck.IsChecked == true;
                selected.TrackLot = DetailLotCheck.IsChecked == true;
            }

            SetDetailEditable(false);
            UpdateTotals();
        }

        private async Task ShowInfoDialog(string title, string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = title,
                Content = message,
                CloseButtonText = "Aceptar"
            };

            await dialog.ShowAsync();
        }

        private static double Sum(IEnumerable<double> values)
        {
            double total = 0;
            foreach (var value in values)
            {
                total += value;
            }

            return total;
        }
    }
}