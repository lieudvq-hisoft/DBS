using Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Entities;

public class LinkedAccount : BaseEntity
{
    public Guid UserId { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
    public string AccountNumber { get; set; }
    public LinkedAccountType Type { get; set; }
    public string Brand { get; set; }
}
