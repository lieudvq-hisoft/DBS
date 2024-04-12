
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class DriverRevenue : BaseEntity
{
    public Guid DriverId { get; set; }
    [ForeignKey("DriverId")]
    public virtual User? Driver { get; set; }
    public long TotalPrice { get; set; }
    public long TotalFee { get; set; }
    public long TotalInCome { get; set; }
    public DateOnly Date { get; set; }
    public DateTime PaidTime { get; set; }
    public bool IsPaid { get; set; }

}
