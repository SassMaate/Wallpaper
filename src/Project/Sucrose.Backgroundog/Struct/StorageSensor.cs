using System.Runtime.InteropServices;

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
        public bool Compressed;
        /// <summary>
        /// 
        /// </summary>
        public long? FreeSpace;
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
        public bool SupportsDiskQuotas;
        /// <summary>
        /// 
        /// </summary>
        public string VolumeSerialNumber;
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