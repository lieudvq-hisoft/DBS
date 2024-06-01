using AutoMapper;
using Confluent.Kafka;
using Data.Common.PaginationModel;
using Data.DataAccess;
using Data.Entities;
using Data.Enums;
using Data.Model;
using Data.Models;
using Data.Utils;
using Data.Utils.Paging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Services.Core;
public interface IBookingService
{
    Task<ResultModel> CreateBooking(BookingCreateModel model);
    Task<ResultModel> GetBooking(Guid BookingId);
    Task<ResultModel> GetBookingsForAdmin(PagingParam<SortBookingCriteria> paginationModel, Guid UserId);
    Task<ResultModel> GetBookingForCustomer(PagingParam<SortBookingCriteria> paginationModel, Guid CustomerId);
    Task<ResultModel> GetBookingForDriver(PagingParam<SortBookingCriteria> paginationModel, Guid DriverId);
    Task<ResultModel> ChangeStatusToArrived(ChangeBookingStatusModel model, Guid DriverId);
    Task<ResultModel> ChangeStatusToCheckIn(ChangeBookingStatusModel model, Guid DriverId);
    Task<ResultModel> ChangeStatusToOnGoing(ChangeBookingStatusModel model, Guid DriverId);
    Task<ResultModel> ChangeStatusToCheckOut(ChangeBookingStatusModel model, Guid DriverId);
    Task<ResultModel> ChangeStatusToComplete(ChangeBookingStatusModel model, Guid DriverId);
    Task<ResultModel> DriverCancelBooking(ChangeBookingStatusModel model, Guid DriverId);
    Task<ResultModel> CustomerCancelBooking(ChangeBookingStatusModel model, Guid CustomerId);
    Task<ResultModel> AddBookingCheckInNote(AddCheckInNoteModel model, Guid DriverId);
    Task<ResultModel> AddBookingCheckOutNote(AddCheckOutNoteModel model, Guid DriverId);
    Task<ResultModel> ResetBooking();

}
public class BookingService : IBookingService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public BookingService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> CreateBooking(BookingCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var searchRequest = _dbContext.SearchRequests
                .Include(_ => _.Customer)
                .Where(_ => _.Id == model.SearchRequestId && !_.IsDeleted)
                .FirstOrDefault();
            if (searchRequest == null)
            {
                result.ErrorMessage = "Search Request not exist";
                return result;
            }
            if (searchRequest.Status != SearchRequestStatus.Processing)
            {
                result.ErrorMessage = "Search Request is not Processing";
                return result;
            }
            var checkExist = _dbContext.Bookings.Where(_ => _.SearchRequestId == searchRequest.Id).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = "Booking with Search Request is exist";
                return result;
            }
            var driver = _dbContext.Users
                .Include(_ => _.DriverLocations)
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == model.DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var booking = _mapper.Map<BookingCreateModel, Booking>(model);
            _dbContext.Bookings.Add(booking);

            searchRequest.Status = SearchRequestStatus.Completed;
            searchRequest.DateUpdated = DateTime.Now;

            var driverStatus = driver.DriverStatuses.FirstOrDefault();
            driverStatus.IsFree = false;
            driverStatus.IsOnline = true;
            driverStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(driverStatus);

            driver.TotalRequest += 1;

            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);

            data.Customer = _mapper.Map<UserModel>(searchRequest.Customer);

            var driverLocation = driver.DriverLocations.FirstOrDefault();
            if (driverLocation != null)
            {
                var location = _mapper.Map<LocationModel>(driverLocation);
                data.DriverLocation = location;

                var kafkaModelLocation = new KafkaModel { UserReceiveNotice = new List<Guid>() { driver.Id }, Payload = location };
                var jsonLocation = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelLocation);
                await _producer.ProduceAsync("dbs-driver-status-busy", new Message<Null, string> { Value = jsonLocation });
                _producer.Flush();
            }

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { searchRequest.CustomerId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-create", new Message<Null, string> { Value = json });
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

    public async Task<ResultModel> GetBooking(Guid BookingId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var booking = _dbContext.Bookings
                .Include(_ => _.SearchRequest)
                    .ThenInclude(_ => _.Customer)
                .Include(_ => _.Driver)
                .Where(_ => _.Id == BookingId && !_.IsDeleted).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);
            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetBookingsForAdmin(PagingParam<SortBookingCriteria> paginationModel, Guid UserId)
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
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "The user must be Admin Or Staff";
                return result;
            }
            var data = _dbContext.Bookings
                .Where(_ => !_.IsDeleted);
            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookings = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookings = bookings.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<BookingModel>(bookings);

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

    public async Task<ResultModel> GetBookingForCustomer(PagingParam<SortBookingCriteria> paginationModel, Guid CustomerId)
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
            var data = _dbContext.Bookings
                .Include(_ => _.Driver)
                .Include(_ => _.SearchRequest)
                    .ThenInclude(sr => sr.Customer)
                .Where(_ => _.SearchRequest.CustomerId == CustomerId && !_.IsDeleted);
            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookings = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookings = bookings.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<BookingModel>(bookings);
            foreach (var item in viewModels)
            {
                item.Customer = _mapper.Map<UserModel>(customer); ;
            }
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

    public async Task<ResultModel> GetBookingForDriver(PagingParam<SortBookingCriteria> paginationModel, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist";
                return result;
            }
            var data = _dbContext.Bookings
                .Include(_ => _.Driver)
                .Include(_ => _.SearchRequest)
                 .ThenInclude(sr => sr.Customer)
                .Where(_ => _.DriverId == DriverId && !_.IsDeleted);

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookings = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookings = bookings.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.Map<List<BookingModel>>(bookings);
            foreach (var item in viewModels)
            {
                foreach (var booking in bookings)
                {
                    var itemId = item.Id;
                    var bookingId = booking.Id;
                    if (itemId == bookingId)
                    {
                        var customer = booking.SearchRequest.Customer;
                        item.Customer = _mapper.Map<UserModel>(customer);
                    }
                }
            }

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

    public async Task<ResultModel> ResetBooking()
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookings = _dbContext.Bookings
                .Include(_ => _.Driver)
                .Include(_ => _.SearchRequest)
                 .ThenInclude(sr => sr.Customer)
                .Where(_ => !_.IsDeleted);
            if (bookings == null || !bookings.Any())
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            foreach (var booking in bookings)
            {
                booking.Status = BookingStatus.Accept;

            }
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<List<BookingModel>>(bookings);
            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangeStatusToArrived(ChangeBookingStatusModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status != BookingStatus.Accept)
            {
                result.ErrorMessage = "Booking Status must be Accept";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }
            booking.Status = BookingStatus.Arrived;
            booking.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-status-arrived", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangeStatusToCheckIn(ChangeBookingStatusModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status != BookingStatus.Arrived)
            {
                result.ErrorMessage = "Booking Status must be Arrived";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }
            booking.Status = BookingStatus.CheckIn;
            booking.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

            var kafkaModelDriver = new KafkaModel { UserReceiveNotice = new List<Guid>() { data.Customer.Id }, Payload = data };
            var jsonDriver = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelDriver);
            await _producer.ProduceAsync("dbs-booking-status-checkin", new Message<Null, string> { Value = jsonDriver });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangeStatusToOnGoing(ChangeBookingStatusModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status != BookingStatus.CheckIn)
            {
                result.ErrorMessage = "Booking Status must be CheckIn";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }
            booking.Status = BookingStatus.OnGoing;
            booking.DateUpdated = DateTime.Now;
            booking.PickUpTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-status-ongoing", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangeStatusToCheckOut(ChangeBookingStatusModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status != BookingStatus.OnGoing)
            {
                result.ErrorMessage = "Booking Status must be OnGoing";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }
            booking.Status = BookingStatus.CheckOut;
            booking.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

            var kafkaModelDriver = new KafkaModel { UserReceiveNotice = new List<Guid>() { data.Customer.Id }, Payload = data };
            var jsonDriver = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelDriver);
            await _producer.ProduceAsync("dbs-booking-status-checkout", new Message<Null, string> { Value = jsonDriver });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangeStatusToComplete(ChangeBookingStatusModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users
                .Include(_ => _.DriverLocations)
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status != BookingStatus.CheckOut)
            {
                result.ErrorMessage = "Booking Status must be CheckOut";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }

            var wallet = _dbContext.Wallets.Where(_ => _.UserId == driver.Id).FirstOrDefault();
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

            var bookingPrice = booking.SearchRequest.Price;
            var priceConfiguration = _dbContext.PriceConfigurations.FirstOrDefault();
            var driverProfit = priceConfiguration.DriverProfit;

            var driverProfitMoney = (long)(bookingPrice * (driverProfit.Price / 100.0));

            var walletTransaction = new WalletTransaction
            {
                TotalMoney = driverProfitMoney,
                TypeWalletTransaction = TypeWalletTransaction.Income,
                WalletId = wallet.Id,
                Status = WalletTransactionStatus.Success,
            };
            _dbContext.WalletTransactions.Add(walletTransaction);

            var walletAdminTransaction = new WalletTransaction
            {
                TotalMoney = driverProfitMoney,
                TypeWalletTransaction = TypeWalletTransaction.DriverIncome,
                WalletId = walletAdmin.Id,
                Status = WalletTransactionStatus.Success,
            };
            _dbContext.WalletTransactions.Add(walletAdminTransaction);

            wallet.TotalMoney += driverProfitMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            walletAdmin.TotalMoney -= driverProfitMoney;
            walletAdmin.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(walletAdmin);

            var driverStatus = driver.DriverStatuses.FirstOrDefault();
            driverStatus.IsFree = true;
            driverStatus.IsOnline = true;
            driverStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(driverStatus);

            driver.LastTripTime = DateTime.Now;
            if (driver.Priority < 4)
            {
                driver.Priority += 0.1f;
            }
            _dbContext.Users.Update(driver);

            booking.Status = BookingStatus.Complete;
            booking.DateUpdated = DateTime.Now;
            booking.DropOffTime = DateTime.Now;

            var customer = booking.SearchRequest.Customer;
            if (customer.Priority < 4)
            {
                customer.Priority += 0.1f;
            }
            _dbContext.Users.Update(customer);

            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);
            data.Status = BookingStatus.Complete;
            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-status-complete", new Message<Null, string> { Value = json });
            _producer.Flush();

            //var payloadWalletAdmin = _mapper.Map<WalletModel>(walletAdmin);
            //var kafkaModelWalletAdmin = new KafkaModel { UserReceiveNotice = new List<Guid>() { admin.Id }, Payload = payloadWalletAdmin };
            //var jsonWalletAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWalletAdmin);
            //await _producer.ProduceAsync("dbs-wallet-driverincome-admin", new Message<Null, string> { Value = jsonWalletAdmin });
            //_producer.Flush();

            var payloadWallet = _mapper.Map<WalletModel>(wallet);
            var kafkaModelWallet = new KafkaModel { UserReceiveNotice = new List<Guid>() { driver.Id }, Payload = payloadWallet };
            var jsonWallet = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWallet);
            await _producer.ProduceAsync("dbs-wallet-income-driver", new Message<Null, string> { Value = jsonWallet });
            _producer.Flush();

            var driverLocations = _mapper.Map<LocationModel>(driver.DriverLocations.FirstOrDefault());
            var kafkaModelLocation = new KafkaModel { UserReceiveNotice = new List<Guid>() { driver.Id }, Payload = driverLocations };
            var jsonLocation = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelLocation);
            await _producer.ProduceAsync("dbs-driver-status-free", new Message<Null, string> { Value = jsonLocation });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DriverCancelBooking(ChangeBookingStatusModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users
                .Include(_ => _.DriverLocations)
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status == BookingStatus.OnGoing || booking.Status == BookingStatus.Complete)
            {
                result.ErrorMessage = "Booking Status not suitable for Cancel";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
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
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-driver-cancel", new Message<Null, string> { Value = json });
            _producer.Flush();

            //var payloadWalletAdmin = _mapper.Map<WalletModel>(walletAdmin);
            //var kafkaModelWalletAdmin = new KafkaModel { UserReceiveNotice = new List<Guid>() { admin.Id }, Payload = payloadWalletAdmin };
            //var jsonWalletAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWalletAdmin);
            //await _producer.ProduceAsync("dbs-wallet-refund-admin", new Message<Null, string> { Value = jsonWalletAdmin });
            //_producer.Flush();

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

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> CustomerCancelBooking(ChangeBookingStatusModel model, Guid CustomerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == CustomerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not found";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "User must be a Customer";
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
            if (booking.Status == BookingStatus.OnGoing || booking.Status == BookingStatus.Complete)
            {
                result.ErrorMessage = "Booking Status not suitable for Cancel";
                return result;
            }
            if (booking.SearchRequest.CustomerId != CustomerId)
            {
                result.ErrorMessage = "Customer don't have permission";
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
                .Include(_ => _.DriverLocations)
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
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.DriverId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-customer-cancel", new Message<Null, string> { Value = json });
            _producer.Flush();

            //var payloadWalletAdmin = _mapper.Map<WalletModel>(walletAdmin);
            //var kafkaModelWalletAdmin = new KafkaModel { UserReceiveNotice = new List<Guid>() { admin.Id }, Payload = payloadWalletAdmin };
            //var jsonWalletAdmin = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelWalletAdmin);
            //await _producer.ProduceAsync("dbs-wallet-refund-admin", new Message<Null, string> { Value = jsonWalletAdmin });
            //_producer.Flush();

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

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> AddBookingCheckInNote(AddCheckInNoteModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status != BookingStatus.CheckIn)
            {
                result.ErrorMessage = "Booking Status must be CheckIn";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }

            booking.CheckInNote = model.CheckInNote;
            booking.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> AddBookingCheckOutNote(AddCheckOutNoteModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not found";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "User must be a Driver";
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
            if (booking.Status != BookingStatus.CheckOut)
            {
                result.ErrorMessage = "Booking Status must be CheckOut";
                return result;
            }
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }

            booking.CheckOutNote = model.CheckOutNote;
            booking.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingModel>(booking);
            data.Customer = _mapper.Map<UserModel>(booking.SearchRequest.Customer);

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
