using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Sucrose.Shared.Store.Interface
{
    internal class Store
    {
        [JsonProperty("Categories", Required = Required.Always)]
        public ConcurrentDictionary<string, Category> Categories { get; set; } = new();
    }
}