using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class BaseFareFirst3km
    {
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }
    public class FareFerAdditionalKm
    {
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }
    public class DriverProfit
    {
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }

    public class AppProfit
    {
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }
    public class PeakHours
    {
        public string? Time { get; set; }
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }

    public class NightSurcharge
    {
        public string? Time { get; set; }
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }
    public class WaitingSurcharge
    {
        public int? PerMinutes { get; set; }
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }
    public class WeatherFee
    {
        public long? Price { get; set; }
        public bool? IsPercent { get; set; }
    }

    public class PriceConfigurationUpdateModel
    {
        public BaseFareFirst3km? BaseFareFirst3km { get; set; }
        public FareFerAdditionalKm? FareFerAdditionalKm { get; set; }
        public DriverProfit? DriverProfit { get; set; }
        public AppProfit? AppProfit { get; set; }
        public PeakHours? PeakHours { get; set; }
        public NightSurcharge? NightSurcharge { get; set; }
        public WaitingSurcharge? WaitingSurcharge { get; set; }
        public WeatherFee? WeatherFee { get; set; }
    }

}
