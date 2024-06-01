using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models;

public class CustomerBookedOnBehalfModel
{
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Note { get; set; }
    public DateTime DateCreated { get; set; }
}
