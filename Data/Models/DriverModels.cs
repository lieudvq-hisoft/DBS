using Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace Data.Model
{
    public class LocationModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class LocationCustomer : LocationModel
    {
        public double Radius { get; set; }
        public bool IsFemaleDriver { get; set; } = false;
    }

    public class TrackingDriverLocationModel : LocationModel
    {
        public Guid CustomerId { get; set; }
    }

    public class DriverOnlineModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public bool IsOnline { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class DriverOnlineSignalRModel
    {
        public Guid CustomerId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public List<Guid> ListDrivers { get; set; } = new List<Guid>();
    }
}
