using System.Runtime.InteropServices;
using SSSSS = Skylark.Struct.Storage.StorageStruct;

namespace Sucrose.Backgroundog.Struct
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DriverSensor
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name;
        /// <summary>
        /// 
        /// </summary>
        public float? Read;
        /// <summary>
        /// 
        /// </summary>
        public float? Write;
        /// <summary>
        /// 
        /// </summary>
        public SSSSS ReadData;
        /// <summary>
        /// 
        /// </summary>
        public SSSSS WriteData;
        /// <summary>
        /// 
        /// </summary>
        public string FormatReadData;
        /// <summary>
        /// 
        /// </summary>
        public string FormatWriteData;
    }
}