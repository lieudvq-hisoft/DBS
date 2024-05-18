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
            CreateMap<RegisterStaffByAdminModel, User>();
            CreateMap<User, UserModel>();
            CreateMap<User, UserModelByAdmin>();
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

            CreateMap<CustomerBookedOnBehalfModel, CustomerBookedOnBehalf>();
            CreateMap<CustomerBookedOnBehalf, CustomerBookedOnBehalfModel>();

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
            CreateMap<SupportBookingIssueCreateModel, Support>();
            CreateMap<Support, SupportModel>();

            CreateMap<BookingCancelCreateModel, BookingCancel>();
            CreateMap<BookingCancel, BookingCancelModel>();
            CreateMap<BookingCancel, BookingCancelNotiModel>();

            CreateMap<WalletCreateModel, Wallet>();
            CreateMap<Wallet, WalletModel>();
            CreateMap<WalletTransactionCreateModel, WalletTransaction>();
            CreateMap<WalletTransaction, WalletTransactionModel>();

            CreateMap<PaymentResponseModel, PaymentResponseModel>();
            CreateMap<MomoCreatePaymentResponseModel, MomoCreatePaymentResponseModel>();

            CreateMap<BrandVehicleCreateModel, BrandVehicle>();
            CreateMap<BrandVehicle, BrandVehicleModel>();
            CreateMap<ModelVehicleCreateModel, ModelVehicle>();
            CreateMap<ModelVehicle, ModelVehicleModel>();

            CreateMap<LinkedAccountCreateModel, LinkedAccount>();
            CreateMap<LinkedAccount, LinkedAccountModel>();
        }
    }
}
