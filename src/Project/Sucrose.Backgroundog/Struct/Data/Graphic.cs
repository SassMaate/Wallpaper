using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GraphicStruct
    {
        /// <summary>
        /// 
        /// </summary>
        public bool State;
        /// <summary>
        /// 
        /// </summary>
        public JArray Amd;
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public JArray Intel;
        /// <summary>
        /// 
        /// </summary>
        public JArray Nvidia;
        /// <summary>
        /// 
        /// </summary>
        public string Manufacturer;
    }
}