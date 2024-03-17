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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Core;

public interface ISearchRequestService
{
    Task<ResultModel> Add(SearchRequestCreateModel model, Guid customerId);
    Task<ResultModel> GetOfCustomer(PagingParam<SortCriteria> paginationModel, Guid customerId);
    Task<ResultModel> UpdateStatusToComplete(Guid SearchRequestId, Guid customerId);
    Task<ResultModel> UpdateStatusToCancel(Guid SearchRequestId, Guid customerId);

}
public class SearchRequestService : ISearchRequestService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public SearchRequestService(AppDbContext dbContext, IMapper mapper, IConfiguration configuration,
        UserManager<User> userManager,
        IMailService mailService, IProducer<Null, string> producer)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _configuration = configuration;
        _mailService = mailService;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> Add(SearchRequestCreateModel model, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if(customer == null)
            {
                result.ErrorMessage = "User not exists";
                result.Succeed = false;
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if(!checkCustomer)
            {
                result.ErrorMessage = "The user must be a customer";
                result.Succeed = false;
                return result;
            }
            var searchRequest = _mapper.Map<SearchRequestCreateModel, SearchRequest>(model);
            searchRequest.CustomerId = customer.Id;
            _dbContext.SearchRequests.Add(searchRequest);
            await _dbContext.SaveChangesAsync();
            result.Succeed = true;
            result.Data = searchRequest.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetOfCustomer(PagingParam<SortCriteria> paginationModel, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "User not exists";
                result.Succeed = false;
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a customer";
                result.Succeed = false;
                return result;
            }
            var data = _dbContext.SearchRequests.Where(_ => _.CustomerId == customerId && !_.IsDeleted);
            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var searchRequests = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            searchRequests = searchRequests.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<SearchRequestModel>(searchRequests);
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

    public async Task<ResultModel> UpdateStatusToComplete(Guid SearchRequestId, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "User not exist";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a customer";
                return result;
            }
            var data = _dbContext.SearchRequests.Where(_ => _.CustomerId == customerId && _.Id == SearchRequestId && _.Id == SearchRequestId && !_.IsDeleted).FirstOrDefault();
            if (data == null)
            {
                result.ErrorMessage = "SearchRequest not exist";
                return result;
            }
            if (data.Status != SearchRequestStatus.Processing)
            {
                result.ErrorMessage = "SearchRequest status not suitable";
                return result;
            }
            data.Status = SearchRequestStatus.Completed;
            data.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<SearchRequestModel>(data);
            result.Succeed = true;
            return result;
        } catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateStatusToCancel(Guid SearchRequestId, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "User not exist";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a customer";
                return result;
            }
            var data = _dbContext.SearchRequests.Where(_ => _.CustomerId == customerId && _.Id == SearchRequestId && !_.IsDeleted).FirstOrDefault();
            if (data == null)
            {
                result.ErrorMessage = "SearchRequest not exist";
                return result;
            }
            if (data.Status != SearchRequestStatus.Processing)
            {
                result.ErrorMessage = "SearchRequest status not suitable";
                return result;
            }
            data.Status = SearchRequestStatus.Cancel;
            data.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<SearchRequestModel>(data);
            result.Succeed = true;
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}
