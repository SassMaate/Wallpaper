using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Sensor
    {
        /// <summary>
        /// 
        /// </summary>
        public float? Max;
        /// <summary>
        /// 
        /// </summary>
        public float? Min;
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