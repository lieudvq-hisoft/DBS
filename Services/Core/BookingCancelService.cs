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
    Task<ResultModel> AddImage(BookingCancelImageCreateModel model);
    Task<ResultModel> GetImagesByBookingCancelId(Guid BookingCancelId);
    Task<ResultModel> DeleteImage(Guid BookingCancelImageId);
    Task<ResultModel> DownloadImage(FileModel model);
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
            var bookingCancel = _mapper.Map<BookingCancelCreateModel, BookingCancel>(model);
            bookingCancel.CancelPersonId = CustomerId;
            _dbContext.BookingCancels.Add(bookingCancel);

            var driver = _dbContext.Users.Where(_ => _.Id == booking.DriverId)
                .Include(_ => _.DriverStatuses)
                .FirstOrDefault();
            var driverStatus = driver.DriverStatuses.FirstOrDefault();
            driverStatus.IsFree = true;
            driverStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(driverStatus);

            booking.Status = BookingStatus.Cancel;
            booking.DateUpdated = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingCancelModel>(bookingCancel);
            data.Booking = _mapper.Map<BookingModel>(booking);
            data.CancelPerson = _mapper.Map<UserModel>(customer);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.DriverId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-customer-cancel", new Message<Null, string> { Value = json });
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
            var checkExist = _dbContext.BookingCancels.Where(_ => _.BookingId == model.BookingId && !_.IsDeleted).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = "Booking has already canceled";
                return result;
            }
            var bookingCancel = _mapper.Map<BookingCancelCreateModel, BookingCancel>(model);
            bookingCancel.CancelPersonId = DriverId;
            _dbContext.BookingCancels.Add(bookingCancel);

            var driverStatus = driver.DriverStatuses.FirstOrDefault();
            driverStatus.IsFree = true;
            driverStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(driverStatus);

            booking.Status = BookingStatus.Cancel;
            booking.DateUpdated = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingCancelModel>(bookingCancel);
            data.Booking = _mapper.Map<BookingModel>(booking);
            data.CancelPerson = _mapper.Map<UserModel>(driver);

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.SearchRequest.CustomerId }, Payload = data };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-booking-driver-cancel", new Message<Null, string> { Value = json });
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
                .Include(_ => _.CancelPerson)
                .Where(_ => _.CancelPersonId == UserId && !_.IsDeleted);

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookingCancels = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookingCancels = bookingCancels.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<BookingCancelModel>(bookingCancels);

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


    public async Task<ResultModel> AddImage(BookingCancelImageCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookingCancel = _dbContext.BookingCancels
                .Include(_ => _.Booking)
                .Include(_ => _.CancelPerson)
                .Where(_ => _.Id == model.BookingCancelId && !_.IsDeleted).FirstOrDefault();
            if (bookingCancel == null)
            {
                result.ErrorMessage = "Booking Canncel not exist";
                return result;
            }

            var bookingCancelImage = _mapper.Map<BookingCancelImageCreateModel, BookingCancelImage>(model);
            _dbContext.BookingCancelImages.Add(bookingCancelImage);
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "BookingCancelImage", bookingCancelImage.Id.ToString());
            bookingCancelImage.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<BookingCancelImageModel>(bookingCancelImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetImagesByBookingCancelId(Guid BookingCancelId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookingCancel = _dbContext.BookingCancels.Where(_ => _.Id == BookingCancelId && !_.IsDeleted).FirstOrDefault();
            if (bookingCancel == null)
            {
                result.ErrorMessage = "Booking Cancel not exist";
                return result;
            }
            var bookingCancelImage = _dbContext.BookingCancelImages.Where(_ => _.BookingCancelId == BookingCancelId && !_.IsDeleted).ToList();
            if (bookingCancelImage == null || bookingCancelImage.Count == 0)
            {
                result.ErrorMessage = "Driving License Image not exist!";
                return result;
            }
            var data = _mapper.Map<List<BookingCancelImageModel>>(bookingCancelImage);
            foreach (var item in data)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + item.ImageUrl;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                item.ImageUrl = Convert.ToBase64String(imageBytes);
            }

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteImage(Guid BookingCancelImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookingCancelImage = _dbContext.BookingCancelImages.Where(_ => _.Id == BookingCancelImageId && !_.IsDeleted).FirstOrDefault();
            if (bookingCancelImage == null)
            {
                result.ErrorMessage = "Booking Cancel Image not exist!";
                return result;
            }
            string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            MyFunction.DeleteFile(dirPathDelete + bookingCancelImage.ImageUrl);

            _dbContext.BookingCancelImages.Remove(bookingCancelImage);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Booking Cancel Image successful";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DownloadImage(FileModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookingCancelImage = _dbContext.BookingCancelImages.Where(_ => _.Id == model.Id && !_.IsDeleted).FirstOrDefault();
            if (bookingCancelImage == null)
            {
                result.Succeed = false;
                result.ErrorMessage = "Driving License Image not found";
            }
            else
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                if (bookingCancelImage.ImageUrl == null || !bookingCancelImage.ImageUrl.Contains(model.Path))
                {
                    result.ErrorMessage = "Image does not exist";
                    result.Succeed = false;
                    return result;
                }
                result.Data = await MyFunction.DownloadFile(dirPath + model.Path);
                result.Succeed = true;
            }
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
                .Include(_ => _.CancelPerson)
                .Where(_ => _.CancelPersonId == UserId && !_.IsDeleted);

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookingCancels = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookingCancels = bookingCancels.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<BookingCancelModel>(bookingCancels);

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
