using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Processor
    {
        /// <summary>
        /// 
        /// </summary>
        public int? Core;
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
        public bool State;
        /// <summary>
        /// 
        /// </summary>
        public int? Thread;
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public float? CoreMax;
        /// <summary>
        /// 
        /// </summary>
        public float? CoreMin;
        /// <summary>
        /// 
        /// </summary>
        public float? CoreNow;
        /// <summary>
        /// 
        /// </summary>
        public string FullName;
    }
}