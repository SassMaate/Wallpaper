using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SMMP = Sucrose.Manager.Manage.Portal;
using SPVMSCVM = Sucrose.Portal.ViewModels.StoreCardViewModel;
using SXAGAB = Sucrose.XamlAnimatedGif.AnimationBehavior;

namespace Sucrose.Portal.Views.Controls
{
    /// <summary>
    /// StoreCard.xaml etkileşim mantığı
    /// </summary>
    public partial class StoreCard : UserControl
    {
        private CancellationTokenSource _cts;

        public StoreCard()
        {
            InitializeComponent();
            Unloaded += StoreCard_Unloaded;
        }

        private SPVMSCVM ViewModel => DataContext as SPVMSCVM;

        private async void StoreCard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            ClearPreview();

            if (e.OldValue is SPVMSCVM oldVm)
            {
                oldVm.UnsubscribeInfoChanged();

                // Release the scrolled-away card's bitmap. The VM stays alive in the
                // (3000+) collection, so without this every cover ever shown would stay
                // resident -> GBs of RAM. The bounded ImageCache still re-serves recent
                // covers instantly on scroll-back.
                oldVm.Thumbnail = null;
            }

            if (ViewModel is not SPVMSCVM viewModel)
            {
                return;
            }

            _cts = new CancellationTokenSource();

            // Capture the VM and token as LOCALS. This is an async handler and the card
            // recycles fast: a newer DataContextChanged can null/replace the `_cts` field
            // (and the DataContext) while we are awaiting, so reading the field after an
            // await would throw NullReference. The captured token still lets a stale
            // invocation detect cancellation and bail out.
            CancellationToken token = _cts.Token;

            try
            {
                // Debounce: only fetch/download for cards the user actually lingers on.
                // Fast scrolling recycles the card within this window, cancelling the token
                // so we never flood thousands of cover downloads/decodes (RAM + stutter).
                await Task.Delay(200, token);

                bool result = await viewModel.EnsureDownloadedAsync(token);

                if (!token.IsCancellationRequested && result)
                {
                    viewModel.SubscribeInfoChanged();

                    // Thumbnail is now valid (EnsureDownloadedAsync populated Info +
                    // ThumbnailPath), so kick off the image load.
                    await viewModel.LoadThumbnailAsync(token);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on recycle
            }
        }

        private void StoreCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            // Cursor: if load failed or incompatible, use Arrow; otherwise Hand.
            Cursor = (ViewModel.IsLoadFailed || ViewModel.IsIncompatible) ? Cursors.Arrow : Cursors.Hand;

            if (SMMP.StorePreview && !string.IsNullOrEmpty(ViewModel.PreviewPath))
            {
                // Defer the swap until the GIF has actually loaded (MediaOpened) so the
                // thumbnail stays visible — and hit-testable — meanwhile.  Hiding it
                // immediately would expose a transparent, non-hit-testable gap that makes
                // MouseEnter/MouseLeave oscillate (Rule 3b).
                SXAGAB.SetSourceUri(Imaginer, new Uri(ViewModel.PreviewPath));
                SXAGAB.AddLoadedHandler(Imaginer, Imaginer_MediaOpened);
            }
        }

        private void Imaginer_MediaOpened(object sender, RoutedEventArgs e)
        {
            Imaginer.Visibility = Visibility.Visible;
            Imagine.Visibility = Visibility.Hidden;

            if (SMMP.StorePreviewHide)
            {
                Preview.Visibility = Visibility.Hidden;
            }
        }

        private void StoreCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SMMP.StorePreview)
            {
                ClearPreview();
            }
        }

        private void ClearPreview()
        {
            Imaginer.Source = null;
            SXAGAB.SetSourceUri(Imaginer, null);
            Imaginer.Visibility = Visibility.Hidden;
            Imagine.Visibility = Visibility.Visible;
            Preview.Visibility = Visibility.Visible;
        }

        private void StoreCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel != null)
            {
                _ = ViewModel.StartDownloadAsync();
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                _ = ViewModel.StartDownloadAsync();
            }
        }

        // A Border's CornerRadius does NOT clip its children to rounded corners (ClipToBounds clips
        // to the rectangular bounds only).  Apply a rounded RectangleGeometry clip to the content
        // grid so the thumbnail, hover GIF, and overlay are all rounded — without a VisualBrush.
        private void CardClip_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            CardClip.Clip = new RectangleGeometry(new Rect(0, 0, e.NewSize.Width, e.NewSize.Height), 10, 10);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel?.RefreshMenuState();
        }

        private void StoreCard_Unloaded(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            ClearPreview();
            ViewModel?.UnsubscribeInfoChanged();
        }
    }
}
