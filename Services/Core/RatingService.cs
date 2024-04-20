using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Data.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.Utils;

namespace Services.Core;
public interface IRatingService
{
    Task<ResultModel> Add(RatingCreateModel model, Guid UserId);
    Task<ResultModel> GetById(Guid RatingId);
    Task<ResultModel> GetByBookingId(Guid BookingId);
    Task<ResultModel> GetByDriverId(Guid UserId);
    Task<ResultModel> UpdateRating(RatingUpdateModel model, Guid RatingId, Guid UserId);
    Task<ResultModel> CheckBookingCanRating(Guid BookingId);

}

public class RatingService : IRatingService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public RatingService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> Add(RatingCreateModel model, Guid CustomerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == CustomerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "User not exist";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be Customer";
                return result;
            }
            var booking = _dbContext.Bookings.Where(_ => _.Id == model.BookingId && !_.IsDeleted).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            if (booking.Status != Data.Enums.BookingStatus.Complete)
            {
                result.ErrorMessage = "Booking not Complete";
                return result;
            }
            var checkExistRating = _dbContext.Ratings.Where(_ => _.BookingId == booking.Id && !_.IsDeleted).FirstOrDefault();
            if (checkExistRating != null)
            {
                result.ErrorMessage = "Booking has been rated";
                return result;
            }

            var rating = _mapper.Map<RatingCreateModel, Rating>(model);
            rating.BookingId = booking.Id;
            _dbContext.Ratings.Add(rating);

            if (rating.ImageUrl != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "RatingImage", rating.Id.ToString());
                rating.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            }

            var driver = _dbContext.Users.Where(_ => _.Id == booking.DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist";
                return result;
            }
            if (driver.Star == null)
            {
                driver.Star = rating.Star;
            }
            else
            {
                var star = 0;
                var ratings = _dbContext.Ratings
                    .Include(_ => _.Booking)
                    .Where(_ => _.Booking.DriverId == driver.Id).ToList();
                foreach (var item in ratings)
                {
                    star += item.Star;
                }
                var calculateStar = (float)(star + model.Star) / (ratings.Count + 1);
                driver.Star = (float)Math.Round(calculateStar, 1);
            }

            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<RatingModel>(rating);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetByBookingId(Guid BookingId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var rating = _dbContext.Ratings
                .Include(_ => _.Booking)
                .Where(_ => _.BookingId == BookingId && !_.IsDeleted).FirstOrDefault();
            if (rating == null)
            {
                result.ErrorMessage = "Rating not exist";
                return result;
            }

            result.Succeed = true;
            result.Data = _mapper.Map<RatingModel>(rating);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetByDriverId(Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "User not exist";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be Driver";
                return result;
            }
            var ratings = _dbContext.Ratings
                .Include(_ => _.Booking)
                .Where(_ => _.Booking.DriverId == DriverId && !_.IsDeleted).ToList();
            if (ratings == null)
            {
                result.ErrorMessage = "Rating not exist";
                return result;
            }

            result.Succeed = true;
            result.Data = _mapper.Map<List<RatingModel>>(ratings);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetById(Guid RatingId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var rating = _dbContext.Ratings
                .Include(_ => _.Booking)
                .Where(_ => _.Id == RatingId && !_.IsDeleted).FirstOrDefault();
            if (rating == null)
            {
                result.ErrorMessage = "Rating not exist";
                return result;
            }

            result.Succeed = true;
            result.Data = _mapper.Map<RatingModel>(rating);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateRating(RatingUpdateModel model, Guid RatingId, Guid CustomerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == CustomerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "User not exist";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be Customer";
                return result;
            }
            var rating = _dbContext.Ratings
                .Include(_ => _.Booking)
                .Where(_ => _.Id == RatingId && !_.IsDeleted).FirstOrDefault();
            if (rating == null)
            {
                result.ErrorMessage = "Rating not exist";
                return result;
            }
            if (model.Star != null)
            {
                rating.Star = model.Star;

                var driver = _dbContext.Users.Where(_ => _.Id == rating.Booking.DriverId && !_.IsDeleted).FirstOrDefault();
                if (driver != null)
                {
                    var star = 0;
                    var ratings = _dbContext.Ratings
                        .Include(_ => _.Booking)
                        .Where(_ => _.Booking.DriverId == driver.Id).ToList();
                    foreach (var item in ratings)
                    {
                        star += item.Star;
                    }
                    driver.Star = (star + model.Star) / (ratings.Count + 1);
                }
            }
            if (model.Comment != null)
            {
                rating.Comment = model.Comment;
            }
            if (model.File != null)
            {
                string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                MyFunction.DeleteFile(dirPathDelete + rating.ImageUrl);
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "RatingImage", rating.Id.ToString());
                rating.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            }

            rating.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<RatingModel>(rating);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> CheckBookingCanRating(Guid BookingId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var booking = _dbContext.Bookings.Where(_ => _.Id == BookingId && !_.IsDeleted).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            var rating = _dbContext.Ratings
                .Include(_ => _.Booking)
                .Where(_ => _.BookingId == BookingId && !_.IsDeleted).FirstOrDefault();

            var dropOffTime = booking.DropOffTime;
            var now = DateTime.Now.AddDays(-2);
            if (rating == null && booking.DropOffTime >= DateTime.Now.AddDays(-2))
            {
                result.Data = true;
            }
            else
            {
                result.Data = false;
            }

            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}
