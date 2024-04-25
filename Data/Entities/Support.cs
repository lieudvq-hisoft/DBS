using Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class Support : BaseEntity
{
    public Guid? BookingId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? BirthPlace { get; set; }
    public string? Address { get; set; }
    public string? DrivingLicenseNumber { get; set; }
    public string? DrivingLicenseType { get; set; }
    public string? MsgContent { get; set; }
    public SupportStatus SupportStatus { get; set; } = SupportStatus.New;
    public SupportType SupportType { get; set; }

}
