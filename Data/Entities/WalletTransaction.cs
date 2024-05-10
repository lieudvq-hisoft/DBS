
using Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    [ForeignKey("WalletId")]
    public virtual Wallet Wallet { get; set; }
    public long TotalMoney { get; set; }
    public TypeWalletTransaction TypeWalletTransaction { get; set; }
}
