using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

namespace Data.Entities;

public class SearchRequest : BaseEntity
{
    public Guid CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public virtual User? Customer { get; set; }
    public double PickupLongitude { get; set; }
    public double PickupLatitude { get; set; }
    public double DropOffLongitude { get; set; }
    public double DropOffLatitude { get; set; }
    public string DropOffAddress { get; set; }
    public string PickupAddress { get; set; }
    public string LicensePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
    public long Price { get; set; }
    public SearchRequestStatus Status { get; set; } = SearchRequestStatus.Processing;
}
