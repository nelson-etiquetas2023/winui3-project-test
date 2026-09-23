using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Foundation;
using WINUI_TEST.Models;
using WINUI_TEST.Services;

namespace WINUI_TEST
{
    public sealed partial class SystemSettingsPage : Page
    {
        private readonly ObservableCollection<ProductUnit> _units = new()
        {
            new ProductUnit { Name = "Pieza", Abbreviation = "PZA", Description = "Unidad básica de venta" },
            new ProductUnit { Name = "Caja", Abbreviation = "CJA", Description = "Caja completa de producto" },
            new ProductUnit { Name = "Kilogramo", Abbreviation = "KG", Description = "Peso en kilogramos" },
            new ProductUnit { Name = "Rollo", Abbreviation = "RLL", Description = "Rollo de material" },
            new ProductUnit { Name = "Metro", Abbreviation = "MTR", Description = "Longitud en metros" },
            new ProductUnit { Name = "Docena", Abbreviation = "DOC", Description = "Paquete de doce unidades" }
        };

        private readonly ObservableCollection<ProductCategory> _categories = new()
        {
            new ProductCategory { Name = "Etiquetas", Description = "Etiquetas térmicas, kraft y RFID" },
            new ProductCategory { Name = "Film", Description = "Films termocontraíbles y estirables" },
            new ProductCategory { Name = "Cintas", Description = "Cintas adhesivas y de margen" },
            new ProductCategory { Name = "Insumos", Description = "Toner, ribbons y tintas" },
            new ProductCategory { Name = "Empaque", Description = "Cajas, totems y burbuja" }
        };

        private readonly ObservableCollection<Warehouse> _warehouses = new()
        {
            new Warehouse { Name = "Almacén Central", Location = "Sede principal", Description = "Bodega principal de la compañía" },
            new Warehouse { Name = "Almacén Norte", Location = "Sucursal norte" },
            new Warehouse { Name = "Punto de venta 1", Location = "Tienda centro" }
        };

        private readonly ObservableCollection<WarehouseLocation> _locations = new()
        {
            new WarehouseLocation { Name = "Rack A-01", Description = "Estantería alta, sección A" },
            new WarehouseLocation { Name = "Rack A-02", Description = "Estantería alta, sección A" },
            new WarehouseLocation { Name = "Pasillo 3, nivel 2", Description = "Zona de etiquetas" },
            new WarehouseLocation { Name = "Muelle de carga", Description = "Área de recepción y despacho" },
            new WarehouseLocation { Name = "Zona devoluciones", Description = "Fuera de stock válido" }
        };

        private Expander? _activeExpander;

        public SystemSettingsPage()
        {
            InitializeComponent();
            CatalogBreadcrumb.ItemsSource = new System.Collections.Generic.List<object> { "Configuración" };

            UnitsList.ItemsSource = _units;
            CategoriesList.ItemsSource = _categories;
            WarehousesList.ItemsSource = _warehouses;
            LocationsList.ItemsSource = _locations;

            LoadCompanyData();

            UpdateCatalogUi();
        }

        // ---------------- Empresa ----------------

        private async void LoadCompanyData()
        {
            var company = CompanyStore.Current;
            CompanyNameBox.Text = company.Name;
            CompanyAddressBox.Text = company.Address;
            CompanyItbisBox.Text = company.Itbis;

            var logo = await CompanyStore.EnsureLogoAsync();
            if (logo != null)
            {
                CompanyLogoEdit.Source = logo;
            }
        }

        private void SaveCompany_Click(object sender, RoutedEventArgs e)
        {
            var company = CompanyStore.Current;
            company.Name = CompanyNameBox.Text.Trim();
            company.Address = CompanyAddressBox.Text.Trim();
            company.Itbis = CompanyItbisBox.Text.Trim();
            CompanyStore.RaiseCompanyInfoChanged();

            ShowStatus("Datos de la empresa guardados.");
        }

        private bool _mapLoaded;

        private void CompanyExpander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (_mapLoaded)
            {
                return;
            }

            _mapLoaded = true;
            LoadCompanyMap();
        }

        private void LoadCompanyMap()
        {
            const double zoom = 15;
            const double lat = 18.4693;
            const double lon = -69.9382;

            double n = Math.Pow(2, zoom);
            double xt = n * (lon + 180) / 360;
            double latRad = lat * Math.PI / 180;
            double yt = n * (1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2;
            int xtI = (int)Math.Floor(xt);
            int ytI = (int)Math.Floor(yt);

            double w = CompanyMapHost.ActualWidth;
            double h = CompanyMapHost.ActualHeight;
            if (w <= 0) w = 420;
            if (h <= 0) h = 320;

            double xOffset = w / 2 - xt * 256;
            double yOffset = h / 2 - yt * 256;

            CompanyMapCanvas.Children.Clear();

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int tileX = xtI + dx;
                    int tileY = ytI + dy;
                    if (tileX < 0 || tileY < 0)
                    {
                        continue;
                    }

                    var img = new Image
                    {
                        Width = 256,
                        Height = 256,
                        Stretch = Stretch.Uniform
                    };
                    Canvas.SetLeft(img, tileX * 256 + xOffset);
                    Canvas.SetTop(img, tileY * 256 + yOffset);
                    img.Source = new BitmapImage(new Uri($"https://mt1.google.com/vt/lyrs=m&x={tileX}&y={tileY}&z={zoom}"));
                    CompanyMapCanvas.Children.Add(img);
                }
            }

            UpdateMapClip();
        }

        private void CompanyMapHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateMapClip();
        }

        private void UpdateMapClip()
        {
            if (CompanyMapHost.ActualWidth <= 0 || CompanyMapHost.ActualHeight <= 0)
            {
                return;
            }

            CompanyMapCanvas.Clip = new RectangleGeometry
            {
                Rect = new Rect(0, 0, CompanyMapHost.ActualWidth, CompanyMapHost.ActualHeight)
            };
        }

        private async void ChangeLogo_Click(object sender, RoutedEventArgs e)
        {
            var hwnd = MainWindow.Instance != null
                ? Microsoft.UI.Win32Interop.GetWindowFromWindowId(MainWindow.Instance.AppWindow.Id)
                : IntPtr.Zero;

            if (await CompanyStore.PickAndSetLogoAsync(hwnd))
            {
                CompanyLogoEdit.Source = CompanyStore.Current.Logo;
                MainWindow.Instance?.RefreshCompanyLogo();
                ShowStatus("Logo de la empresa actualizado.");
            }
        }

        // ---------------- Catálogos: agregar ----------------

        private async void AddUnitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await ShowItemEditorDialog(
                "Nueva unidad",
                "Abreviatura",
                new ItemEditorResult(),
                name => ValidateUnique(_units, null, name));

            if (result == null)
            {
                return;
            }

            _units.Add(new ProductUnit
            {
                Name = result.Name,
                Abbreviation = result.Secondary,
                Description = result.Description,
                IsActive = result.IsActive
            });

            UpdateCatalogUi();
            ShowStatus($"Unidad \"{result.Name}\" agregada.");
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await ShowItemEditorDialog(
                "Nueva categoría",
                null,
                new ItemEditorResult(),
                name => ValidateUnique(_categories, null, name));

            if (result == null)
            {
                return;
            }

            _categories.Add(new ProductCategory
            {
                Name = result.Name,
                Description = result.Description,
                IsActive = result.IsActive
            });

            UpdateCatalogUi();
            ShowStatus($"Categoría \"{result.Name}\" agregada.");
        }

        private async void AddWarehouseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await ShowItemEditorDialog(
                "Nuevo almacén",
                "Ubicación",
                new ItemEditorResult(),
                name => ValidateUnique(_warehouses, null, name));

            if (result == null)
            {
                return;
            }

            _warehouses.Add(new Warehouse
            {
                Name = result.Name,
                Location = result.Secondary,
                Description = result.Description,
                IsActive = result.IsActive
            });

            UpdateCatalogUi();
            ShowStatus($"Almacén \"{result.Name}\" agregado.");
        }

        private async void AddLocationButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await ShowItemEditorDialog(
                "Nueva ubicación",
                null,
                new ItemEditorResult(),
                name => ValidateUnique(_locations, null, name));

            if (result == null)
            {
                return;
            }

            _locations.Add(new WarehouseLocation
            {
                Name = result.Name,
                Description = result.Description,
                IsActive = result.IsActive
            });

            UpdateCatalogUi();
            ShowStatus($"Ubicación \"{result.Name}\" agregada.");
        }

        // ---------------- Editar (genérico por tipo) ----------------

        private async void EditItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.Tag is null)
            {
                return;
            }

            switch (element.Tag)
            {
                case ProductUnit unit:
                    await EditUnitAsync(unit);
                    break;
                case ProductCategory category:
                    await EditCategoryAsync(category);
                    break;
                case Warehouse warehouse:
                    await EditWarehouseAsync(warehouse);
                    break;
                case WarehouseLocation location:
                    await EditLocationAsync(location);
                    break;
            }
        }

        private async Task EditUnitAsync(ProductUnit unit)
        {
            var result = await ShowItemEditorDialog(
                "Editar unidad",
                "Abreviatura",
                ToResult(unit.Name, unit.Abbreviation, unit.Description, unit.IsActive),
                name => ValidateUnique(_units, unit, name));

            if (result == null)
            {
                return;
            }

            unit.Name = result.Name;
            unit.Abbreviation = result.Secondary;
            unit.Description = result.Description;
            unit.IsActive = result.IsActive;

            UpdateCatalogUi();
            ShowStatus($"Unidad \"{unit.Name}\" actualizada.");
        }

        private async Task EditCategoryAsync(ProductCategory category)
        {
            var result = await ShowItemEditorDialog(
                "Editar categoría",
                null,
                ToResult(category.Name, null, category.Description, category.IsActive),
                name => ValidateUnique(_categories, category, name));

            if (result == null)
            {
                return;
            }

            category.Name = result.Name;
            category.Description = result.Description;
            category.IsActive = result.IsActive;

            UpdateCatalogUi();
            ShowStatus($"Categoría \"{category.Name}\" actualizada.");
        }

        private async Task EditWarehouseAsync(Warehouse warehouse)
        {
            var result = await ShowItemEditorDialog(
                "Editar almacén",
                "Ubicación",
                ToResult(warehouse.Name, warehouse.Location, warehouse.Description, warehouse.IsActive),
                name => ValidateUnique(_warehouses, warehouse, name));

            if (result == null)
            {
                return;
            }

            warehouse.Name = result.Name;
            warehouse.Location = result.Secondary;
            warehouse.Description = result.Description;
            warehouse.IsActive = result.IsActive;

            UpdateCatalogUi();
            ShowStatus($"Almacén \"{warehouse.Name}\" actualizado.");
        }

        private async Task EditLocationAsync(WarehouseLocation location)
        {
            var result = await ShowItemEditorDialog(
                "Editar ubicación",
                null,
                ToResult(location.Name, null, location.Description, location.IsActive),
                name => ValidateUnique(_locations, location, name));

            if (result == null)
            {
                return;
            }

            location.Name = result.Name;
            location.Description = result.Description;
            location.IsActive = result.IsActive;

            UpdateCatalogUi();
            ShowStatus($"Ubicación \"{location.Name}\" actualizada.");
        }

        // ---------------- Eliminar (genérico por tipo) ----------------

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element || element.Tag is null)
            {
                return;
            }

            switch (element.Tag)
            {
                case ProductUnit unit:
                    await DeleteWithConfirmationAsync(
                        "Eliminar unidad",
                        $"¿Eliminar la unidad \"{unit.Name}\"? Esta acción no se puede deshacer.",
                        () => _units.Remove(unit),
                        "unidad");
                    break;
                case ProductCategory category:
                    await DeleteWithConfirmationAsync(
                        "Eliminar categoría",
                        $"¿Eliminar la categoría \"{category.Name}\"? Esta acción no se puede deshacer.",
                        () => _categories.Remove(category),
                        "categoría");
                    break;
                case Warehouse warehouse:
                    await DeleteWithConfirmationAsync(
                        "Eliminar almacén",
                        $"¿Eliminar el almacén \"{warehouse.Name}\"? Esta acción no se puede deshacer.",
                        () => _warehouses.Remove(warehouse),
                        "almacén");
                    break;
                case WarehouseLocation location:
                    await DeleteWithConfirmationAsync(
                        "Eliminar ubicación",
                        $"¿Eliminar la ubicación \"{location.Name}\"? Esta acción no se puede deshacer.",
                        () => _locations.Remove(location),
                        "ubicación");
                    break;
            }
        }

        private async Task DeleteWithConfirmationAsync(string title, string message, Action remove, string kind)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = title,
                Content = message,
                PrimaryButtonText = "Eliminar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            remove();
            UpdateCatalogUi();
            ShowStatus($"{kind[0].ToString().ToUpperInvariant()}{kind.Substring(1)} eliminado.");
        }

        // ---------------- Diálogo genérico de edición ----------------

        private async Task<ItemEditorResult?> ShowItemEditorDialog(
            string title,
            string? secondaryHeader,
            ItemEditorResult initial,
            Func<string, string?> validator)
        {
            var nameBox = new TextBox { Header = "Nombre", Text = initial.Name };
            TextBox? secondaryBox = null;
            if (secondaryHeader != null)
            {
                secondaryBox = new TextBox { Header = secondaryHeader, Text = initial.Secondary };
            }

            var descriptionBox = new TextBox
            {
                Header = "Descripción (opcional)",
                Text = initial.Description,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 64
            };

            var activeSwitch = new ToggleSwitch
            {
                Header = "Activo",
                OnContent = "",
                OffContent = "",
                IsOn = initial.IsActive
            };

            var errorText = new TextBlock
            {
                Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["SystemFillColorCriticalBrush"],
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed
            };

            var panel = new StackPanel { Spacing = 12, MinWidth = 340 };
            panel.Children.Add(nameBox);
            if (secondaryBox != null)
            {
                panel.Children.Add(secondaryBox);
            }
            panel.Children.Add(descriptionBox);
            panel.Children.Add(activeSwitch);
            panel.Children.Add(errorText);

            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = title,
                Content = panel,
                PrimaryButtonText = "Guardar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary
            };

            dialog.PrimaryButtonClick += (s, args) =>
            {
                var error = validator(nameBox.Text.Trim());
                if (error != null)
                {
                    errorText.Text = error;
                    errorText.Visibility = Visibility.Visible;
                    args.Cancel = true;
                    return;
                }

                errorText.Visibility = Visibility.Collapsed;
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return null;
            }

            return new ItemEditorResult
            {
                Name = nameBox.Text.Trim(),
                Secondary = secondaryBox?.Text.Trim() ?? "",
                Description = descriptionBox.Text.Trim(),
                IsActive = activeSwitch.IsOn
            };
        }

        // ---------------- Helpers ----------------

        private static ItemEditorResult ToResult(string name, string? secondary, string description, bool isActive)
            => new()
            {
                Name = name,
                Secondary = secondary ?? "",
                Description = description,
                IsActive = isActive
            };

        private static string? ValidateUnique<T>(ObservableCollection<T> items, T? current, string name)
            where T : CatalogEntity
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "El nombre es obligatorio.";
            }

            foreach (var item in items)
            {
                if (!ReferenceEquals(item, current)
                    && string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return "Ya existe un elemento con ese nombre.";
                }
            }

            return null;
        }

        private void UpdateCatalogUi()
        {
            UnitsCountText.Text = $"{_units.Count} unidades";
            CategoriesCountText.Text = $"{_categories.Count} categorías";
            WarehousesCountText.Text = $"{_warehouses.Count} almacenes";
            LocationsCountText.Text = $"{_locations.Count} ubicaciones";

            UnitsEmptyText.Visibility = _units.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            CategoriesEmptyText.Visibility = _categories.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            WarehousesEmptyText.Visibility = _warehouses.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            LocationsEmptyText.Visibility = _locations.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ShowStatus(string message)
        {
            StatusInfoBar.Severity = InfoBarSeverity.Success;
            StatusInfoBar.Title = message;
            StatusInfoBar.IsOpen = true;

            await Task.Delay(4000);
            StatusInfoBar.IsOpen = false;
        }

        private sealed class ItemEditorResult
        {
            public string Name { get; set; } = "";
            public string Secondary { get; set; } = "";
            public string Description { get; set; } = "";
            public bool IsActive { get; set; } = true;
        }

        // ---------------- Búsqueda por categoría/subcategoría ----------------

        private void ConfigSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var query = (sender.Text ?? "").Trim();

            bool showUnits = MatchesSearch("Unidades de producto", "unidad unidades productos medida pieza abreviatura", query);
            bool showCategories = MatchesSearch("Categorías", "categoria categorias clasificar productos", query);
            bool showWarehouses = MatchesSearch("Almacenes", "almacen almacenes inventario stock ubicacion bodega punto venta", query);
            bool showLocations = MatchesSearch("Ubicaciones de almacén", "ubicacion ubicaciones rack pasillo zona muelle inventario stock", query);
            bool showCompany = MatchesSearch("Datos de la empresa", "empresa compania nombre direccion itbis rnc logo map ubicacion", query);

            UnitsExpander.Visibility = showUnits ? Visibility.Visible : Visibility.Collapsed;
            CategoryExpander.Visibility = showCategories ? Visibility.Visible : Visibility.Collapsed;
            WarehouseExpander.Visibility = showWarehouses ? Visibility.Visible : Visibility.Collapsed;
            LocationExpander.Visibility = showLocations ? Visibility.Visible : Visibility.Collapsed;
            CompanyExpander.Visibility = showCompany ? Visibility.Visible : Visibility.Collapsed;

            DataSectionHeader.Visibility = (showUnits || showCategories) ? Visibility.Visible : Visibility.Collapsed;
            InventorySectionHeader.Visibility = (showWarehouses || showLocations) ? Visibility.Visible : Visibility.Collapsed;
            CompanySectionHeader.Visibility = showCompany ? Visibility.Visible : Visibility.Collapsed;

            if (query.Length == 0)
            {
                return;
            }

            Expander? firstMatch = showUnits ? UnitsExpander
                : showCategories ? CategoryExpander
                : showWarehouses ? WarehouseExpander
                : showLocations ? LocationExpander
                : showCompany ? CompanyExpander
                : null;

            if (firstMatch != null && !firstMatch.IsExpanded)
            {
                firstMatch.IsExpanded = true;
            }
        }

        private static bool MatchesSearch(string title, string keywords, string query)
        {
            if (query.Length == 0)
            {
                return true;
            }

            var haystack = (title + " " + keywords).ToLowerInvariant();
            return haystack.Contains(query.ToLowerInvariant(), StringComparison.Ordinal);
        }

        // ---------------- Breadcrumb / acordeón ----------------

        private void CatalogExpander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (ReferenceEquals(sender, CompanyExpander))
            {
                CompanyExpander_Expanding(sender, args);
            }

            if (_activeExpander != null && !ReferenceEquals(_activeExpander, sender))
            {
                _activeExpander.IsExpanded = false;
            }

            _activeExpander = sender;
            UpdateBreadcrumb();
        }

        private void CatalogExpander_Collapsed(Expander sender, ExpanderCollapsedEventArgs args)
        {
            if (ReferenceEquals(_activeExpander, sender))
            {
                _activeExpander = null;
            }

            UpdateBreadcrumb();
        }

        private void CatalogBreadcrumb_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Index == 0)
            {
                if (_activeExpander != null)
                {
                    _activeExpander.IsExpanded = false;
                }

                return;
            }

            ActivateSectionByLabel(args.Item?.ToString() ?? "");
        }

        public void ActivateSectionByLabel(string label)
        {
            Expander? target = FindExpanderByTitle(label);
            if (target != null && !target.IsExpanded)
            {
                target.IsExpanded = true;
            }
        }

        public void CollapseActiveSection()
        {
            if (_activeExpander != null)
            {
                _activeExpander.IsExpanded = false;
            }
        }

        public string? GetActiveSectionLabel()
        {
            if (_activeExpander == null)
            {
                return null;
            }

            return _activeExpander.Name switch
            {
                nameof(UnitsExpander) => "Unidades de producto",
                nameof(WarehouseExpander) => "Almacenes",
                nameof(CategoryExpander) => "Categorías",
                nameof(LocationExpander) => "Ubicaciones de almacén",
                nameof(CompanyExpander) => "Datos de la empresa",
                _ => _activeExpander.Name
            };
        }

        private Expander? FindExpanderByTitle(string title)
        {
            return title switch
            {
                "Unidades de producto" => UnitsExpander,
                "Categorías" => CategoryExpander,
                "Almacenes" => WarehouseExpander,
                "Ubicaciones de almacén" => LocationExpander,
                "Datos de la empresa" => CompanyExpander,
                _ => null
            };
        }

        private void UpdateBreadcrumb()
        {
            var items = new System.Collections.Generic.List<object> { "Configuración" };
            string? activeSection = GetActiveSectionLabel();

            if (activeSection != null)
            {
                items.Add(activeSection);
            }

            CatalogBreadcrumb.ItemsSource = items;

            MainWindow.Instance?.NotifySectionChanged(activeSection);
        }
    }
}