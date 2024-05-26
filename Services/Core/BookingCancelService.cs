using Data.Model;
using Data.Models;
using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Data.Utils;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Data.Common.PaginationModel;
using Data.Enums;
using Data.Utils.Paging;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using Services.Utils;

namespace Services.Core;

public interface IBookingCancelService
{
    Task<ResultModel> CustomerCancel(BookingCancelCreateModel model, Guid CustomerId);
    Task<ResultModel> DriverCancel(BookingCancelCreateModel model, Guid DriverId);
    Task<ResultModel> Get(PagingParam<SortCriteria> paginationModel, Guid UserId);
    Task<ResultModel> GetByID(Guid BookingCancelId, Guid UserId);
    Task<ResultModel> GetForAdmin(PagingParam<SortCriteria> paginationModel, Guid UserId, Guid AdminId);
    Task<ResultModel> GetByIdForAdmin(Guid BookingCancelId, Guid AdminId);
}

public class BookingCancelService : IBookingCancelService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public BookingCancelService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> CustomerCancel(BookingCancelCreateModel model, Guid CustomerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == CustomerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be Customer";
                return result;
            }
            var booking = _dbContext.Bookings
                .Include(_ => _.Driver)
                .Include(_ => _.SearchRequest)
                 .ThenInclude(sr => sr.Customer)
                .Where(_ => _.Id == model.BookingId && !_.IsDeleted).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            if (booking.Status != BookingStatus.Accept && booking.Status != BookingStatus.Arrived && booking.Status != BookingStatus.CheckIn)
            {
                result.ErrorMessage = "Booking status do not suit for cancel";
                return result;
            }
            if (CustomerId != booking.SearchRequest.CustomerId)
            {
                result.ErrorMessage = "Customer is not belong to Booking";
                return result;
            }
            var checkExist = _dbContext.BookingCancels.Where(_ => _.BookingId == model.BookingId && !_.IsDeleted).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = "Booking has already canceled";
                return result;
            }
            if (booking.Status == BookingStatus.CheckIn)
            {
                var images = _dbContext.BookingImages.Where(_ => _.BookingId == booking.Id).ToList();
                if (images.Count > 0)
                {
                    _dbContext.BookingImages.RemoveRange(images);
                }
            }
            var bookingCancel = _mapper.Map<BookingCancelCreateModel, BookingCancel>(model);
            bookingCancel.CancelPersonId = CustomerId;
            _dbContext.BookingCancels.Add(bookingCancel);

            if (model.Files != null && model.Files.Count > 0)
            {
                bookingCancel.ImageUrls = Array.Empty<string>();
                var imgUrlsList = bookingCancel.ImageUrls.ToList();
                foreach (var file in model.Files)
                {
                    string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BookingCancelImage", bookingCancel.Id.ToString(), DateTime.Now.Ticks.ToString());
                    var ImgUrl = await MyFunction.UploadImageAsync(file, dirPath);
                    imgUrlsList.Add(ImgUrl);
                }
                bookingCancel.ImageUrls = imgUrlsList.ToArray();
            }

            var wallet = _dbContext.Wallets.Where(_ => _.UserId == booking.SearchRequest.CustomerId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }

            var admin = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Admin) && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }

            var walletAdmin = _dbContext.Wallets.Where(_ => _.UserId == admin.Id).FirstOrDefault();
            if (walletAdmin == null)
            {
                result.ErrorMessage = "Wallet Admin not exist";
                return result;
            }

            var refundMoney = booking.SearchRequest.Price;
            if (customer.Priority <= 1)
            {
                var priceConfiguration = _dbContext.PriceConfigurations.FirstOrDefault();

                var cancelFee = priceConfiguration.CustomerCancelFee;

                if (cancelFee.IsPercent.HasValue && cancelFee.IsPercent.Value)
                {
                    refundMoney -= (long)(refundMoney * (cancelFee.Price / 100.0));
                }
                else
                {
                    refundMoney -= (long)(cancelFee.Price);
                }
            }

            var walletTransaction = new WalletTransaction
            {
                TotalMoney = refundMoney,
                TypeWalletTransaction = TypeWalletTransaction.Refund,
                WalletId = wallet.Id,
                Status = WalletTransactionStatus.Success,
            };
            _dbContext.WalletTransactions.Add(walletTransaction);

            var walletAdminTransaction = new WalletTransaction
            {
                TotalMoney = refundMoney,
                TypeWalletTransaction = TypeWalletTransaction.Refund,
                WalletId = walletAdmin.Id,
                Status = WalletTransactionStatus.Success,
            };
            _dbContext.WalletTransactions.Add(walletAdminTransaction);

            wallet.TotalMoney += walletTransaction.TotalMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            walletAdmin.TotalMoney -= walletTransaction.TotalMoney;
            walletAdmin.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(walletAdmin);

            var driver = _dbContext.Users.Where(_ => _.Id == booking.DriverId)
                .Include(_ => _.DriverStatuses)
                .FirstOrDefault();
            var driverStatus = driver.DriverStatuses.FirstOrDefault();
            driverStatus.IsFree = true;
            driverStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(driverStatus);

            booking.Status = BookingStatus.Cancel;
            booking.DateUpdated = DateTime.Now;

            if (customer.Priority >= 0.5)
            {
                customer.Priority -= 0.5f;
                _dbContext.Users.Update(customer);
            }

            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.BookingCancel = _mapper.Map<BookingCancelNotiModel>(bookingCancel);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.DriverId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-customer-cancel", new Message<Null, string> { Value = json });
            _producer.Flush();

            var payloadWalletAdmin = _mapper.Map<WalletModel>(walletAdmin);
            var kafkaModelWalletAdmin = new KafkaModel { UserReceiveNotice = new List<Guid>() { admin.Id }, Payload = payloadWalletAdmin };
            var jsonWalletAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWalletAdmin);
            await _producer.ProduceAsync("dbs-wallet-refund-admin", new Message<Null, string> { Value = jsonWalletAdmin });
            _producer.Flush();

            var payloadWallet = _mapper.Map<WalletModel>(wallet);
            var kafkaModelWallet = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = payloadWallet };
            var jsonWallet = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWallet);
            await _producer.ProduceAsync("dbs-wallet-refund-customer", new Message<Null, string> { Value = jsonWallet });
            _producer.Flush();

            var driverLocations = _mapper.Map<LocationModel>(driver.DriverLocations.FirstOrDefault());
            var kafkaModelLocation = new KafkaModel { UserReceiveNotice = new List<Guid>() { driver.Id }, Payload = driverLocations };
            var jsonLocation = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelLocation);
            await _producer.ProduceAsync("dbs-driver-status-free", new Message<Null, string> { Value = jsonLocation });
            _producer.Flush();

            result.Succeed = true;
            result.Data = data;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DriverCancel(BookingCancelCreateModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Include(_ => _.DriverLocations)
                .Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Customer not exist";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be Driver";
                return result;
            }
            var booking = _dbContext.Bookings
                .Include(_ => _.Driver)
                .Include(_ => _.SearchRequest)
                 .ThenInclude(sr => sr.Customer)
                .Where(_ => _.Id == model.BookingId && !_.IsDeleted).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            if (booking.Status != BookingStatus.Accept && booking.Status != BookingStatus.Arrived && booking.Status != BookingStatus.CheckIn)
            {
                result.ErrorMessage = "Booking status do not suit for cancel";
                return result;
            }
            if (DriverId != booking.DriverId)
            {
                result.ErrorMessage = "Driver is not belong to Booking";
                return result;
            }
            if (booking.Status == BookingStatus.CheckIn)
            {
                var images = _dbContext.BookingImages.Where(_ => _.BookingId == booking.Id).ToList();
                if (images.Count > 0)
                {
                    _dbContext.BookingImages.RemoveRange(images);
                }
            }
            var checkExist = _dbContext.BookingCancels.Where(_ => _.BookingId == model.BookingId && !_.IsDeleted).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = "Booking has already canceled";
                return result;
            }
            var bookingCancel = _mapper.Map<BookingCancelCreateModel, BookingCancel>(model);
            bookingCancel.CancelPersonId = DriverId;
            _dbContext.BookingCancels.Add(bookingCancel);

            if (model.Files != null && model.Files.Count > 0)
            {
                bookingCancel.ImageUrls = Array.Empty<string>();
                var imgUrlsList = bookingCancel.ImageUrls.ToList();
                foreach (var file in model.Files)
                {
                    string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BookingCancelImage", bookingCancel.Id.ToString(), DateTime.Now.ToString());
                    var ImgUrl = await MyFunction.UploadImageAsync(file, dirPath);
                    imgUrlsList.Add(ImgUrl);
                }
                bookingCancel.ImageUrls = imgUrlsList.ToArray();
            }
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == booking.SearchRequest.CustomerId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }

            var admin = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Admin) && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }

            var walletAdmin = _dbContext.Wallets.Where(_ => _.UserId == admin.Id).FirstOrDefault();
            if (walletAdmin == null)
            {
                result.ErrorMessage = "Wallet Admin not exist";
                return result;
            }

            var walletTransaction = new WalletTransaction
            {
                TotalMoney = booking.SearchRequest.Price,
                TypeWalletTransaction = TypeWalletTransaction.Refund,
                WalletId = wallet.Id,
                Status = WalletTransactionStatus.Success,
            };
            _dbContext.WalletTransactions.Add(walletTransaction);

            var walletAdminTransaction = new WalletTransaction
            {
                TotalMoney = booking.SearchRequest.Price,
                TypeWalletTransaction = TypeWalletTransaction.Refund,
                WalletId = walletAdmin.Id,
                Status = WalletTransactionStatus.Success,
            };
            _dbContext.WalletTransactions.Add(walletAdminTransaction);

            wallet.TotalMoney += walletTransaction.TotalMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            walletAdmin.TotalMoney -= walletTransaction.TotalMoney;
            walletAdmin.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(walletAdmin);

            var driverStatus = driver.DriverStatuses.FirstOrDefault();
            driverStatus.IsFree = true;
            driverStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(driverStatus);

            booking.Status = BookingStatus.Cancel;
            booking.DateUpdated = DateTime.Now;

            if (driver.Priority >= 0.2)
            {
                driver.Priority -= 0.2f;
            }

            if (driver.Priority == 0)
            {
                driver.IsActive = false;
                var driverBan = _mapper.Map<UserModel>(driver);
                var kafkaModelBan = new KafkaModel { UserReceiveNotice = new List<Guid>() { DriverId }, Payload = driverBan };
                var jsonBan = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelBan);
                await _producer.ProduceAsync("dbs-driver-status-ban", new Message<Null, string> { Value = jsonBan });
                _producer.Flush();
            }
            else if (driver.Priority <= 1)
            {
                var kafkaModelWarning = new KafkaModel { UserReceiveNotice = new List<Guid>() { DriverId }, Payload = "" };
                var jsonWarning = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWarning);
                await _producer.ProduceAsync("dbs-driver-status-warning", new Message<Null, string> { Value = jsonWarning });
                _producer.Flush();
            }
            _dbContext.Users.Update(driver);

            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.BookingCancel = _mapper.Map<BookingCancelNotiModel>(bookingCancel);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-driver-cancel", new Message<Null, string> { Value = json });
            _producer.Flush();

            var payloadWalletAdmin = _mapper.Map<WalletModel>(walletAdmin);
            var kafkaModelWalletAdmin = new KafkaModel { UserReceiveNotice = new List<Guid>() { admin.Id }, Payload = payloadWalletAdmin };
            var jsonWalletAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWalletAdmin);
            await _producer.ProduceAsync("dbs-wallet-refund-admin", new Message<Null, string> { Value = jsonWalletAdmin });
            _producer.Flush();

            var payloadWallet = _mapper.Map<WalletModel>(wallet);
            var kafkaModelWallet = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = payloadWallet };
            var jsonWallet = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWallet);
            await _producer.ProduceAsync("dbs-wallet-refund-customer", new Message<Null, string> { Value = jsonWallet });
            _producer.Flush();

            var driverLocations = _mapper.Map<LocationModel>(driver.DriverLocations.FirstOrDefault());
            var kafkaModelLocation = new KafkaModel { UserReceiveNotice = new List<Guid>() { driver.Id }, Payload = driverLocations };
            var jsonLocation = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelLocation);
            await _producer.ProduceAsync("dbs-driver-status-free", new Message<Null, string> { Value = jsonLocation });
            _producer.Flush();

            result.Succeed = true;
            result.Data = data;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> Get(PagingParam<SortCriteria> paginationModel, Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var data = _dbContext.BookingCancels
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.SearchRequest)
                        .ThenInclude(sr => sr.Customer)
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.Driver)
                .Include(_ => _.CancelPerson)
                .Where(_ => _.CancelPersonId == UserId && !_.IsDeleted);

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookingCancels = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookingCancels = bookingCancels.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.Map<List<BookingCancelModel>>(bookingCancels);

            paging.Data = viewModels;
            result.Data = paging;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetByID(Guid BookingCancelId, Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exist";
                return result;
            }
            var bookingCancel = _dbContext.BookingCancels
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.SearchRequest)
                        .ThenInclude(sr => sr.Customer)
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.Driver)
                .Include(_ => _.CancelPerson)
                .Where(_ => _.Id == BookingCancelId && !_.IsDeleted).FirstOrDefault();
            if (bookingCancel == null)
            {
                result.ErrorMessage = "Booking Cancel not exist";
                return result;
            }
            var data = _mapper.Map<BookingCancelModel>(bookingCancel);

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetForAdmin(PagingParam<SortCriteria> paginationModel, Guid UserId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user muse be Admin";
                return result;
            }
            var data = _dbContext.BookingCancels
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.SearchRequest)
                        .ThenInclude(sr => sr.Customer)
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.Driver)
                .Include(_ => _.CancelPerson)
                .Where(_ => _.CancelPersonId == UserId && !_.IsDeleted);

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookingCancels = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookingCancels = bookingCancels.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);

            var viewModels = _mapper.Map<List<BookingCancelModel>>(bookingCancels);

            paging.Data = viewModels;
            result.Data = paging;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetByIdForAdmin(Guid BookingCancelId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user muse be Admin";
                return result;
            }
            var bookingCancel = _dbContext.BookingCancels
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.SearchRequest)
                        .ThenInclude(sr => sr.Customer)
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.Driver)
                .Include(_ => _.CancelPerson)
                .Where(_ => _.Id == BookingCancelId && !_.IsDeleted)
                .FirstOrDefault();

            var data = _mapper.Map<BookingCancelModel>(bookingCancel);

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}
