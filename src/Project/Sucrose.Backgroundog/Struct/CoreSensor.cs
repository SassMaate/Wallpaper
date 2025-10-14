using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CoreSensor
    {
        /// <summary>
        /// 
        /// </summary>
        public int? Index;
        /// <summary>
        /// 
        /// </summary>
        public bool IsMax;
        /// <summary>
        /// 
        /// </summary>
        public bool IsMin;
        /// <summary>
        /// 
        /// </summary>
        public float? Now;
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public string Type;
    }
}