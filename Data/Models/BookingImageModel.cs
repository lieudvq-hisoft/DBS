using Data.Enums;
using Microsoft.AspNetCore.Http;

namespace Data.Models;

public class BookingImageCreateModel
{
    public Guid BookingId { get; set; }
    public IFormFile File { get; set; }
    public BookingImageType BookingImageType { get; set; }
}

public class BookingImageUpdateModel
{
    public IFormFile File { get; set; }
}

public class BookingImageModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public BookingModel Booking { get; set; }
    public string ImageUrl { get; set; }
    public BookingImageType BookingImageType { get; set; }
    public BookingImageTime BookingImageTime { get; set; }
    public DateTime DateCreated { get; set; }
}
