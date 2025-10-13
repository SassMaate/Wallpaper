using Vortice;
using Vortice.DXGI;

namespace Sucrose.Backgroundog.Enumerators
{
    public static class VorticeAdapterEnumerator
    {
        #region Public Methods

        public static string[] EnumerateAdaptersLuid()
        {
            return EnumerateAdapters().Select(Adapter => Adapter.LuidString).ToArray();
        }

        public static string[] EnumerateAdaptersVendorName()
        {
            return EnumerateAdapters().Select(Adapter => Adapter.VendorName).ToArray();
        }

        public static string[] EnumerateAdaptersDescription()
        {
            return EnumerateAdapters().Select(Adapter => Adapter.Description).ToArray();
        }

        public static List<VorticeAdapterInfo> EnumerateAdapters()
        {
            List<VorticeAdapterInfo> adapters = new();

            try
            {
                // Create DXGI Factory1 (supports DXGI 1.1+)
                using IDXGIFactory1 factory = DXGI.CreateDXGIFactory1<IDXGIFactory1>();

                // Enumerate all adapters
                for (int index = 0; factory.EnumAdapters1(index, out IDXGIAdapter1 adapter).Success; index++)
                {
                    using (adapter)
                    {
                        AdapterDescription1 desc = adapter.Description1;
                        adapters.Add(new VorticeAdapterInfo(desc, index));
                    }
                }
            }
            catch (Exception Exception)
            {
                throw new VorticeDxgiException($"Failed to enumerate DXGI adapters: {Exception.Message}", Exception);
            }

            return adapters;
        }

        public static List<VorticeAdapterInfo> EnumerateHardwareAdapters()
        {
            return EnumerateAdapters()
                .Where(a => a.VendorId != 0x1414) // Exclude Microsoft software renderer
                .Where(a => a.DedicatedVideoMemoryMB > 0) // Exclude adapters with no VRAM
                .ToList();
        }

        public static Dictionary<string, string> GetLuidToNameMapping()
        {
            Dictionary<string, string> mapping = new();

            try
            {
                List<VorticeAdapterInfo> adapters = EnumerateAdapters();

                foreach (VorticeAdapterInfo adapter in adapters)
                {
                    mapping[adapter.LuidString] = adapter.Description;
                }
            }
            catch (VorticeDxgiException)
            {
                // Return empty mapping on error
                // Caller can check if dictionary is empty
            }

            return mapping;
        }

        public static Dictionary<string, string> GetNameToLuidMapping()
        {
            Dictionary<string, string> mapping = new();

            try
            {
                List<VorticeAdapterInfo> adapters = EnumerateAdapters();

                foreach (VorticeAdapterInfo adapter in adapters)
                {
                    mapping[adapter.Description] = adapter.LuidString;
                }
            }
            catch (VorticeDxgiException)
            {
                // Return empty mapping on error
                // Caller can check if dictionary is empty
            }

            return mapping;
        }

        public static Dictionary<string, VorticeAdapterInfo> GetLuidToAdapterMapping()
        {
            Dictionary<string, VorticeAdapterInfo> mapping = new();

            try
            {
                List<VorticeAdapterInfo> adapters = EnumerateAdapters();

                foreach (VorticeAdapterInfo adapter in adapters)
                {
                    mapping[adapter.LuidString] = adapter;
                }
            }
            catch (VorticeDxgiException)
            {
                // Return empty mapping on error
            }

            return mapping;
        }

        public static Dictionary<string, VorticeAdapterInfo> GetNameToAdapterMapping()
        {
            Dictionary<string, VorticeAdapterInfo> mapping = new();

            try
            {
                List<VorticeAdapterInfo> adapters = EnumerateAdapters();

                foreach (VorticeAdapterInfo adapter in adapters)
                {
                    mapping[adapter.Description] = adapter;
                }
            }
            catch (VorticeDxgiException)
            {
                // Return empty mapping on error
            }

            return mapping;
        }

        public static string GetGpuNameByLuid(string luidString)
        {
            try
            {
                Dictionary<string, string> mapping = GetLuidToNameMapping();

                if (mapping.TryGetValue(luidString, out string gpuName))
                {
                    return gpuName;
                }
                else
                {
                    return string.Empty;
                    //throw new VorticeDxgiException($"No GPU found with LUID: {luidString}");
                }
            }
            catch (VorticeDxgiException)
            {
                return string.Empty;
                //throw new VorticeDxgiException($"Failed to get GPU name by LUID: {Exception.Message}", Exception);
            }
        }

        public static string GetGpuVendorNameByName(string name)
        {
            try
            {
                Dictionary<string, VorticeAdapterInfo> mapping = GetNameToAdapterMapping();

                if (mapping.TryGetValue(name, out VorticeAdapterInfo adapterInfo))
                {
                    return adapterInfo.VendorName;
                }
                else
                {
                    return string.Empty;
                    //throw new VorticeDxgiException($"No GPU found with name: {name}");
                }
            }
            catch (VorticeDxgiException)
            {
                return string.Empty;
                //throw new VorticeDxgiException($"Failed to get GPU vendor name by name: {Exception.Message}", Exception);
            }
        }

        public static bool TryGetGpuNameByLuid(string luidString, out string gpuName)
        {
            gpuName = null;

            try
            {
                Dictionary<string, string> mapping = GetLuidToNameMapping();

                return mapping.TryGetValue(luidString, out gpuName);
            }
            catch (VorticeDxgiException)
            {
                return false;
            }
        }

        public static bool TryGetAdapterByName(string name, out VorticeAdapterInfo adapterInfo)
        {
            adapterInfo = null;

            try
            {
                Dictionary<string, VorticeAdapterInfo> mapping = GetNameToAdapterMapping();

                return mapping.TryGetValue(name, out adapterInfo);
            }
            catch (VorticeDxgiException)
            {
                return false;
            }
        }

        public static bool TryGetAdapterByLuid(string luidString, out VorticeAdapterInfo adapterInfo)
        {
            adapterInfo = null;

            try
            {
                Dictionary<string, VorticeAdapterInfo> mapping = GetLuidToAdapterMapping();

                return mapping.TryGetValue(luidString, out adapterInfo);
            }
            catch (VorticeDxgiException)
            {
                return false;
            }
        }

        public static VorticeAdapterInfo GetPrimaryAdapter()
        {
            List<VorticeAdapterInfo> adapters = EnumerateAdapters();

            return adapters.Count > 0 ? adapters[0] : null;
        }

        #endregion
    }

    #region Public Data Classes

    public class VorticeAdapterInfo
    {
        public int Index { get; }

        public Luid Luid { get; }

        public uint DeviceId { get; }

        public uint Revision { get; }

        public uint VendorId { get; }

        public uint SubsystemId { get; }

        public string Description { get; }

        public AdapterFlags Flags { get; }

        public ulong SharedSystemMemory { get; }

        public ulong DedicatedVideoMemory { get; }

        public ulong DedicatedSystemMemory { get; }

        public string VendorName => VendorId switch
        {
            0x10DE => "NVIDIA",
            0x1002 => "AMD",
            0x8086 => "Intel",
            0x1414 => "Microsoft",
            _ => "Unknown"
        };

        public bool IsRemoteAdapter => Flags.HasFlag(AdapterFlags.Remote);

        public bool IsSoftwareAdapter => Flags.HasFlag(AdapterFlags.Software);

        public string LuidString => $"0x{Luid.HighPart:X8}_0x{Luid.LowPart:X8}";

        public ulong DedicatedVideoMemoryMB => DedicatedVideoMemory / 1024 / 1024;

        internal VorticeAdapterInfo(AdapterDescription1 desc, int index)
        {
            Index = index;
            Luid = desc.Luid;
            Flags = desc.Flags;
            Description = desc.Description;
            DeviceId = (uint)desc.DeviceId;
            Revision = (uint)desc.Revision;
            VendorId = (uint)desc.VendorId;
            SubsystemId = (uint)desc.SubsystemId;
            SharedSystemMemory = (ulong)(nint)desc.SharedSystemMemory;
            DedicatedVideoMemory = (ulong)(nint)desc.DedicatedVideoMemory;
            DedicatedSystemMemory = (ulong)(nint)desc.DedicatedSystemMemory;
        }

        public override string ToString()
        {
            return $"{Description} ({VendorName}) - {DedicatedVideoMemoryMB} MB VRAM - LUID: {LuidString}";
        }

        public string ToDetailedString()
        {
            return $"Adapter #{Index}: {Description}\n" +
                   $"  Vendor: {VendorName} (0x{VendorId:X4})\n" +
                   $"  Device ID: 0x{DeviceId:X4}\n" +
                   $"  VRAM: {DedicatedVideoMemoryMB:N0} MB\n" +
                   $"  LUID: {LuidString}\n" +
                   $"  Flags: {Flags}";
        }
    }

    public class VorticeDxgiException : Exception
    {
        public VorticeDxgiException(string message) : base(message)
        { }

        public VorticeDxgiException(string message, Exception innerException) : base(message, innerException)
        { }
    }

    #endregion
}