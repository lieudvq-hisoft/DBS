using Data.Model;
using Microsoft.AspNetCore.Http;

namespace Data.Models;

public class SearchRequestDetailCreateModel
{
    public Guid SearchRequestId { get; set; }
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Note { get; set; }
    public IFormFile? ImageData { get; set; }
    public string? LicensePlate { get; set; }
    public string? Brand { get; set; }
    public string? Color { get; set; }
    public IFormFile? VehicleImage { get; set; }
}

public class SearchRequestDetailModel
{
    public Guid Id { get; set; }
    public SearchRequestModel SearchRequest { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public string Note { get; set; }
    public string ImageData { get; set; }
    public string LicensePlate { get; set; }
    public string Brand { get; set; }
    public string Color { get; set; }
    public string VehicleImage { get; set; }
}
