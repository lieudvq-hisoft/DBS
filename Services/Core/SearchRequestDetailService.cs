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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core;

public interface ISearchRequestDetailService
{
    Task<ResultModel> Create(SearchRequestDetailCreateModel model, Guid UserId);
    Task<ResultModel> GetById(Guid SearchRequestDetailId);
    Task<ResultModel> GetBySearchRequestId(Guid SearchRequestId);
}

public class SearchRequestDetailService : ISearchRequestDetailService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public SearchRequestDetailService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> Create(SearchRequestDetailCreateModel model, Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be customer";
                return result;
            }
            var searchRequest = _dbContext.SearchRequests.Where(_ => _.Id == model.SearchRequestId && !_.IsDeleted).FirstOrDefault();
            if (searchRequest == null)
            {
                result.ErrorMessage = "Search Request not exist";
                return result;
            }
            var searchRequestDetail = _mapper.Map<SearchRequestDetailCreateModel, SearchRequestDetail>(model);
            _dbContext.SearchRequestDetails.Add(searchRequestDetail);

            if (model.ImageData != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "SearchRequestDetail", "CustomerImage", searchRequestDetail.Id.ToString());
                searchRequestDetail.ImageData = await MyFunction.UploadFileAsync(model.ImageData, dirPath, "/app/Storage");
            }

            if (model.VehicleImage != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "SearchRequestDetail", "VehicleImage", searchRequestDetail.Id.ToString());
                searchRequestDetail.VehicleImage = await MyFunction.UploadFileAsync(model.VehicleImage, dirPath, "/app/Storage");
            }

            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<SearchRequestDetailModel>(searchRequestDetail);
            data.SearchRequest = _mapper.Map<SearchRequestModel>(searchRequest);

            result.Succeed = true;
            result.Data = data;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetById(Guid SearchRequestDetailId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var searchRequestDetailId = _dbContext.SearchRequestDetails
                .Include(_ => _.SearchRequest)
                .Where(_ => _.Id == SearchRequestDetailId && !_.IsDeleted).FirstOrDefault();
            if (searchRequestDetailId == null)
            {
                result.ErrorMessage = "Search Request Detail not exist";
                return result;
            }
            var data = _mapper.Map<SearchRequestDetailModel>(searchRequestDetailId);
            data.SearchRequest = _mapper.Map<SearchRequestModel>(searchRequestDetailId.SearchRequest);

            if (data.ImageData != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + data.ImageData;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                data.ImageData = Convert.ToBase64String(imageBytes);
            }

            if (data.VehicleImage != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + data.VehicleImage;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                data.VehicleImage = Convert.ToBase64String(imageBytes);
            }

            result.Succeed = true;
            result.Data = data;

        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetBySearchRequestId(Guid SearchRequestId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var searchRequestId = _dbContext.SearchRequests.Where(_ => _.Id == SearchRequestId && !_.IsDeleted).FirstOrDefault();
            if (searchRequestId == null)
            {
                result.ErrorMessage = "Search Request not exist";
                return result;
            }
            var searchRequestDetailId = _dbContext.SearchRequestDetails
                .Include(_ => _.SearchRequest)
                .Where(_ => _.SearchRequestId == SearchRequestId && !_.IsDeleted).FirstOrDefault();
            if (searchRequestDetailId == null)
            {
                result.ErrorMessage = "Search Request Detail not exist";
                return result;
            }
            var data = _mapper.Map<SearchRequestDetailModel>(searchRequestDetailId);
            data.SearchRequest = _mapper.Map<SearchRequestModel>(searchRequestDetailId.SearchRequest);

            if (data.ImageData != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + data.ImageData;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                data.ImageData = Convert.ToBase64String(imageBytes);
            }

            if (data.VehicleImage != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + data.VehicleImage;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                data.VehicleImage = Convert.ToBase64String(imageBytes);
            }

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
