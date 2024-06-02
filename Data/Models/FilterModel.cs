using Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class AccountFilterModel
    {
        public List<Gender>? Gender { get; set; }
        public List<bool>? IsActive { get; set; }
        public List<string>? Role { get; set; }
    }

    public class BookingFilterModel
    {
        public List<BookingStatus>? Status { get; set; }
    }

    public class EmergencyFilterModel
    {
        public List<EmergencyType>? EmergencyType { get; set; }
        public List<EmergencyStatus>? Status { get; set; }
        public List<Guid>? HandlerId { get; set; }
    }

    public class ListStaff
    {
        public string text { get; set; }
        public Guid HandlerId { get; set; }
    }

    public class TransactionFilterModel
    {
        public List<WalletTransactionStatus>? Status { get; set; }
    }

    public class WithdrawFundsRequestFilterModel
    {
        public List<WalletTransactionStatus>? Status { get; set; }
    }

    public class SupportFilterModel
    {
        public List<SupportStatus>? SupportStatus { get; set; }
        public List<SupportType>? SupportType { get; set; }
    }
}
