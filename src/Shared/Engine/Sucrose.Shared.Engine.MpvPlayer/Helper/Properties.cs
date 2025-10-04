using Newtonsoft.Json.Linq;
using System.IO;
using System.Text.RegularExpressions;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SSDEPT = Sucrose.Shared.Dependency.Enum.PropertiesType;
using SSECCE = Skylark.Standard.Extension.Cryptology.CryptologyExtension;
using SSEHP = Sucrose.Shared.Engine.Helper.Properties;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEMPMI = Sucrose.Shared.Engine.MpvPlayer.Manage.Internal;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHR = Sucrose.Shared.Space.Helper.Regexer;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.MpvPlayer.Helper
{
    internal static class Properties
    {
        public static void Start()
        {
            if (!Directory.Exists(SSEMPMI.MpvPath))
            {
                Directory.CreateDirectory(SSEMPMI.MpvPath);
            }

            if (!File.Exists(SSEMI.PropertiesPath))
            {
                SSEMI.PropertiesPath = Path.Combine(SSEMPMI.MpvPath, SMMRC.SucroseProperties);

                SSSHF.WriteStream(SSEMI.PropertiesPath, SSECCE.BaseToText(SSEMI.MpvProperties));
            }

            SSEMI.PropertiesCache = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Properties);
            SSEMI.PropertiesFile = Path.Combine(SSEMI.PropertiesCache, $"{SSEMI.LibrarySelected}{SSDEPT.MpvPlayer}");
            SSEMI.WatcherFile = Path.Combine(SSEMI.PropertiesCache, $"*.{SSEMI.LibrarySelected}{SSDEPT.MpvPlayer}");

            if (!Directory.Exists(SSEMI.PropertiesCache))
            {
                Directory.CreateDirectory(SSEMI.PropertiesCache);
            }

            if (!File.Exists(SSEMI.PropertiesFile))
            {
                SSSHF.CopyBuffer(SSEMI.PropertiesPath, SSEMI.PropertiesFile);
            }

            try
            {
                SSEMI.Properties = SSTHP.ReadJson(SSEMI.PropertiesFile);
            }
            catch (NotSupportedException Exception)
            {
                SSSHF.Delete(SSEMI.PropertiesFile);

                throw new NotSupportedException(Exception.Message);
            }
            catch (Exception Exception)
            {
                SSSHF.Delete(SSEMI.PropertiesFile);

                throw new Exception(Exception.Message, Exception.InnerException);
            }

            SSEMI.Properties.State = true;

            SSEHP.Watcher(SSEMI.WatcherFile);
        }

        private static string Value(string Data)
        {
            int StartIndex = Data.IndexOf("{");
            int EndIndex = Data.LastIndexOf("}");

            if (StartIndex >= 0 && EndIndex > StartIndex)
            {
                return Data.Substring(StartIndex, EndIndex - StartIndex + 1);
            }

            return string.Empty;
        }

        private static string Property(string Data)
        {
            Match Matches = SSSHR.Match(Data, @"'\s*([^']+)\s*'"); //@"SucrosePropertyListener\('(\w+)'"

            return Matches.Success ? Matches.Groups[1].Value : string.Empty;
        }

        public static async void ExecuteScript(string Script)
        {
            try
            {
                JObject ParsedScript = JObject.Parse(Value(Script));

                string PropertyType = ParsedScript.Value<string>("type");

                if (!PropertyType.Equals("label", StringComparison.OrdinalIgnoreCase) && !PropertyType.Equals("button", StringComparison.OrdinalIgnoreCase) && !PropertyType.Equals("filedropdown", StringComparison.OrdinalIgnoreCase))
                {
                    string PropertyName = Property(Script);

                    if (!string.IsNullOrWhiteSpace(PropertyName))
                    {
                        if (PropertyType.Equals("slider", StringComparison.OrdinalIgnoreCase))
                        {
                            SSEMPMI.MediaEngine.API.SetPropertyDouble(PropertyName, ParsedScript.Value<double>("value"));
                        }
                        else if (PropertyType.Equals("textbox", StringComparison.OrdinalIgnoreCase))
                        {
                            SSEMPMI.MediaEngine.API.SetPropertyString(PropertyName, ParsedScript.Value<string>("value"));
                        }
                        else if (PropertyType.Equals("checkbox", StringComparison.OrdinalIgnoreCase))
                        {
                            SSEMPMI.MediaEngine.API.SetPropertyString(PropertyName, ParsedScript.Value<bool>("value") ? "yes" : "no");
                        }
                        else if (PropertyType.Equals("dropdown", StringComparison.OrdinalIgnoreCase))
                        {
                            SSEMPMI.MediaEngine.API.SetPropertyLong(PropertyName, ParsedScript.Value<long>("value"));
                        }
                        else if (PropertyType.Equals("numberbox", StringComparison.OrdinalIgnoreCase))
                        {
                            SSEMPMI.MediaEngine.API.SetPropertyDouble(PropertyName, ParsedScript.Value<double>("value"));
                        }
                        else if (PropertyType.Equals("colorpicker", StringComparison.OrdinalIgnoreCase))
                        {
                            SSEMPMI.MediaEngine.API.SetPropertyString(PropertyName, ParsedScript.Value<string>("value"));
                        }
                        else if (PropertyType.Equals("passwordbox", StringComparison.OrdinalIgnoreCase))
                        {
                            SSEMPMI.MediaEngine.API.SetPropertyString(PropertyName, ParsedScript.Value<string>("value"));
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }
    }
}