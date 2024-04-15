using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class Rating : BaseEntity
{
    public Guid BookingId { get; set; }
    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }
    public int Star { get; set; }
    public string? Comment { get; set; }
    public string? ImageUrl { get; set; }
}
