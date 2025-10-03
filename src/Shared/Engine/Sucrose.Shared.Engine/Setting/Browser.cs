using System.Collections.Concurrent;

namespace Sucrose.Shared.Engine.Setting
{
    internal class Browser
    {
        public List<string> WebView { get; set; }

        public ConcurrentDictionary<string, string> CefSharp { get; set; }
    }
}