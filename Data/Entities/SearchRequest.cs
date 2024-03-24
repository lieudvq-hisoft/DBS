using System.ComponentModel.DataAnnotations.Schema;
using Data.Enums;

namespace Data.Entities;

public class SearchRequest : BaseEntity
{
    public Guid CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public virtual User? Customer { get; set; }
    public double PickupLocation { get; set; }
    public double DropOffLocation { get; set; }
    public long Price { get; set; }
    public SearchRequestStatus Status { get; set; } = SearchRequestStatus.Processing;
}
