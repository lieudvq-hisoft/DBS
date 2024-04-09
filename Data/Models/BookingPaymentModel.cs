
namespace Data.Models;

public class BookingPaymentCreateModel
{
    public Guid BookingId { get; set; }
    public long Amount { get; set; }
}

public class BookingPaymentModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public BookingModel Booking { get; set; }
    public long Amount { get; set; }
    public bool IsPaid { get; set; }
}
