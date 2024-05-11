using Data.Enums;
using Data.Model;

namespace Data.Models
{
    public class WalletCreateModel
    {
        public Guid UserId { get; set; }
    }

    public class WalletModel
    {
        public Guid Id { get; set; }
        public UserModel User { get; set; }
        public long TotalMoney { get; set; }
    }

    public class WalletTransactionCreateModel
    {
        public long TotalMoney { get; set; }
    }

    public class WalletTransactionModel
    {
        public Guid Id { get; set; }
        public long TotalMoney { get; set; }
        public TypeWalletTransaction TypeWalletTransaction { get; set; }
        public PaymentType PaymentType { get; set; }
        public WalletTransactionStatus Status { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
