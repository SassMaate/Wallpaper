using NPSMLib;
using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Audio
    {
        /// <summary>
        /// 
        /// </summary>
        //private uint? PID;
        /// <summary>
        /// 
        /// </summary>
        public bool State;
        /// <summary>
        /// 
        /// </summary>
        public string Title;
        /// <summary>
        /// 
        /// </summary>
        public string Artist;
        /// <summary>
        /// 
        /// </summary>
        public double[] Data;
        /// <summary>
        /// 
        /// </summary>
        //private IntPtr? Hwnd;
        /// <summary>
        /// 
        /// </summary>
        public string Subtitle;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan EndTime;
        /// <summary>
        /// 
        /// </summary>
        public string AlbumTitle;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Position;
        /// <summary>
        /// 
        /// </summary>
        public uint? TrackNumber;
        /// <summary>
        /// 
        /// </summary>
        public string AlbumArtist;
        /// <summary>
        /// 
        /// </summary>
        public string SourceAppId;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan StartTime;
        /// <summary>
        /// 
        /// </summary>
        public bool ShuffleEnabled;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan MaxSeekTime;
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan MinSeekTime;
        /// <summary>
        /// 
        /// </summary>
        public double? PlaybackRate;
        /// <summary>
        /// 
        /// </summary>
        public uint? AlbumTrackCount;
        /// <summary>
        /// 
        /// </summary>
        //private string RenderDeviceId;
        /// <summary>
        /// 
        /// </summary>
        //private string SourceDeviceId;
        /// <summary>
        /// 
        /// </summary>
        public string ThumbnailString;
        /// <summary>
        /// 
        /// </summary>
        public MediaPlaybackMode MediaType;
        /// <summary>
        /// 
        /// </summary>
        public DateTime LastPlayingFileTime;
        /// <summary>
        /// 
        /// </summary>
        public DateTime PositionSetFileTime;
        /// <summary>
        /// 
        /// </summary>
        public MediaPlaybackProps PropsValid;
        /// <summary>
        /// 
        /// </summary>
        public MediaPlaybackMode PlaybackMode;
        /// <summary>
        /// 
        /// </summary>
        public MediaPlaybackState PlaybackState;
        /// <summary>
        /// 
        /// </summary>
        public MediaPlaybackRepeatMode RepeatMode;
        /// <summary>
        /// 
        /// </summary>
        //private MediaPlaybackCapabilities PlaybackCaps;
    }
}