#if LIVE_WEBVIEW || LIVE_CEFSHARP

using Newtonsoft.Json;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using STEMREA = Sucrose.Transmission.Event.MessageReceivedEventArgs;
using STIB = Sucrose.Transmission.Interface.Backgroundog;

namespace Sucrose.Shared.Transmission.Services
{
    public static class BackgroundogTransmissionService
    {
        public static void Handler(STEMREA e)
        {
            try
            {
                if (e != null && !string.IsNullOrEmpty(e.Message))
                {
                    STIB Data = JsonConvert.DeserializeObject<STIB>(e.Message);

                    if (Data != null)
                    {
                        if (Data.Bios != null)
                        {
                            SSEMI.BiosData = JsonConvert.SerializeObject(Data.Bios, Formatting.Indented);
                        }

                        if (Data.Date != null)
                        {
                            SSEMI.DateData = JsonConvert.SerializeObject(Data.Date, Formatting.Indented);
                        }

                        if (Data.Audio != null)
                        {
                            SSEMI.AudioData = JsonConvert.SerializeObject(Data.Audio, Formatting.Indented);
                        }

                        if (Data.Memory != null)
                        {
                            SSEMI.MemoryData = JsonConvert.SerializeObject(Data.Memory, Formatting.Indented);
                        }

                        if (Data.Battery != null)
                        {
                            SSEMI.BatteryData = JsonConvert.SerializeObject(Data.Battery, Formatting.Indented);
                        }

                        if (Data.Graphic != null)
                        {
                            SSEMI.GraphicData = JsonConvert.SerializeObject(Data.Graphic, Formatting.Indented);
                        }

                        if (Data.Network != null)
                        {
                            SSEMI.NetworkData = JsonConvert.SerializeObject(Data.Network, Formatting.Indented);
                        }

                        if (Data.Storage != null)
                        {
                            SSEMI.StorageData = JsonConvert.SerializeObject(Data.Storage, Formatting.Indented);
                        }

                        if (Data.Processor != null)
                        {
                            SSEMI.ProcessorData = JsonConvert.SerializeObject(Data.Processor, Formatting.Indented);
                        }

                        if (Data.Motherboard != null)
                        {
                            SSEMI.MotherboardData = JsonConvert.SerializeObject(Data.Motherboard, Formatting.Indented);
                        }
                    }
                }
            }
            catch { }
        }
    }
}

#endif