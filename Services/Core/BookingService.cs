﻿using AutoMapper;
using Confluent.Kafka;
using Data.Common.PaginationModel;
using Data.DataAccess;
using Data.Entities;
using Data.Enums;
using Data.Model;
using Data.Models;
using Data.Utils.Paging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Core;
public interface IBookingService
{
    Task<ResultModel> CreateBooking(BookingCreateModel model);
    Task<ResultModel> GetBooking(Guid BookingId);
    Task<ResultModel> GetBookingForCustomer(PagingParam<SortCriteria> paginationModel, Guid CustomerId);
    Task<ResultModel> GetBookingForDriver(PagingParam<SortCriteria> paginationModel, Guid DriverId);

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
            var searchRequest = _dbContext.SearchRequests.Where(_ => _.Id == model.SearchRequestId && !_.IsDeleted).FirstOrDefault();
            if (searchRequest == null)
            {
                result.ErrorMessage = "Search Request not exist";
                return result;
            }
            var driver = _dbContext.Users.Where(_ => _.Id == model.DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist";
                return result;
            }
            var booking = _mapper.Map<BookingCreateModel, Booking>(model);
            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = booking.Id;
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
                .Include(_ => _.Driver)
                .Where(_ => _.Id == BookingId && !_.IsDeleted).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }

            result.Data = _mapper.Map<Booking>(booking);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetBookingForCustomer(PagingParam<SortCriteria> paginationModel, Guid CustomerId)
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

    public async Task<ResultModel> GetBookingForDriver(PagingParam<SortCriteria> paginationModel, Guid DriverId)
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
                .Where(_ => _.DriverId == DriverId && !_.IsDeleted);
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
}