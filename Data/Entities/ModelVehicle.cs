using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class ModelVehicle : BaseEntity
{
    public Guid BrandVehicleId { get; set; }
    [ForeignKey("BrandVehicleId")]
    public virtual BrandVehicle BrandVehicle { get; set; }
    public string ModelName { get; set; }
}
