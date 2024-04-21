using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;
using Data.Models;

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
    [NotMapped]
    public virtual BookingVehicleModel? BookingVehicle { get; set; }
    [NotMapped]
    public virtual CustomerBookedOnBehalfModel? CustomerBookedOnBehalf { get; set; }
    public long Price { get; set; }
    public string? Note { get; set; }
    public BookingType BookingType { get; set; } = BookingType.MySelf;
    public SearchRequestStatus Status { get; set; } = SearchRequestStatus.Processing;
}
