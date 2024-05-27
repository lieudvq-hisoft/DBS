using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class Emergency : BaseEntity
{
    public Guid SenderId { get; set; }
    [ForeignKey("SenderId")]
    public virtual User? Sender { get; set; }
    public Guid HandlerId { get; set; }
    [ForeignKey("HandlerId")]
    public virtual User? Handler { get; set; }
    public Guid BookingId { get; set; }
    [ForeignKey("BookingId")]
    public virtual Booking? Booking { get; set; }
    public string? Note { get; set; }
    public string? Solution { get; set; }
    public EmergencyStatus Status { get; set; } = EmergencyStatus.Pending;
    public EmergencyType EmergencyType { get; set; }
}
