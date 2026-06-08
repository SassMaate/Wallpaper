using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SMMP = Sucrose.Manager.Manage.Portal;
using SPVMLC = Sucrose.Portal.ViewModels.LibraryCardViewModel;
using SXAGAB = Sucrose.XamlAnimatedGif.AnimationBehavior;

namespace Sucrose.Portal.Views.Controls
{
    /// <summary>
    /// LibraryCard.xaml etkileşim mantığı
    /// </summary>
    public partial class LibraryCard : UserControl
    {
        private CancellationTokenSource _cts;

        public LibraryCard()
        {
            InitializeComponent();
            Unloaded += LibraryCard_Unloaded;
        }

        private SPVMLC ViewModel => DataContext as SPVMLC;

        private async void LibraryCard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            ClearPreview();

            if (ViewModel == null)
            {
                return;
            }

            _cts = new CancellationTokenSource();

            try
            {
                await ViewModel.LoadThumbnailAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                // expected on recycle
            }
        }

        private void LibraryCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            Cursor = (ViewModel.IsIncompatible || ViewModel.IsSelectedAndRunning()) ? Cursors.Arrow : Cursors.Hand;

            if (SMMP.LibraryPreview && File.Exists(ViewModel.PreviewPath))
            {
                SXAGAB.SetSourceUri(Imaginer, new Uri(ViewModel.PreviewPath));
                Imaginer.Visibility = Visibility.Visible;
                Imagine.Visibility = Visibility.Hidden;

                if (SMMP.LibraryPreviewHide)
                {
                    Preview.Visibility = Visibility.Hidden;
                }
            }
        }

        private void LibraryCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SMMP.LibraryPreview)
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

        private void LibraryCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel?.UseEntryCommand.CanExecute(null) == true)
            {
                ViewModel.UseEntryCommand.Execute(null);
            }
        }

        private void ThemeMore_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel?.RefreshMenuState();
        }

        private void LibraryCard_Unloaded(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            ClearPreview();
        }
    }
}
