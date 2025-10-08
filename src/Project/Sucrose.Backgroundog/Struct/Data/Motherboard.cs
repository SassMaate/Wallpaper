using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Motherboard
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
        public string Product;
        /// <summary>
        /// 
        /// </summary>
        public string Version;
        /// <summary>
        /// 
        /// </summary>
        public string Manufacturer;
    }
}