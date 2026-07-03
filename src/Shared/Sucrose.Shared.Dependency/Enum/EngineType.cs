namespace Sucrose.Shared.Dependency.Enum
{
    internal enum EngineType
    {
        AuroraLive,
        NebulaLive,
        VexanaLive,
        XavierLive,
        WebViewLive,
        CefSharpLive,
        MpvPlayerLive,
        VlcPlayerLive
    }

    internal enum GifEngineType
    {
        Vexana = EngineType.VexanaLive,
        Xavier = EngineType.XavierLive,
        WebView = EngineType.WebViewLive,
        CefSharp = EngineType.CefSharpLive,
        MpvPlayer = EngineType.MpvPlayerLive,
        VlcPlayer = EngineType.VlcPlayerLive
    }

    internal enum UrlEngineType
    {
        WebView = EngineType.WebViewLive,
        CefSharp = EngineType.CefSharpLive
    }

    internal enum WebEngineType
    {
        WebView = EngineType.WebViewLive,
        CefSharp = EngineType.CefSharpLive
    }

    internal enum VideoEngineType
    {
        Nebula = EngineType.NebulaLive,
        WebView = EngineType.WebViewLive,
        CefSharp = EngineType.CefSharpLive,
        MpvPlayer = EngineType.MpvPlayerLive,
        VlcPlayer = EngineType.VlcPlayerLive
    }

    internal enum YouTubeEngineType
    {
        WebView = EngineType.WebViewLive,
        CefSharp = EngineType.CefSharpLive
    }

    internal enum ApplicationEngineType
    {
        Aurora = EngineType.AuroraLive
    }
}