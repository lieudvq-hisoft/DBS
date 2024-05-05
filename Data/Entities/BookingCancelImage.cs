
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class BookingCancelImage : BaseEntity
{
    public Guid BookingCancelId { get; set; }
    [ForeignKey("BookingCancelId")]
    public virtual BookingCancel BookingCancel { get; set; }
    public string ImageUrl { get; set; }
}
