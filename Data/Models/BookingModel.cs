
using Data.Entities;
using Data.Enums;
using Data.Model;

namespace Data.Models
{
    public class ChangeBookingStatusModel
    {
        public Guid BookingId { get; set; }
    }

    public class AddCheckInNoteModel
    {
        public Guid BookingId { get; set; }
        public string CheckInNote { get; set; }
    }

    public class BookingCreateModel
    {
        public Guid SearchRequestId { get; set; }
        public Guid DriverId { get; set; }
    }

    public class BookingModel
    {
        public Guid Id { get; set; }
        public Guid SearchRequestId { get; set; }
        public Guid DriverId { get; set; }
        public string CheckInNote { get; set; }
        public DateTime? PickUpTime { get; set; }
        public DateTime? DropOffTime { get; set; }
        public BookingStatus Status { get; set; }
        public SearchRequestModel SearchRequest { get; set; }
        public UserModel Driver { get; set; }
        public UserModel Customer { get; set; }
        public LocationModel DriverLocation { get; set; }
    }

}
