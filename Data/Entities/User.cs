﻿using Microsoft.AspNetCore.Identity;

namespace Data.Entities;

public class User : IdentityUser<Guid>
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Avatar { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime DateUpdated { get; set; } = DateTime.Now;

    public virtual ICollection<UserRole> UserRoles { get; set; }
}
