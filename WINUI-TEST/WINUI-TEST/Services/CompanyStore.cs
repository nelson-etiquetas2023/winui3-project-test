using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;
using WINUI_TEST.Models;

namespace WINUI_TEST.Services
{
    public static class CompanyStore
    {
        public static CompanyInfo Current { get; } = new CompanyInfo
        {
            Name = "Amazon",
            Address = "Av. Churchill esq. Ortega y Gasset, Santo Domingo, R.D.",
            Itbis = "1-01-12345-6-0"
        };

        public static event EventHandler? CompanyInfoChanged;

        public static void RaiseCompanyInfoChanged() => CompanyInfoChanged?.Invoke(null, EventArgs.Empty);

        public static async Task<ImageSource?> EnsureLogoAsync()
        {
            if (Current.Logo != null)
            {
                return Current.Logo;
            }

            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "CompanyLogo.png");
            if (!File.Exists(path))
            {
                return null;
            }

            using var fs = File.OpenRead(path);
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(fs.AsRandomAccessStream());
            Current.Logo = bitmap;
            return Current.Logo;
        }

        public static async Task<bool> PickAndSetLogoAsync(IntPtr hwnd)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");
            InitializeWithWindow.Initialize(picker, hwnd);

            StorageFile? file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return false;
            }

            using var stream = await file.OpenStreamForReadAsync();
            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
            Current.Logo = bitmap;
            return true;
        }
    }
}