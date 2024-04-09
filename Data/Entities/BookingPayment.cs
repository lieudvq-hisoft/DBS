
namespace Data.Entities;

public class BookingPayment : BaseEntity
{
    public Guid BookingId { get; set; }
    public virtual Booking? Booking { get; set; }
    public long Amount { get; set; }
    public bool IsPaid { get; set; } = false;
}
