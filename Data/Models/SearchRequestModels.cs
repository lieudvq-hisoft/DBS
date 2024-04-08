﻿using Data.Entities;
using Data.Enums;
using Data.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Model
{
    public class SearchRequestCreateModel
    {
        public Guid DriverId { get; set; }
        public double PickupLongitude { get; set; }
        public double PickupLatitude { get; set; }
        public double DropOffLongitude { get; set; }
        public double DropOffLatitude { get; set; }
        public string DropOffAddress { get; set; }
        public string PickupAddress { get; set; }
        public BookingVehicleModel BookingVehicle { get; set; }
        public long Price { get; set; }
    }

    public class SearchRequestModel
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DriverId { get; set; }
        public UserModel Customer { get; set; }
        public double PickupLongitude { get; set; }
        public double PickupLatitude { get; set; }
        public double DropOffLongitude { get; set; }
        public double DropOffLatitude { get; set; }
        public string DropOffAddress { get; set; }
        public string PickupAddress { get; set; }
        public BookingVehicleModel BookingVehicle { get; set; }
        public long Price { get; set; }
        public SearchRequestStatus Status { get; set; }
    }

    public class NewDriverModel
    {
        public Guid SearchRequestId { get; set; }
        public Guid DriverId { get; set; }
    }
}
