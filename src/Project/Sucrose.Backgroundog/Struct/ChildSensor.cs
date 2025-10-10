using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ChildSensor
    {
        /// <summary>
        /// 
        /// </summary>
        public int? Index;
        /// <summary>
        /// 
        /// </summary>
        public float? Now;
        /// <summary>
        /// 
        /// </summary>
        public string Name;
    }
}