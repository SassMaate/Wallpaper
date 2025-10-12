using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using SSSSS = Skylark.Struct.Storage.StorageStruct;
using SBSSS = Sucrose.Backgroundog.Struct.StorageSensor;

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
        public SBSSS Drivers;
    }
}