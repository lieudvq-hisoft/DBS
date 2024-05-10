
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;

namespace Data.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    public long TotalMoney { get; set; } = 0;
}
