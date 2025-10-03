using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Sucrose.Shared.Store.Interface
{
    internal class Category
    {
        [JsonProperty("Wallpapers", Required = Required.Default)]
        public Dictionary<string, Wallpaper> Wallpapers { get; set; } = new();
    }
}