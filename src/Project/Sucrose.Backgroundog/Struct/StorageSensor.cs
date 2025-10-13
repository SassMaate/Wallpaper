using System.Runtime.InteropServices;
using SSSSS = Skylark.Struct.Storage.StorageStruct;

namespace Sucrose.Backgroundog.Struct
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StorageSensor
    {
        /// <summary>
        /// 
        /// </summary>
        public long? Size;
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public string Caption;
        /// <summary>
        /// 
        /// </summary>
        public int? DriveType;
        /// <summary>
        /// 
        /// </summary>
        public int? MediaType;
        /// <summary>
        /// 
        /// </summary>
        public SSSSS SizeData;
        /// <summary>
        /// 
        /// </summary>
        public bool Compressed;
        /// <summary>
        /// 
        /// </summary>
        public long? FreeSpace;
        /// <summary>
        /// 
        /// </summary>
        public long? UsedSpace;
        /// <summary>
        /// 
        /// </summary>
        public string FileSystem;
        /// <summary>
        /// 
        /// </summary>
        public string VolumeName;
        /// <summary>
        /// 
        /// </summary>
        public string Description;
        /// <summary>
        /// 
        /// </summary>
        public SSSSS FreeSpaceData;
        /// <summary>
        /// 
        /// </summary>
        public SSSSS UsedSpaceData;
        /// <summary>
        /// 
        /// </summary>
        public string FormatSizeData;
        /// <summary>
        /// 
        /// </summary>
        public bool SupportsDiskQuotas;
        /// <summary>
        /// 
        /// </summary>
        public string VolumeSerialNumber;
        /// <summary>
        /// 
        /// </summary>
        public string FormatFreeSpaceData;
        /// <summary>
        /// 
        /// </summary>
        public string FormatUsedSpaceData;
        /// <summary>
        /// 
        /// </summary>
        public int? MaximumComponentLength;
        /// <summary>
        /// 
        /// </summary>
        public bool SupportsFileBasedCompression;
    }
}