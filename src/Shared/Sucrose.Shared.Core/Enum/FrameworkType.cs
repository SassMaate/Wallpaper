using System.ComponentModel.DataAnnotations;

namespace Sucrose.Shared.Core.Enum
{
    internal enum FrameworkType
    {
        [DisplayAttribute(Name = "Unknown", Description = "Unknown")]
        Unknown,
        [DisplayAttribute(Name = ".NET 6.0", Description = ".NET_6.0")]
        NET_6_0,
        [DisplayAttribute(Name = ".NET 7.0", Description = ".NET_7.0")]
        NET_7_0,
        [DisplayAttribute(Name = ".NET 8.0", Description = ".NET_8.0")]
        NET_8_0,
        [DisplayAttribute(Name = ".NET 9.0", Description = ".NET_9.0")]
        NET_9_0,
        [DisplayAttribute(Name = ".NET 10.0", Description = ".NET_10.0")]
        NET_10_0,
        [DisplayAttribute(Name = ".NET 11.0", Description = ".NET_11.0")]
        NET_11_0,
        [DisplayAttribute(Name = ".NET 12.0", Description = ".NET_12.0")]
        NET_12_0,
        [DisplayAttribute(Name = ".NET 13.0", Description = ".NET_13.0")]
        NET_13_0,
        [DisplayAttribute(Name = ".NET 14.0", Description = ".NET_14.0")]
        NET_14_0,
        [DisplayAttribute(Name = ".NET 15.0", Description = ".NET_15.0")]
        NET_15_0,
        [DisplayAttribute(Name = ".NET Framework 4.8", Description = ".NET_Framework_4.8")]
        NET_Framework_4_8,
        [DisplayAttribute(Name = ".NET Framework 4.8.1", Description = ".NET_Framework_4.8.1")]
        NET_Framework_4_8_1
    }
}