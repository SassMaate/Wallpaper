using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

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
        public float Total;
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
        public JArray PingData;
        /// <summary>
        /// 
        /// </summary>
        public JArray TotalData;
        /// <summary>
        /// 
        /// </summary>
        public JArray UploadData;
        /// <summary>
        /// 
        /// </summary>
        public string PingAddress;
        /// <summary>
        /// 
        /// </summary>
        public JArray DownloadData;
        /// <summary>
        /// 
        /// </summary>
        public string FormatTotalData;
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