using Data.Entities;
using Data.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Model
{
    public class SearchRequestCreateModel
    {
        public double PickupLocation { get; set; }
        public double DropOffLocation { get; set; }
        public long Price { get; set; }
    }

    public class SearchRequestModel
    {
        public Guid CustomerId { get; set; }
        public double PickupLocation { get; set; }
        public double DropOffLocation { get; set; }
        public long Price { get; set; }
        public SearchRequestStatus Status { get; set; }
    }
}
