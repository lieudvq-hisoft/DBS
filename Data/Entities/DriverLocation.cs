using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

namespace Data.Entities;

public class DriverLocation : BaseEntity
{
    public Guid DriverId { get; set; }
    [ForeignKey("DriverId")]
    public virtual User? Driver { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
