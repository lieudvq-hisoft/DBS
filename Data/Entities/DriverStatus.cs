using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

namespace Data.Entities;

public class DriverStatus : BaseEntity
{
    public Guid DriverId { get; set; }
    [ForeignKey("DriverId")]
    public virtual User? Driver { get; set; }
    public bool IsOnline { get; set; }
}
