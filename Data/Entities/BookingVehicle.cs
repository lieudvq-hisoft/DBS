﻿
namespace Data.Entities;

public class BookingVehicle : BaseEntity
{
    public string LicensePlate { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Color { get; set; }
    public string ImageUrl { get; set; }
}
