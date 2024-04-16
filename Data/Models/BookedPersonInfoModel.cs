using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class BookedPersonInfoCreateModel
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public IFormFile? File { get; set; }
    }
    public class BookedPersonInfoModel
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
    }
}
