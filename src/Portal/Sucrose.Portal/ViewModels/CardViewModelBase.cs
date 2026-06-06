using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using SPEIC = Sucrose.Portal.Extension.ImageCache;

namespace Sucrose.Portal.ViewModels
{
    public abstract partial class CardViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private string _theme = string.Empty;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private ImageSource _thumbnail;

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private bool _isIncompatible;

        public abstract string ThumbnailPath { get; }

        public abstract string PreviewPath { get; }

        public async Task LoadThumbnailAsync(CancellationToken Token)
        {
            IsLoading = true;

            try
            {
                ImageSource Image = await SPEIC.GetAsync(ThumbnailPath, 360, Token);

                if (!Token.IsCancellationRequested)
                {
                    Thumbnail = Image;
                    IsLoading = false;
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected when a recycled container rebinds;
                // the next LoadThumbnailAsync call resets IsLoading.
            }
        }
    }
}
