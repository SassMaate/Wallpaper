using LibVLCSharp.Shared;
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
using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHR = Sucrose.Shared.Space.Helper.Regexer;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.VlcPlayer.Helper
{
    internal static class Properties
    {
        public static void Start()
        {
            if (!Directory.Exists(SSEVPMI.VlcPath))
            {
                Directory.CreateDirectory(SSEVPMI.VlcPath);
            }

            if (!File.Exists(SSEMI.PropertiesPath))
            {
                SSEMI.PropertiesPath = Path.Combine(SSEVPMI.VlcPath, SMMRC.SucroseProperties);

                SSSHF.WriteStream(SSEMI.PropertiesPath, SSECCE.BaseToText(SSEMI.VlcProperties));
            }

            SSEMI.PropertiesCache = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Properties);
            SSEMI.PropertiesFile = Path.Combine(SSEMI.PropertiesCache, $"{SSEMI.LibrarySelected}{SSDEPT.VlcPlayer}");
            SSEMI.WatcherFile = Path.Combine(SSEMI.PropertiesCache, $"*.{SSEMI.LibrarySelected}{SSDEPT.VlcPlayer}");

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
                        switch (PropertyName)
                        {
                            case "saturation":
                                {
                                    float InputValue = Convert.ToSingle(ParsedScript.Value<double>("value"));

                                    // Map -100..100 to VLC range 0.0..3.0 (default 1.0)
                                    float Saturation = MapWithPivot(InputValue, -100f, 100f, 0f, 0f, 3f, 1f);

                                    SSEVPMI.MediaEngine.SetAdjustFloat(VideoAdjustOption.Saturation, Saturation);
                                }
                                break;
                            case "hue":
                                {
                                    float InputValue = Convert.ToSingle(ParsedScript.Value<double>("value"));

                                    // Map -100..100 to VLC range -180..180 (default 0)
                                    float hue = MapWithPivot(InputValue, -100f, 100f, 0f, -180f, 180f, 0f);

                                    SSEVPMI.MediaEngine.SetAdjustFloat(VideoAdjustOption.Hue, hue);
                                }
                                break;
                            case "brightness":
                                {
                                    float InputValue = Convert.ToSingle(ParsedScript.Value<double>("value"));

                                    // Map -100..100 to VLC range 0.0..2.0 (default 1.0)
                                    float Brightness = MapWithPivot(InputValue, -100f, 100f, 0f, 0f, 2f, 1f);

                                    SSEVPMI.MediaEngine.SetAdjustFloat(VideoAdjustOption.Brightness, Brightness);
                                }
                                break;
                            case "contrast":
                                {
                                    float InputValue = Convert.ToSingle(ParsedScript.Value<double>("value"));

                                    // Map -100..100 to VLC range 0.0..2.0 (default 1.0)
                                    float Contrast = MapWithPivot(InputValue, -100f, 100f, 0f, 0f, 2f, 1f);

                                    SSEVPMI.MediaEngine.SetAdjustFloat(VideoAdjustOption.Contrast, Contrast);
                                }
                                break;
                            // This filter is not working?
                            case "gamma":
                                {
                                    float InputValue = Convert.ToSingle(ParsedScript.Value<double>("value"));

                                    // Map -100..100 to VLC range 0.01..10.0 (default 1.0)
                                    float Gamma = MapWithPivot(InputValue, -100f, 100f, 0f, 0.01f, 10f, 1f);

                                    SSEVPMI.MediaEngine.SetAdjustFloat(VideoAdjustOption.Gamma, Gamma);
                                }
                                break;
                            case "speed":
                                {
                                    float InputValue = Convert.ToSingle(ParsedScript.Value<double>("value"));

                                    // Speed is already in correct range (0.25 - 5.0).
                                    //  --rate=<float [-340282346638528859811704183484516925440.000000 .. 340282346638528859811704183484516925440.000000]>

                                    SSEVPMI.MediaEngine.SetRate(InputValue);
                                }
                                break;
                            case "mute":
                                SSEVPMI.MediaEngine.Mute = ParsedScript.Value<bool>("value");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private static float MapWithPivot(float Value, float SourceMin, float SourceMax, float SourcePivot, float TargetMin, float TargetMax, float TargetPivot)
        {
            // Clamp so we don't extrapolate
            if (Value < SourceMin)
            {
                Value = SourceMin;
            }
            else if (Value > SourceMax)
            {
                Value = SourceMax;
            }

            if (Value >= SourcePivot)
            {
                // Upper half
                return TargetPivot + ((Value - SourcePivot) / (SourceMax - SourcePivot) * (TargetMax - TargetPivot));
            }
            else
            {
                // Lower half
                return TargetPivot + ((Value - SourcePivot) / (SourceMin - SourcePivot) * (TargetMin - TargetPivot));
            }
        }
    }
}