using Data.Entities;
using Data.Enums;
using Data.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class EmergencyCreateModel
    {
        public Guid BookingId { get; set; }
        public string? Note { get; set; }
        public EmergencyType EmergencyType { get; set; }
    }

    public class EmergencyUpdateSolveModel
    {
        public Guid EmergencyId { get; set; }
        public string Solution { get; set; }
    }

    public class EmergencyModel
    {
        public Guid Id { get; set; }
        public UserModel Sender { get; set; }
        public UserModel Handler { get; set; }
        public BookingModel Booking { get; set; }
        public string? Note { get; set; }
        public string? Solution { get; set; }
        public EmergencyStatus Status { get; set; }
        public EmergencyType EmergencyType { get; set; }
    }
}
