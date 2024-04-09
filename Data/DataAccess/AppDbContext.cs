using Data.Entities;
using Data.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Data.DataAccess;

public class AppDbContext : IdentityDbContext<User, Role, Guid>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<DriverOnlineModel>().HasNoKey();
        modelBuilder.Entity<User>(b =>
        {
            // Each User can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<Role>(b =>
        {
            // Each Role can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<Role>().HasData(
            new Entities.Role()
            {
                Id = Guid.Parse("003f7676-1d91-4143-9bfd-7a6c17c156fe"),
                Name = "Customer",
                NormalizedName = "CUSTOMER",
            });
        modelBuilder.Entity<Role>().HasData(
            new Entities.Role()
            {
                Id = Guid.Parse("7119a2e7-e680-4ecd-8344-0c53082cdc87"),
                Name = "Driver",
                NormalizedName = "DRIVER",
            });
        modelBuilder.Entity<Role>().HasData(
            new Entities.Role()
            {
                Id = Guid.Parse("931c6340-f21a-4bbf-b71c-e39d7cebc997"),
                Name = "Staff",
                NormalizedName = "STAFF",
            });
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRole { get; set; }
    public DbSet<SearchRequest> SearchRequests { get; set; }
    public DbSet<IdentityCard> IdentityCards { get; set; }
    public DbSet<IdentityCardImage> IdentityCardImages { get; set; }
    public DbSet<DriverLocation> DriverLocations { get; set; }
    public DbSet<DriverStatus> DriverStatuses { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<DrivingLicense> DrivingLicenses { get; set; }
    public DbSet<DrivingLicenseImage> DrivingLicenseImages { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleImage> VehicleImages { get; set; }
    public DbSet<BookingVehicle> BookingVehicles { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<BookingPayment> BookingPayments { get; set; }
}
