
namespace Data.Models
{
    public class BookingCreateModel
    {
        public Guid SearchRequestId { get; set; }
        public Guid DriverId { get; set; }
    }
    public class BookingModel
    {
        public Guid SearchRequestId { get; set; }
        public Guid DriverId { get; set; }
    }
}
