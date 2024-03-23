using Data.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class User : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? Dob { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime DateUpdated { get; set; } = DateTime.Now;
    public Guid? IdentityCardId { get; set; }
    [ForeignKey("IdentityCardId")]
    public virtual IdentityCard? IdentityCard { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; }

    //Driver
    public virtual ICollection<DriverLocation> DriverLocations { get; set; }
    public virtual ICollection<DriverStatus> DriverStatuses { get; set; }

}
