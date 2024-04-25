using Data.Model;
using Data.Models;
using Data.Enums;
using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Data.Utils;
using Data.Common.PaginationModel;
using Microsoft.EntityFrameworkCore;
using Data.Utils.Paging;

namespace Services.Core;

public interface ISupportService
{
    Task<ResultModel> Create(SupportCreateModel model);
    Task<ResultModel> CreateBookingIssue(SupportBookingIssueCreateModel model);
    Task<ResultModel> GetByID(Guid SupportId, Guid UserId);
    Task<ResultModel> GetAll(PagingParam<SortSupportCriteria> paginationModel, Guid UserId);
    Task<ResultModel> ChangeStatusToInProcess(Guid SupportId, Guid UserId);
    Task<ResultModel> ChangeStatusToSolved(Guid SupportId, Guid UserId);
    Task<ResultModel> ChangeStatusToCantSolved(Guid SupportId, Guid UserId);
    Task<ResultModel> Delete(Guid SupportId, Guid UserId);
}

public class SupportService : ISupportService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public SupportService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> Create(SupportCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            switch (model.SupportType)
            {
                case SupportType.Recruitment:
                    if (model.FullName == null ||
                        model.Email == null ||
                        model.PhoneNumber == null ||
                        model.IdentityCardNumber == null ||
                        model.BirthPlace == null ||
                        model.Address == null ||
                        model.DrivingLicenseNumber == null ||
                        model.DrivingLicenseType == null
                        )
                    {
                        result.ErrorMessage = "All field is required";
                        return result;
                    }
                    var supportRecruitment = _mapper.Map<SupportCreateModel, Support>(model);
                    _dbContext.Supports.Add(supportRecruitment);
                    await _dbContext.SaveChangesAsync();

                    result.Succeed = true;
                    result.Data = _mapper.Map<SupportModel>(supportRecruitment);
                    break;
                case SupportType.SupportIssue:
                    if (model.FullName == null ||
                        model.Email == null ||
                        model.PhoneNumber == null ||
                        model.MsgContent == null
                        )
                    {
                        result.ErrorMessage = "All field is required";
                        return result;
                    }
                    var supportSupportIssue = _mapper.Map<SupportCreateModel, Support>(model);
                    _dbContext.Supports.Add(supportSupportIssue);
                    await _dbContext.SaveChangesAsync();

                    result.Succeed = true;
                    result.Data = _mapper.Map<SupportModel>(supportSupportIssue);
                    break;
                default:
                    result.ErrorMessage = "Support Type not suitable";
                    return result;

            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
    public async Task<ResultModel> CreateBookingIssue(SupportBookingIssueCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var supportBookingIssue = _mapper.Map<SupportBookingIssueCreateModel, Support>(model);
            supportBookingIssue.SupportType = SupportType.BookingIssue;
            _dbContext.Supports.Add(supportBookingIssue);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<SupportModel>(supportBookingIssue);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
    public async Task<ResultModel> GetAll(PagingParam<SortSupportCriteria> paginationModel, Guid UserId)
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var data = _dbContext.Supports
                .Where(_ => !_.IsDeleted);
            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var supports = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            supports = supports.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<SupportModel>(supports);

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
    public async Task<ResultModel> GetByID(Guid SupportId, Guid UserId)
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var support = _dbContext.Supports.Where(_ => _.Id == SupportId && !_.IsDeleted).FirstOrDefault();
            if (support == null)
            {
                result.ErrorMessage = "Support not exist";
                return result;
            }

            result.Succeed = true;
            result.Data = _mapper.Map<SupportModel>(support);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
    public async Task<ResultModel> ChangeStatusToCantSolved(Guid SupportId, Guid UserId)
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var support = _dbContext.Supports.Where(_ => _.Id == SupportId && !_.IsDeleted).FirstOrDefault();
            if (support == null)
            {
                result.ErrorMessage = "Support not exist";
                return result;
            }
            support.SupportStatus = SupportStatus.CantSolved;
            support.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<SupportModel>(support);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangeStatusToInProcess(Guid SupportId, Guid UserId)
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var support = _dbContext.Supports.Where(_ => _.Id == SupportId && !_.IsDeleted).FirstOrDefault();
            if (support == null)
            {
                result.ErrorMessage = "Support not exist";
                return result;
            }
            support.SupportStatus = SupportStatus.InProcess;
            support.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<SupportModel>(support);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangeStatusToSolved(Guid SupportId, Guid UserId)
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var support = _dbContext.Supports.Where(_ => _.Id == SupportId && !_.IsDeleted).FirstOrDefault();
            if (support == null)
            {
                result.ErrorMessage = "Support not exist";
                return result;
            }
            support.SupportStatus = SupportStatus.Solved;
            support.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<SupportModel>(support);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> Delete(Guid SupportId, Guid UserId)
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var support = _dbContext.Supports.Where(_ => _.Id == SupportId && !_.IsDeleted).FirstOrDefault();
            if (support == null)
            {
                result.ErrorMessage = "Support not exist";
                return result;
            }
            _dbContext.Supports.Remove(support);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<SupportModel>(support);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}
