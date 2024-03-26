using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class DrivingLicense : BaseEntity
{
    public Guid DriverId { get; set; }
    [ForeignKey("DriverId")]
    public virtual User? Driver { get; set; }
    public string Type { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpriedDate { get; set; }
}
