using System.Runtime.InteropServices;
using SEEST = Skylark.Enum.EnergySaverType;
using SEPPT = Skylark.Enum.PowerPlanType;

namespace Sucrose.Backgroundog.Struct.Data
{
    /// <summary>
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Battery
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
        public int Chemistry;
        /// <summary>
        /// 
        /// </summary>
        public string Status;
        /// <summary>
        /// 
        /// </summary>
        public bool SavingMode;
        /// <summary>
        /// 
        /// </summary>
        public int FullLifetime;
        /// <summary>
        /// 
        /// </summary>
        public int DesignVoltage;
        /// <summary>
        /// 
        /// </summary>
        public float LifePercent;
        /// <summary>
        /// 
        /// </summary>
        public int LifeRemaining;
        /// <summary>
        /// 
        /// </summary>
        public string BatteryFlag;
        /// <summary>
        /// 
        /// </summary>
        public string Description;
        /// <summary>
        /// 
        /// </summary>
        public string SaverStatus;
        /// <summary>
        /// 
        /// </summary>
        public SEPPT PowerPlanType;
        /// <summary>
        /// 
        /// </summary>
        public string ACPowerStatus;
        /// <summary>
        /// 
        /// </summary>
        public int EstimatedRunTime;
        /// <summary>
        /// 
        /// </summary>
        public SEEST EnergySaverType;
        /// <summary>
        /// 
        /// </summary>
        public int EstimatedChargeRemaining;
        /// <summary>
        /// 
        /// </summary>
        public PowerLineStatus PowerLineStatus;
        /// <summary>
        /// 
        /// </summary>
        public BatteryChargeStatus ChargeStatus;
    }
}