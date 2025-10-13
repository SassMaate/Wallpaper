using System.Runtime.InteropServices;
using SSPPSS = Skylark.Struct.Ping.PingSendStruct;
using SSSSS = Skylark.Struct.Storage.StorageStruct;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Network
    {
        /// <summary>
        /// 
        /// </summary>
        public long? Ping;
        /// <summary>
        /// 
        /// </summary>
        public bool State;
        /// <summary>
        /// 
        /// </summary>
        public string Host;
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public bool Online;
        /// <summary>
        /// 
        /// </summary>
        public float Upload;
        /// <summary>
        /// 
        /// </summary>
        public float Download;
        /// <summary>
        /// 
        /// </summary>
        public SSPPSS PingData;
        /// <summary>
        /// 
        /// </summary>
        public SSSSS UploadData;
        /// <summary>
        /// 
        /// </summary>
        public SSSSS DownloadData;
        /// <summary>
        /// 
        /// </summary>
        public string PingAddress;
        /// <summary>
        /// 
        /// </summary>
        public string FormatUploadData;
        /// <summary>
        /// 
        /// </summary>
        public string FormatDownloadData;
    }
}