using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace WINUI_TEST
{
    public sealed partial class ModulePage : Page
    {
        public ModulePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is string moduleName && !string.IsNullOrEmpty(moduleName))
            {
                ModuleTitleText.Text = moduleName;
                ModuleBreadcrumb.ItemsSource = new System.Collections.Generic.List<object> { moduleName };
            }
        }
    }
}