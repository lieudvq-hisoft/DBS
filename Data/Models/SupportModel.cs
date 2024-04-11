using Data.Enums;

namespace Data.Models;

public class SupportCreateModel
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? BirthPlace { get; set; }
    public string? Address { get; set; }
    public string? DrivingLicenseNumber { get; set; }
    public string? DrivingLicenseType { get; set; }
    public string? MsgContent { get; set; }
    public SupportType SupportType { get; set; }
}

public class SupportModel
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdentityCardNumber { get; set; }
    public string? BirthPlace { get; set; }
    public string? Address { get; set; }
    public string? DrivingLicenseNumber { get; set; }
    public string? DrivingLicenseType { get; set; }
    public string? MsgContent { get; set; }
    public SupportStatus SupportStatus { get; set; }
    public SupportType SupportType { get; set; }
    public DateTime DateCreated { get; set; }
}
