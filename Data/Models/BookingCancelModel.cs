using Data.Entities;
using Data.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class BookingCancelCreateModel
    {
        public Guid BookingId { get; set; }
        public IList<IFormFile>? Files { get; set; }
        public string CancelReason { get; set; }
    }

    public class BookingCancelNotiModel
    {
        public Guid Id { get; set; }
        public string[]? ImageUrls { get; set; }
        public UserModel CancelPerson { get; set; }
        public string CancelReason { get; set; }
    }

    public class BookingCancelModel
    {
        public Guid Id { get; set; }
        public BookingModel Booking { get; set; }
        public UserModel CancelPerson { get; set; }
        public string[]? ImageUrls { get; set; }
        public string CancelReason { get; set; }
    }

}
