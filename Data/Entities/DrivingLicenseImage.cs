using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities;

public class DrivingLicenseImage : BaseEntity
{
    public Guid DrivingLicenseId { get; set; }
    [ForeignKey("DrivingLicenseId")]
    public virtual DrivingLicense? DrivingLicense { get; set; }
    public bool IsFront { get; set; }
    public string ImageUrl { get; set; }
}
