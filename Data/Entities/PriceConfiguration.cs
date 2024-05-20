using Data.Models;

namespace Data.Entities;

public class PriceConfiguration : BaseEntity
{
    public BaseFareFirst3km BaseFareFirst3km { get; set; } = new BaseFareFirst3km { IsPercent = false, Price = 110000 };
    public FareFerAdditionalKm FareFerAdditionalKm { get; set; } = new FareFerAdditionalKm { Price = 20000, IsPercent = false };
    public DriverProfit DriverProfit { get; set; } = new DriverProfit { IsPercent = true, Price = 80 };
    public AppProfit AppProfit { get; set; } = new AppProfit { IsPercent = true, Price = 20 };
    public PeakHours PeakHours { get; set; } = new PeakHours { Time = "17:00-19:59", IsPercent = true, Price = 10 };
    public NightSurcharge NightSurcharge { get; set; } = new NightSurcharge { Time = "22:00-5:59", IsPercent = true, Price = 20 };
    public WaitingSurcharge WaitingSurcharge { get; set; } = new WaitingSurcharge { PerMinutes = 10, IsPercent = false, Price = 20000 };
    public WeatherFee WeatherFee { get; set; } = new WeatherFee { IsPercent = false, Price = 20000 };

}
