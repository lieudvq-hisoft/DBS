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

            CreateMap<DrivingLicenseCreateModel, DrivingLicense>();
            CreateMap<DrivingLicense, DrivingLicenseModel>();
            CreateMap<DrivingLicenseImageCreateModel, DrivingLicenseImage>();
            CreateMap<DrivingLicenseImage, DrivingLicenseImageModel>();


            CreateMap<LocationModel, DriverLocation>();
        }
    }
}
