using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics;
using WINUI_TEST.Services;

namespace WINUI_TEST
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        private readonly List<TrailEntry> _trail = new();

        private sealed class TrailEntry
        {
            public string Key { get; set; } = "";
            public string Label { get; set; } = "";
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            // DPI-aware window sizing per WinUI 3 rubric
            // Get DPI using GetDpiForWindow from user32.dll
            var hwnd = GetForegroundWindow();
            var dpi = GetDpiForWindow(hwnd);
            var scale = dpi / 96.0;

            // Single-purpose utility: ~520 DIP wide, ~600 DIP tall
            // Rubric: Width = widest row + 48 padding, Height = 32(titlebar) + content + spacing + 48 padding
            int widthDip = 1100;
            int heightDip = 760;

            AppWindow.Resize(new SizeInt32((int)(widthDip * scale), (int)(heightDip * scale)));

            SetAppWindowIcon();
            _ = LoadCompanyLogoAsync();
            CompanyPaneName.Text = CompanyStore.Current.Name;
            CompanyStore.CompanyInfoChanged += (_, _) =>
            {
                CompanyPaneName.Text = CompanyStore.Current.Name;
                RefreshCompanyLogo();
            };

            UserSession.SessionChanged += (_, _) => UpdateUserArea();
            UpdateUserArea();

            if (Content is FrameworkElement root)
            {
                ThemeSwitch.IsOn = root.ActualTheme == ElementTheme.Dark;
            }
        }

        private void ThemeSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (Content is FrameworkElement root)
            {
                root.RequestedTheme = ThemeSwitch.IsOn ? ElementTheme.Dark : ElementTheme.Light;
            }
        }

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            bool compact = sender.DisplayMode == NavigationViewDisplayMode.Compact
                        || sender.DisplayMode == NavigationViewDisplayMode.Minimal;

            UserTexts.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateUserArea()
        {
            var user = UserSession.Current;

            if (user == null)
            {
                UserAvatar.Initials = "?";
                UserNameText.Text = "Iniciar sesión";
                UserEmailText.Text = "Sin sesión activa";
                FlyoutLogin.Visibility = Visibility.Visible;
                FlyoutLogout.Visibility = Visibility.Collapsed;
                return;
            }

            UserAvatar.Initials = user.Initials;
            UserNameText.Text = user.Name;
            UserEmailText.Text = user.Email;
            FlyoutLogin.Visibility = Visibility.Collapsed;
            FlyoutLogout.Visibility = Visibility.Visible;
        }

        private async void LoginMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var emailBox = new TextBox { Header = "Correo o usuario", PlaceholderText = "admin", Margin = new Thickness(0, 0, 0, 8) };
            var passBox = new PasswordBox { Header = "Contraseña", PlaceholderText = "admin" };
            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(emailBox);
            panel.Children.Add(passBox);

            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Iniciar sesión",
                Content = panel,
                PrimaryButtonText = "Entrar",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            if (!UserSession.Login(emailBox.Text, passBox.Password))
            {
                await ShowErrorDialog("Usuario o contraseña incorrectos.\nPrueba con admin / admin.");
            }
        }

        private async void CreateAccountMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var nameBox = new TextBox { Header = "Nombre completo", Margin = new Thickness(0, 0, 0, 8) };
            var emailBox = new TextBox { Header = "Correo", PlaceholderText = "correo@empresa.com", Margin = new Thickness(0, 0, 0, 8) };
            var passBox = new PasswordBox { Header = "Contraseña", Margin = new Thickness(0, 0, 0, 8) };
            var confirmBox = new PasswordBox { Header = "Confirmar contraseña" };
            var panel = new StackPanel { Spacing = 8 };
            panel.Children.Add(nameBox);
            panel.Children.Add(emailBox);
            panel.Children.Add(passBox);
            panel.Children.Add(confirmBox);

            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Crear cuenta",
                Content = panel,
                PrimaryButtonText = "Crear",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Primary
            };

            if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            {
                return;
            }

            if (passBox.Password != confirmBox.Password)
            {
                await ShowErrorDialog("Las contraseñas no coinciden.");
                return;
            }

            if (!UserSession.Register(nameBox.Text, emailBox.Text, passBox.Password))
            {
                await ShowErrorDialog("Datos incompletos o el correo ya está registrado.");
            }
        }

        private void LogoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Logout();
        }

        private async Task ShowErrorDialog(string message)
        {
            var dialog = new ContentDialog
            {
                XamlRoot = Content.XamlRoot,
                Title = "Error",
                Content = message,
                CloseButtonText = "Aceptar"
            };

            await dialog.ShowAsync();
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // Select the first navigation item by default (Dashboard)
            NavDashboard.IsSelected = true;

            if (NavView.SettingsItem is NavigationViewItem settingsItem)
            {
                settingsItem.Content = "Configuración";
            }
        }

        private void ToolbarNav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not AppBarButton button || button.Tag is not string key)
            {
                return;
            }

            if (key == "NavSettings")
            {
                OpenSettingsTab();
                return;
            }

            NavigationViewItem? target = key switch
            {
                nameof(NavOrdenesCompra) => NavOrdenesCompra,
                nameof(NavPedidos) => NavPedidos,
                nameof(NavProduccion) => NavProduccion,
                nameof(NavProductos) => NavProductos,
                nameof(NavInventario) => NavInventario,
                nameof(NavDespacho) => NavDespacho,
                nameof(NavUsuarios) => NavUsuarios,
                nameof(NavClientes) => NavClientes,
                nameof(NavProveedores) => NavProveedores,
                _ => null
            };

            if (target == null)
            {
                return;
            }

            NavView.SelectedItem = target;
            OpenModuleTab(target);
        }

        private void ModuleSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var query = (sender.Text ?? "").Trim();
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem)
                {
                    ApplyFilter(navItem, query);
                }
            }
        }

        private bool ApplyFilter(NavigationViewItem item, string query)
        {
            bool selfMatch = query.Length == 0 || (item.Content?.ToString()?.IndexOf(query, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
            bool childMatch = false;

            if (item.MenuItems != null)
            {
                foreach (var child in item.MenuItems)
                {
                    if (child is NavigationViewItem childItem)
                    {
                        childMatch |= ApplyFilter(childItem, query);
                    }
                }
            }

            bool show = selfMatch || childMatch;
            item.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

            if (item.MenuItems != null && item.MenuItems.Count > 0)
            {
                item.IsExpanded = query.Length > 0 && (selfMatch || childMatch);
            }

            return show;
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                OpenSettingsTab();
                return;
            }

            var selectedItem = args.SelectedItem as NavigationViewItem;
            if (selectedItem == null)
            {
                return;
            }

            // Group headers (Ventas / Compras) only expand; they don't open a tab
            if (selectedItem.MenuItems != null && selectedItem.MenuItems.Count > 0)
            {
                return;
            }

            OpenModuleTab(selectedItem);
        }

        private void OpenModuleTab(NavigationViewItem navItem)
        {
            var moduleKey = navItem.Name;
            var title = navItem.Content as string ?? moduleKey;

            foreach (var tab in MainTabs.TabItems)
            {
                if (tab is TabViewItem existingTab && (existingTab.Tag as string) == moduleKey)
                {
                    MainTabs.SelectedItem = existingTab;
                    return;
                }
            }

            var tabItem = new TabViewItem
            {
                Header = CreateTabHeader(title),
                Tag = moduleKey,
                IsClosable = true
            };

            if (navItem.Icon is SymbolIcon symbolIcon)
            {
                tabItem.IconSource = new SymbolIconSource { Symbol = symbolIcon.Symbol };
            }

            var frame = new Frame();
            tabItem.Content = frame;
            MainTabs.TabItems.Add(tabItem);
            frame.Navigate(moduleKey == nameof(NavProductos) ? typeof(ProductsPage)
                : typeof(ModulePage), title);

            MainTabs.SelectedItem = tabItem;
            tabItem.IsSelected = true;

            if (MainTabs.Visibility != Visibility.Visible)
            {
                MainTabs.Visibility = Visibility.Visible;
                WelcomeText.Visibility = Visibility.Collapsed;
            }

            TabsMoreButton.Visibility = Visibility.Visible;
            HeaderTitleText.Text = title;
            ResetTrail(moduleKey, title);
        }

        private void OpenSettingsTab()
        {
            const string moduleKey = "NavSettings";
            const string title = "Configuración";

            foreach (var tab in MainTabs.TabItems)
            {
                if (tab is TabViewItem existingTab && (existingTab.Tag as string) == moduleKey)
                {
                    MainTabs.SelectedItem = existingTab;
                    return;
                }
            }

            var tabItem = new TabViewItem
            {
                Header = CreateTabHeader(title),
                Tag = moduleKey,
                IsClosable = true,
                IconSource = new SymbolIconSource { Symbol = Symbol.Setting }
            };

            var frame = new Frame();
            tabItem.Content = frame;
            MainTabs.TabItems.Add(tabItem);
            frame.Navigate(typeof(SystemSettingsPage), title);

            MainTabs.SelectedItem = tabItem;
            tabItem.IsSelected = true;

            if (MainTabs.Visibility != Visibility.Visible)
            {
                MainTabs.Visibility = Visibility.Visible;
                WelcomeText.Visibility = Visibility.Collapsed;
            }

            TabsMoreButton.Visibility = Visibility.Visible;
            HeaderTitleText.Text = title;
            ResetTrail(moduleKey, title);
        }

        private void MainTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateActiveTabHighlight();

            if (MainTabs.SelectedItem is TabViewItem activeTab && activeTab.Tag is string moduleKey)
            {
                if (activeTab.Header is StackPanel headerPanel && headerPanel.Children.Count > 0 && headerPanel.Children[0] is TextBlock headerText)
                {
                    HeaderTitleText.Text = headerText.Text;
                }

                var navItem = FlattenMenuItems(NavView.MenuItems).FirstOrDefault(i => i.Name == moduleKey);
                if (navItem != null && !ReferenceEquals(NavView.SelectedItem, navItem))
                {
                    NavView.SelectedItem = navItem;
                }

                if (activeTab.Header is StackPanel crumbPanel && crumbPanel.Children.Count > 0 && crumbPanel.Children[0] is TextBlock crumbText)
                {
                    ResetTrail(moduleKey, crumbText.Text);
                    SyncCurrentPageSection();
                }
            }
        }

        private void UpdateActiveTabHighlight()
        {
            var primaryBrush = ThemeBrush("TextFillColorPrimaryBrush");
            var accentBrush = ThemeBrush("AccentFillColorDefaultBrush");
            var transparentBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            foreach (var tab in MainTabs.TabItems)
            {
                if (tab is not TabViewItem tabItem || tabItem.Header is not StackPanel headerPanel || headerPanel.Children.Count < 2)
                {
                    continue;
                }

                bool isActive = ReferenceEquals(MainTabs.SelectedItem, tabItem);

                if (headerPanel.Children[0] is TextBlock headerText)
                {
                    headerText.Foreground = primaryBrush;
                    headerText.FontWeight = isActive ? FontWeights.SemiBold : FontWeights.Normal;
                    headerText.Opacity = isActive ? 1.0 : 0.72;
                }

                if (headerPanel.Children[1] is Rectangle underline)
                {
                    underline.Fill = isActive ? accentBrush ?? transparentBrush : transparentBrush;
                }
            }
        }

        private static StackPanel CreateTabHeader(string title)
        {
            var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 2 };

            var titleText = new TextBlock { Text = title };
            panel.Children.Add(titleText);

            var underline = new Rectangle
            {
                Height = 3,
                RadiusX = 1.5,
                RadiusY = 1.5,
                Fill = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
            };
            panel.Children.Add(underline);

            return panel;
        }

        private static Brush? ThemeBrush(string resourceName)
        {
            if (Application.Current.Resources.TryGetValue(resourceName, out var brush))
            {
                return brush as Brush;
            }

            return null;
        }

        private static IEnumerable<NavigationViewItem> FlattenMenuItems(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                if (item is not NavigationViewItem navItem)
                {
                    continue;
                }

                yield return navItem;

                if (navItem.MenuItems != null)
                {
                    foreach (var child in FlattenMenuItems(navItem.MenuItems))
                    {
                        yield return child;
                    }
                }
            }
        }

        private void MainTabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);

            if (sender.TabItems.Count == 0)
            {
                ResetTrailToHome();
                MainTabs.Visibility = Visibility.Collapsed;
                WelcomeText.Visibility = Visibility.Visible;
                TabsMoreButton.Visibility = Visibility.Collapsed;
                HeaderTitleText.Text = "WINUI-TEST";
            }
        }

        private void CloseAllTabs_Click(object sender, RoutedEventArgs e)
        {
            MainTabs.TabItems.Clear();
            ResetTrailToHome();
            MainTabs.Visibility = Visibility.Collapsed;
            WelcomeText.Visibility = Visibility.Visible;
            TabsMoreButton.Visibility = Visibility.Collapsed;
            HeaderTitleText.Text = "WINUI-TEST";
        }

        public void NotifySectionChanged(string? sectionLabel)
        {
            if (_trail.Count < 2)
            {
                return;
            }

            if (string.IsNullOrEmpty(sectionLabel))
            {
                if (_trail[^1].Key.StartsWith("section:", StringComparison.Ordinal))
                {
                    _trail.RemoveAt(_trail.Count - 1);
                    RefreshGlobalBreadcrumb();
                }

                return;
            }

            string key = "section:" + sectionLabel;

            if (_trail[^1].Key.StartsWith("section:", StringComparison.Ordinal))
            {
                _trail[^1] = new TrailEntry { Key = key, Label = sectionLabel };
            }
            else
            {
                _trail.Add(new TrailEntry { Key = key, Label = sectionLabel });
            }

            RefreshGlobalBreadcrumb();
        }

        private void ResetTrail(string moduleKey, string label)
        {
            _trail.Clear();
            _trail.Add(new TrailEntry { Key = "Home", Label = "Home" });

            if (!string.IsNullOrEmpty(moduleKey))
            {
                _trail.Add(new TrailEntry { Key = moduleKey, Label = label });
            }

            RefreshGlobalBreadcrumb();
        }

        private void ResetTrailToHome()
        {
            _trail.Clear();
            _trail.Add(new TrailEntry { Key = "Home", Label = "Home" });
            RefreshGlobalBreadcrumb();
        }

        private void RefreshGlobalBreadcrumb()
        {
            GlobalBreadcrumb.ItemsSource = _trail.Select(e => (object)e.Label).ToList();
            GlobalCrumbBar.Visibility = _trail.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void GlobalBreadcrumb_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Index < 0 || args.Index >= _trail.Count || args.Index == _trail.Count - 1)
            {
                return;
            }

            if (args.Index == 0)
            {
                NavigateToTrailModule("NavDashboard");
                return;
            }

            CollapseCurrentPageSection();
        }

        private void NavigateToTrailModule(string moduleKey)
        {
            if (moduleKey == "NavSettings")
            {
                OpenSettingsTab();
                return;
            }

            var target = FlattenMenuItems(NavView.MenuItems).FirstOrDefault(item => item.Name == moduleKey);
            if (target == null)
            {
                return;
            }

            if (!ReferenceEquals(NavView.SelectedItem, target))
            {
                NavView.SelectedItem = target;
            }

            OpenModuleTab(target);
        }

        private static void CollapseCurrentPageSection()
        {
            if (MainWindow.Instance?.MainTabs.SelectedItem is TabViewItem activeTab
                && activeTab.Content is Frame frame
                && frame.Content is SystemSettingsPage page)
            {
                page.CollapseActiveSection();
            }
        }

        private static void SyncCurrentPageSection()
        {
            var instance = MainWindow.Instance;
            if (instance?.MainTabs.SelectedItem is TabViewItem activeTab
                && activeTab.Content is Frame frame
                && frame.Content is SystemSettingsPage page)
            {
                instance.NotifySectionChanged(page.GetActiveSectionLabel());
            }
        }

        private void SetAppWindowIcon()
        {
            var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", "App.ico");
            if (System.IO.File.Exists(iconPath))
            {
                AppWindow.SetIcon(iconPath);
            }
        }

        private async Task LoadCompanyLogoAsync()
        {
            var logo = await CompanyStore.EnsureLogoAsync();
            if (logo != null)
            {
                CompanyLogo.Source = logo;
            }
        }

        public void RefreshCompanyLogo()
        {
            CompanyLogo.Source = CompanyStore.Current.Logo;
        }

        private async void CompanyLogo_Click(object sender, RoutedEventArgs e)
        {
            var hwnd = Microsoft.UI.Win32Interop.GetWindowFromWindowId(AppWindow.Id);
            if (await CompanyStore.PickAndSetLogoAsync(hwnd))
            {
                RefreshCompanyLogo();
            }
        }
    }
}