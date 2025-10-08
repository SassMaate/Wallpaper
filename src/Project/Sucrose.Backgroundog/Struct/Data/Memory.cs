using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Memory
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
        public float? MemoryLoad;
        /// <summary>
        /// 
        /// </summary>
        public float? MemoryUsed;
        /// <summary>
        /// 
        /// </summary>
        public string VirtualName;
        /// <summary>
        /// 
        /// </summary>
        public float? MemoryAvailable;
        /// <summary>
        /// 
        /// </summary>
        public float? VirtualMemoryLoad;
        /// <summary>
        /// 
        /// </summary>
        public float? VirtualMemoryUsed;
        /// <summary>
        /// 
        /// </summary>
        public float? VirtualMemoryAvailable;
    }
}