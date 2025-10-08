using System.Runtime.InteropServices;

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
        public float? Voltage;
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
        public float? ChargeRate;
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
        public float? ChargeLevel;
        /// <summary>
        /// 
        /// </summary>
        public string SaverStatus;
        /// <summary>
        /// 
        /// </summary>
        public string ACPowerStatus;
        /// <summary>
        /// 
        /// </summary>
        public float? ChargeCurrent;
        /// <summary>
        /// 
        /// </summary>
        public float? DischargeRate;
        /// <summary>
        /// 
        /// </summary>
        public float? DischargeLevel;
        /// <summary>
        /// 
        /// </summary>
        public float? DegradationLevel;
        /// <summary>
        /// 
        /// </summary>
        public float? DesignedCapacity;
        /// <summary>
        /// 
        /// </summary>
        public float? DischargeCurrent;
        /// <summary>
        /// 
        /// </summary>
        public float? RemainingCapacity;
        /// <summary>
        /// 
        /// </summary>
        public float? ChargeDischargeRate;
        /// <summary>
        /// 
        /// </summary>
        public float? FullChargedCapacity;
        /// <summary>
        /// 
        /// </summary>
        public float? ChargeDischargeCurrent;
        /// <summary>
        /// 
        /// </summary>
        public float? RemainingTimeEstimated;
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