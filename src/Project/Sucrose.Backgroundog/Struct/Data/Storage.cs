using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Storage
    {
        /// <summary>
        /// 
        /// </summary>
        public bool State;
        /// <summary>
        /// 
        /// </summary>
        public JArray Drivers;
        /// <summary>
        /// 
        /// </summary>
        public JArray LogicalDrivers;
        /// <summary>
        /// 
        /// </summary>
        public JArray PhysicalDrivers;
    }
}