using CefSharp;

namespace Sucrose.Shared.Engine.CefSharp.Handler
{
    internal class CustomDownload : IDownloadHandler
    {
        public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        {
            return false;
        }

        public bool OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            downloadItem.IsCancelled = true;
            return false;
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            callback.Cancel();
            return;
        }
    }
}