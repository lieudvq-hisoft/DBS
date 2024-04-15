
using Data.Enums;
using Data.Model;
using Microsoft.AspNetCore.Http;

namespace Data.Models;

public class VehicleCreateModel
{
    public string LicensePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
}

public class VehicleUpdateModel
{
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
}

public class VehicleModel
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public UserModel Customer { get; set; }
    public string LicensePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
    public string ImageUrl { get; set; }
}

public class VehicleImageCreateModel
{
    public Guid VehicleId { get; set; }
    public IFormFile File { get; set; }
    public VehicleImageType VehicleImageType { get; set; }
}

public class VehicleImageUpdateModel
{
    public IFormFile File { get; set; }
}

public class VehicleImageModel
{
    public Guid Id { get; set; }
    public Guid VehicleId { get; set; }
    public VehicleModel Vehicle { get; set; }
    public string ImageUrl { get; set; }
    public VehicleImageType VehicleImageType { get; set; }
}