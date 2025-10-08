using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;

namespace Sucrose.Shared.Theme.Helper
{
    internal partial class Localization
    {
        [JsonProperty("Title", Required = Required.Always)]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("Description", Required = Required.Always)]
        public string Description { get; set; } = string.Empty;
    }

    internal partial class Localization
    {
        public static string Read(string Path)
        {
            string Content = SSSHF.ReadStream(Path);

            return string.IsNullOrWhiteSpace(Content) ? string.Empty : Content;
        }

        public static Localization FromJson(string Json)
        {
            return JsonConvert.DeserializeObject<Localization>(Json, Converter.Settings);
        }

        public static Localization ReadJson(string Path)
        {
            return JsonConvert.DeserializeObject<Localization>(Read(Path), Converter.Settings);
        }

        public static bool FromCheck(string Json)
        {
            try
            {
                JsonConvert.DeserializeObject<Localization>(Json, Converter.Settings);

                JToken.Parse(Json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ReadCheck(string Path)
        {
            try
            {
                string Content = Read(Path);

                JsonConvert.DeserializeObject<Localization>(Content, Converter.Settings);

                JToken.Parse(Content);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Write(string Path, Localization Json)
        {
            SSSHF.WriteStream(Path, JsonConvert.SerializeObject(Json, Converter.Settings));
        }
    }
}