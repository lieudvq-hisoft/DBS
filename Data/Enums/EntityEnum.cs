using System;
namespace Data.Enums;

public enum RoleType
{
    Admin = 1,
    Member = 2,
}
public enum PickingRequestType
{
    Outbound = 1,
}
public enum ReceiptType
{
    Inbound = 1,
}
public enum NotificationSortCriteria
{
    DateCreated
}
public enum UserSortCriteria
{
    Email
}

public enum CustomerSortCriteria
{
    Email
}

public enum DriverSortCriteria
{
    Email
}

public enum UserSortByAdminCriteria
{
    DateCreated,
    Name,
    Gender,
    IsActive,
    Role
}

public enum SupplierSortCriteria
{
    DateCreated
}

public enum ReceiptSortCriteria
{
    DateCreated

}

public enum PickingRequestSortCriteria
{
    DateCreated

}

public enum ProductSortCriteria
{
    DateCreated

}

public enum SortCriteria
{
    DateCreated

}

public enum SortSupportCriteria
{
    DateCreated,
    SupportStatus,
}

public enum SortBookingCriteria
{
    DateCreated,
    Status,
}

public enum SortEmergencyCriteria
{
    DateCreated,
    EmergencyType,
    Status,
}

public enum SortWithdrawFundsTransactionCriteria
{
    DateCreated,
    Status,
    TotalMoney
}

public enum SortWalletTransactionCriteria
{
    DateCreated,
    Status,
    TotalMoney,
    TypeWalletTransaction
}

public enum RackSortCriteria
{
    DateCreated

}

public enum RackLevelSortCriteria
{
    DateCreated

}

public enum OrderSortCriteria
{
    DateCreated

}

public enum InboundProductSortCriteria
{
    DateCreated
}

public enum OutboundProductSortCriteria
{
    DateCreated
}

public enum InboundProductStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Cancel = 3,
}

public enum OutboundProductStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Cancel = 3,
}

public enum PickingRequestStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Cancel = 3,
}

public enum ReceiptStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Cancel = 3,
}

public enum SearchRequestStatus
{
    Processing = 0,
    Completed = 1,
    Cancel = 2,
}

public enum InventoryType
{
    In = 0,
    Out = 1,
}

public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2,
}

public enum BookingStatus
{
    Pending = 0,
    Accept = 1,
    Arrived = 2,
    CheckIn = 3,
    OnGoing = 4,
    CheckOut = 5,
    Complete = 6,
    Cancel = 7,
}

public enum VehicleImageType
{
    Front = 0,
    Behind = 1,
    Left = 2,
    Right = 3,
}

public enum BookingImageType
{
    Front = 0,
    Behind = 1,
    Left = 2,
    Right = 3,
    Customer = 4,
}

public enum BookingImageTime
{
    CheckIn = 0,
    CheckOut = 1,
}

public enum SupportType
{
    Recruitment = 0,
    SupportIssue = 1,
    BookingIssue = 2
}

public enum SupportStatus
{
    New = 0,
    InProcess = 1,
    Solved = 2,
    Pause = 3,
}

public enum BookingType
{
    MySelf,
    Someone,
}

public enum BookingPaymentMethod
{
    Cash,
    SecureWallet,
    MoMo,
    VNPay
}

public enum TypeWalletTransaction
{
    AddFunds = 0,
    WithdrawFunds = 1,
    Pay = 2,
    Income = 3,
    Refund = 4,
    DriverIncome = 5
}

public enum WalletTransactionStatus
{
    Waiting = 0,
    Success = 1,
    Failure = 2,
}

public enum PaymentType
{
    VNPay = 0,
    MoMo = 1,
}

public enum LinkedAccountType
{
    Bank = 0,
    DigitalWallet = 1,
}

public enum EmergencyStatus
{
    Pending = 0,
    Processing = 1,
    Solved = 2,
}

public enum EmergencyType
{
    Chat = 0,
    Call = 1,
    Police = 2
}