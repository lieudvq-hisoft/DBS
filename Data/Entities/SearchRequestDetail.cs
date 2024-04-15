
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class SearchRequestDetail : BaseEntity
{
    public Guid SearchRequestId { get; set; }
    [ForeignKey("SearchRequestId")]
    public virtual SearchRequest? SearchRequest { get; set; }
    //Customer
    public string? Name { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Note { get; set; }
    public string? ImageUrl { get; set; }

    //Vehicle
    public string? LicensePlate { get; set; }
    public string? Brand { get; set; }
    public string? Color { get; set; }
    public string? VehicleImage { get; set; }

}
