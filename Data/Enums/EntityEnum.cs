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

public enum UserSortByAdminCriteria
{
    DateCreated,
    Email
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
    PayBooking = 7,
    Cancel = 8,
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
}

public enum SupportStatus
{
    New = 0,
    InProcess = 1,
    Solved = 2,
    CantSolved = 3,
}
