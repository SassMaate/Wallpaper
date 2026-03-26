using System.Windows.Controls;
using System.Windows.Media;
using MediaEngine = System.Windows.Controls.MediaElement;
using SWHA = System.Windows.HorizontalAlignment;
using SWVA = System.Windows.VerticalAlignment;

namespace Sucrose.Shared.Engine.Nebula.Manage
{
    internal static class Internal
    {
        public static MediaEngine MediaEngine = new()
        {
            LoadedBehavior = MediaState.Manual,
            HorizontalAlignment = SWHA.Stretch,
            VerticalAlignment = SWVA.Stretch,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true,
            Stretch = Stretch.Fill,
            Volume = 0
        };
    }
}