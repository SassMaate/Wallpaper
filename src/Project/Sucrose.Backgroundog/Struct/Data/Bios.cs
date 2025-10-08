using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Bios
    {
        /// <summary>
        /// 
        /// </summary>
        public bool State;
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
        public string Version;
        /// <summary>
        /// 
        /// </summary>
        public string Description;
        /// <summary>
        /// 
        /// </summary>
        public string ReleaseDate;
        /// <summary>
        /// 
        /// </summary>
        public string Manufacturer;
        /// <summary>
        /// 
        /// </summary>
        public string SerialNumber;
        /// <summary>
        /// 
        /// </summary>
        public string CurrentLanguage;
    }
}