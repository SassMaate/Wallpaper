namespace Sucrose.Portal.ViewModels.Pages.Library
{
    public partial class FullLibraryViewModel : ViewModel, IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}