using System.Windows.Controls;
using System.Windows.Media;
using ImageEngine = System.Windows.Controls.Image;
using SWHA = System.Windows.HorizontalAlignment;
using SWVA = System.Windows.VerticalAlignment;

namespace Sucrose.Shared.Engine.Xavier.Manage
{
    internal static class Internal
    {
        public static ImageEngine ImageEngine = new()
        {
            Stretch = Stretch.Fill,
            UseLayoutRounding = true,
            SnapsToDevicePixels = true,
            VerticalAlignment = SWVA.Stretch,
            HorizontalAlignment = SWHA.Stretch,
            StretchDirection = StretchDirection.Both
        };
    }
}