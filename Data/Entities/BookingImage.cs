
using Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class BookingImage : BaseEntity
{
    public Guid BookingId { get; set; }
    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }
    public string ImageUrl { get; set; }
    public BookingImageType BookingImageType { get; set; }
    public BookingImageTime BookingImageTime { get; set; }
}
