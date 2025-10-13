using Newtonsoft.Json.Linq;
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
        public string Type;
        /// <summary>
        /// 
        /// </summary>
        public JArray Cores;
        /// <summary>
        /// 
        /// </summary>
        public float? CoresMax;
        /// <summary>
        /// 
        /// </summary>
        public float? CoresMin;
        /// <summary>
        /// 
        /// </summary>
        public float? CoresNow;
        /// <summary>
        /// 
        /// </summary>
        public int? ProcessorCount;
    }
}