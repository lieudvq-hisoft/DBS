
using Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class VehicleImage : BaseEntity
{
    public Guid VehicleId { get; set; }
    [ForeignKey("VehicleId")]
    public virtual Vehicle Vehicle { get; set; }
    public string ImageData { get; set; }
    public VehicleImageType VehicleImageType { get; set; }
}
