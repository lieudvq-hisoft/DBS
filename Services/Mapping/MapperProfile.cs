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
            CreateMap<RegisterDriverByAdminModel, User>();
            CreateMap<User, UserModel>();
            CreateMap<User, UserModelByAdmin>();
            CreateMap<User, ProfileModel>();

            CreateMap<SearchRequestCreateModel, SearchRequest>();
            CreateMap<SearchRequest, SearchRequestModel>();
            CreateMap<SearchRequestDetailCreateModel, SearchRequestDetail>();
            CreateMap<SearchRequestDetail, SearchRequestDetailModel>();

            CreateMap<IdentityCardCreateModel, IdentityCard>();
            CreateMap<IdentityCard, IdentityCardModel>();
            CreateMap<IdentityCardImageCreateModel, IdentityCardImage>();
            CreateMap<IdentityCardImage, IdentityCardImageModel>();

            CreateMap<BookingCreateModel, Booking>();
            CreateMap<Booking, BookingModel>();

            CreateMap<BookingVehicleCreateModel, BookingVehicle>();
            CreateMap<BookingVehicle, BookingVehicleModel>();

            CreateMap<BookedPersonInfoCreateModel, BookedPersonInfo>();
            CreateMap<BookedPersonInfo, BookedPersonInfoModel>();

            CreateMap<RatingCreateModel, Rating>();
            CreateMap<Rating, RatingModel>();

            CreateMap<BookingPaymentCreateModel, BookingPayment>();
            CreateMap<BookingPayment, BookingPaymentModel>();

            CreateMap<BookingImageCreateModel, BookingImage>();
            CreateMap<BookingImage, BookingImageModel>();

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

            CreateMap<SupportCreateModel, Support>();
            CreateMap<Support, SupportModel>();
        }
    }
}
