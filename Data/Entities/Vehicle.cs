
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class Vehicle : BaseEntity
{
    public Guid CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public virtual User? Customer { get; set; }
    public string LicensePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }

}