
using Data.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class BookingCancel : BaseEntity
{
    public Guid BookingId { get; set; }
    [ForeignKey("BookingId")]
    public virtual Booking Booking { get; set; }
    public Guid CancelPersonId { get; set; }
    [ForeignKey("CancelPersonId")]
    public virtual User CancelPerson { get; set; }
    public string CancelReason { get; set; }

}
