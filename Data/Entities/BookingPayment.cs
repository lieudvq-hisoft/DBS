
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class BookingPayment : BaseEntity
{
    public Guid BookingId { get; set; }
    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }
    public long Amount { get; set; }
    public bool IsPaid { get; set; } = false;
}
