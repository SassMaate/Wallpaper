using System.Runtime.InteropServices;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Date
    {
        /// <summary>
        /// 
        /// </summary>
        public int? Day;
        /// <summary>
        /// 
        /// </summary>
        public int? Hour;
        /// <summary>
        /// 
        /// </summary>
        public int? Year;
        /// <summary>
        /// 
        /// </summary>
        public int? Month;
        /// <summary>
        /// 
        /// </summary>
        public bool State;
        /// <summary>
        /// 
        /// </summary>
        public int? Minute;
        /// <summary>
        /// 
        /// </summary>
        public int? Second;
        /// <summary>
        /// 
        /// </summary>
        public int? DayOfYear;
        /// <summary>
        /// 
        /// </summary>
        public int? Millisecond;
        /// <summary>
        /// 
        /// </summary>
        public DateTimeKind? Kind;
        /// <summary>
        /// 
        /// </summary>
        public DayOfWeek? DayOfWeek;
    }
}