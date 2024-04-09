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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core;

public interface IBookingPaymentService
{
    Task<ResultModel> Create(BookingPaymentCreateModel model, Guid DriverId);
    Task<ResultModel> GetByBookingId(Guid BookingId);
    Task<ResultModel> ConfirmPaid(Guid BookingPaymentId, Guid DriverId);
}
public class BookingPaymentService : IBookingPaymentService
{

    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public BookingPaymentService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> ConfirmPaid(Guid BookingPaymentId, Guid DriverId)
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
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be Driver";
                return result;
            }
            var bookingPayment = _dbContext.BookingPayments
                .Include(_ => _.Booking)
                .Where(_ => _.Id == BookingPaymentId && !_.IsDeleted).FirstOrDefault();
            if (bookingPayment == null)
            {
                result.ErrorMessage = "BookingPayment not exist";
                return result;
            }
            bookingPayment.IsPaid = true;
            bookingPayment.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingPaymentModel>(bookingPayment);
            data.Booking = _mapper.Map<BookingModel>(bookingPayment.Booking);
            result.Succeed = true;
            result.Data = data;

        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> Create(BookingPaymentCreateModel model, Guid DriverId)
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
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be Driver";
                return result;
            }
            var booking = _dbContext.Bookings
                .Include(_ => _.SearchRequest)
                .Where(_ => _.Id == model.BookingId && !_.IsDeleted).FirstOrDefault();
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
            if (booking.DriverId != DriverId)
            {
                result.ErrorMessage = "Driver don't have permission";
                return result;
            }
            var checkExist = _dbContext.BookingPayments.Where(_ => _.BookingId == booking.Id && !_.IsDeleted).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = "Booking has been had Booking Payment";
                return result;
            }
            var bookingPayment = _mapper.Map<BookingPaymentCreateModel, BookingPayment>(model);
            _dbContext.BookingPayments.Add(bookingPayment);
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<BookingPaymentModel>(bookingPayment);
            data.Booking = _mapper.Map<BookingModel>(booking);
            result.Succeed = true;
            result.Data = data;
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
            var bookingPayment = _dbContext.BookingPayments
                .Include(_ => _.Booking)
                .Where(_ => _.BookingId == BookingId && !_.IsDeleted).FirstOrDefault();
            if (bookingPayment == null)
            {
                result.ErrorMessage = "BookingPayment not exist";
                return result;
            }

            var data = _mapper.Map<BookingPaymentModel>(bookingPayment);
            data.Booking = _mapper.Map<BookingModel>(bookingPayment.Booking);
            result.Succeed = true;
            result.Data = data;

        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}