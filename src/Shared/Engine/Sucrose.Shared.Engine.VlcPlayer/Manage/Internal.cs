using MediaBase = LibVLCSharp.Shared.Media;
using MediaEngine = LibVLCSharp.Shared.MediaPlayer;
using MediaLibrary = LibVLCSharp.Shared.LibVLC;
using MediaView = LibVLCSharp.WPF.VideoView;

namespace Sucrose.Shared.Engine.VlcPlayer.Manage
{
    internal static class Internal
    {
        public static string Source;

        public static string VlcPath;

        public static MediaBase MediaBase;

        public static MediaView MediaView;

        public static MediaEngine MediaEngine;

        public static MediaLibrary MediaLibrary;

        public static IntPtr MediaHandle = IntPtr.Zero;
    }
}