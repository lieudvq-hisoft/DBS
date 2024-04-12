using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class RevenueDetail : BaseEntity
{
    public Guid DriverRevenueId { get; set; }
    [ForeignKey("DriverRevenueId")]
    public virtual DriverRevenue? DriverRevenue { get; set; }
    public Guid BookingId { get; set; }
    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }
    public long DriverIncome { get; set; }
    public long SystemIncome { get; set; }
}
