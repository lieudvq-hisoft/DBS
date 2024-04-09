using AutoMapper;
using Data.Entities;
using Data.Model;
using Data.Models;

namespace Services.Mapping
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<RegisterModel, User>();
            CreateMap<User, UserModel>();
            CreateMap<User, ProfileModel>();

            CreateMap<SearchRequestCreateModel, SearchRequest>();
            CreateMap<SearchRequest, SearchRequestModel>();

            CreateMap<IdentityCardCreateModel, IdentityCard>();
            CreateMap<IdentityCard, IdentityCardModel>();
            CreateMap<IdentityCardImageCreateModel, IdentityCardImage>();
            CreateMap<IdentityCardImage, IdentityCardImageModel>();

            CreateMap<BookingCreateModel, Booking>();
            CreateMap<Booking, BookingModel>();

            CreateMap<BookingVehicleModel, BookingVehicle>();
            CreateMap<BookingVehicle, BookingVehicleModel>();

            CreateMap<RatingCreateModel, Rating>();
            CreateMap<Rating, RatingModel>();

            CreateMap<VehicleCreateModel, Vehicle>();
            CreateMap<Vehicle, VehicleModel>();
            CreateMap<VehicleImageCreateModel, VehicleImage>();
            CreateMap<VehicleImage, VehicleImageModel>();

            CreateMap<DrivingLicenseCreateModel, DrivingLicense>();
            CreateMap<DrivingLicense, DrivingLicenseModel>();
            CreateMap<DrivingLicenseImageCreateModel, DrivingLicenseImage>();
            CreateMap<DrivingLicenseImage, DrivingLicenseImageModel>();

            CreateMap<LocationModel, DriverLocation>();
            CreateMap<DriverLocation, LocationModel>();
        }
    }
}
