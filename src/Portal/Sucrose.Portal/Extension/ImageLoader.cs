using System.IO;
using System.Net.Cache;
using System.Windows.Media.Imaging;

namespace Sucrose.Portal.Extension
{
    internal class ImageLoader : IDisposable
    {
        public BitmapImage LoadOptimal(string ImagePath, bool Decode = true, int Width = 360)
        {
            BitmapImage Image = new();

            using FileStream Stream = new(ImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            Image.BeginInit();

            Image.UriCachePolicy = new(RequestCacheLevel.BypassCache);
            Image.CacheOption = BitmapCacheOption.OnLoad;

            if (Decode)
            {
                //Image.DecodePixelHeight = 160;
                Image.DecodePixelWidth = Width;
            }

            Image.StreamSource = Stream;

            Image.EndInit();

            if (Image.CanFreeze && !Image.IsFrozen)
            {
                Image.Freeze();
            }

            return Image;
        }

        public async Task<BitmapImage> LoadOptimalAsync(string ImagePath, bool Decode = true, int Width = 360)
        {
            return await Task.Run(() => LoadOptimal(ImagePath, Decode, Width));
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}