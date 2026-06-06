using System.Collections.Concurrent;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sucrose.Portal.Extension
{
    public static class ImageCache
    {
        private const int Capacity = 240; // ~enough for several screens of cards

        private static readonly object Gate = new();
        private static readonly LinkedList<string> Lru = new();
        private static readonly Dictionary<string, (LinkedListNode<string> Node, ImageSource Image)> Map = [];
        private static readonly ConcurrentDictionary<string, Task<ImageSource>> InFlight = [];

        public static async Task<ImageSource> GetAsync(string Path, int DecodeWidth = 360, CancellationToken Token = default)
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
            {
                return null;
            }

            string Key = $"{Path}|{DecodeWidth}";

            lock (Gate)
            {
                if (Map.TryGetValue(Key, out (LinkedListNode<string> Node, ImageSource Image) Hit))
                {
                    Lru.Remove(Hit.Node);
                    Lru.AddFirst(Hit.Node);
                    return Hit.Image;
                }
            }

            Task<ImageSource> Load = InFlight.GetOrAdd(Key, _ => Task.Run(() => Decode(Path, DecodeWidth), Token));

            try
            {
                ImageSource Image = await Load.WaitAsync(Token);
                Store(Key, Image);
                return Image;
            }
            finally
            {
                InFlight.TryRemove(Key, out _);
            }
        }

        private static ImageSource Decode(string Path, int DecodeWidth)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            BitmapImage Image = new();
            Image.BeginInit();
            Image.CacheOption = BitmapCacheOption.OnLoad;
            Image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            Image.DecodePixelWidth = DecodeWidth;
            Image.StreamSource = Stream;
            Image.EndInit();

            if (Image.CanFreeze)
            {
                Image.Freeze();
            }

            return Image;
        }

        private static void Store(string Key, ImageSource Image)
        {
            if (Image == null)
            {
                return;
            }

            lock (Gate)
            {
                if (Map.ContainsKey(Key))
                {
                    return;
                }

                LinkedListNode<string> Node = Lru.AddFirst(Key);
                Map[Key] = (Node, Image);

                while (Map.Count > Capacity && Lru.Last != null)
                {
                    string Evict = Lru.Last.Value;
                    Lru.RemoveLast();
                    Map.Remove(Evict);
                }
            }
        }
    }
}
