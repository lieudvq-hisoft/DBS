using Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;
public class Booking : BaseEntity
{
    public Guid SearchRequestId { get; set; }
    [ForeignKey("SearchRequestId")]
    public virtual SearchRequest? SearchRequest { get; set; }

    public Guid DriverId { get; set; }
    [ForeignKey("DriverId")]
    public virtual User? Driver { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Accept;

}
