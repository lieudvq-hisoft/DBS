
using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.Utils;

namespace Services.Core;

public interface IBookingImageService
{
    Task<ResultModel> AddImage(BookingImageCreateModel model);
    Task<ResultModel> GetImagesByBookingId(Guid BookingId);
    Task<ResultModel> UpdateImage(BookingImageUpdateModel model, Guid BookingImageId);
    Task<ResultModel> DeleteImage(Guid BookingImageId);
    Task<ResultModel> DownloadImage(FileModel model);
}

public class BookingImageService : IBookingImageService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public BookingImageService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> AddImage(BookingImageCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var booking = _dbContext.Bookings.Where(_ => _.Id == model.BookingId && !_.IsDeleted).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist!";
                return result;
            }
            if (booking.Status != Data.Enums.BookingStatus.Arrived)
            {
                result.ErrorMessage = "Driver not Arrived";
                return result;
            }
            var bookingImage = _mapper.Map<BookingImageCreateModel, BookingImage>(model);
            _dbContext.BookingImages.Add(bookingImage);
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "BookingImage", bookingImage.Id.ToString());
            bookingImage.ImageData = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<BookingImageModel>(bookingImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteImage(Guid BookingImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookingImage = _dbContext.BookingImages.Where(_ => _.Id == BookingImageId && !_.IsDeleted).FirstOrDefault();
            if (bookingImage == null)
            {
                result.ErrorMessage = "Booking Image not exist!";
                return result;
            }
            string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            MyFunction.DeleteFile(dirPathDelete + bookingImage.ImageData);

            _dbContext.BookingImages.Remove(bookingImage);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Booking Image successful";
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
            var bookingImage = _dbContext.BookingImages.Where(_ => _.Id == model.Id && !_.IsDeleted).FirstOrDefault();
            if (bookingImage == null)
            {
                result.Succeed = false;
                result.ErrorMessage = "Booking Image not found";
            }
            else
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                if (bookingImage.ImageData == null || !bookingImage.ImageData.Contains(model.Path))
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

    public async Task<ResultModel> GetImagesByBookingId(Guid BookingId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookingImages = _dbContext.BookingImages
                .Include(_ => _.Booking)
                .Where(_ => _.BookingId == BookingId && !_.IsDeleted)
                .ToList();
            if (bookingImages == null || bookingImages.Count == 0)
            {
                result.ErrorMessage = "Booking Image not exist!";
                return result;
            }
            var data = _mapper.Map<List<BookingImageModel>>(bookingImages);
            foreach (var item in bookingImages)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + item.ImageData;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                item.ImageData = Convert.ToBase64String(imageBytes);
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

    public async Task<ResultModel> UpdateImage(BookingImageUpdateModel model, Guid BookingImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var bookingImage = _dbContext.BookingImages
                .Include(_ => _.Booking)
                .Where(_ => _.Id == BookingImageId && !_.IsDeleted).FirstOrDefault();
            if (bookingImage == null)
            {
                result.ErrorMessage = "Booking Image not exist!";
                return result;
            }
            if (model.File != null)
            {
                string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                MyFunction.DeleteFile(dirPathDelete + bookingImage.ImageData);
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "BookingImage", bookingImage.Id.ToString());
                bookingImage.ImageData = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            }
            bookingImage.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<BookingImageModel>(bookingImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}