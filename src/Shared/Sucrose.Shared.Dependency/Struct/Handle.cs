using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Sucrose.Shared.Dependency.Struct
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Handle
    {
        /// <summary>
        /// 
        /// </summary>
        public Process Process;
        /// <summary>
        /// 
        /// </summary>
        public IntPtr NativeHandle;
        /// <summary>
        /// 
        /// </summary>
        public IntPtr MainWindowHandle;
    }
}