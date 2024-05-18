using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core;

public interface ILinkedAccountService
{
    Task<ResultModel> AddLinkedAccount(LinkedAccountCreateModel model, Guid UserId);
    Task<ResultModel> GetAllLinkedAccount(Guid UserId);
    Task<ResultModel> GetLinkedAccountById(Guid LinkedAccountId, Guid UserId);
    Task<ResultModel> DeleteLinkedAccount(Guid LinkedAccountId, Guid UserId);
}

public class LinkedAccountService : ILinkedAccountService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public LinkedAccountService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> AddLinkedAccount(LinkedAccountCreateModel model, Guid UserId)
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
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var linkedAccount = _mapper.Map<LinkedAccountCreateModel, LinkedAccount>(model);
            _dbContext.LinkedAccounts.Add(linkedAccount);
            linkedAccount.UserId = UserId;
            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<LinkedAccountModel>(linkedAccount);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> DeleteLinkedAccount(Guid LinkedAccountId, Guid UserId)
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
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var linkedAccount = _dbContext.LinkedAccounts.Where(_ => _.Id == LinkedAccountId && !_.IsDeleted).FirstOrDefault();
            if (linkedAccount == null)
            {
                result.ErrorMessage = "Linked Account not exist";
                return result;
            }
            if (user.Id != linkedAccount.UserId)
            {
                result.ErrorMessage = "You don't have permission to get Linked Account";
                return result;
            }
            _dbContext.LinkedAccounts.Remove(linkedAccount);
            await _dbContext.SaveChangesAsync();

            result.Data = "Delete Linked Account successful!";
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetAllLinkedAccount(Guid UserId)
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
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var linkedAccounts = _dbContext.LinkedAccounts.Where(_ => _.UserId == UserId && !_.IsDeleted).ToList();
            if (linkedAccounts == null)
            {
                result.ErrorMessage = "Linked Account not exist";
                return result;
            }

            result.Data = _mapper.Map<List<LinkedAccountModel>>(linkedAccounts);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetLinkedAccountById(Guid LinkedAccountId, Guid UserId)
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
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var linkedAccount = _dbContext.LinkedAccounts.Where(_ => _.Id == LinkedAccountId && !_.IsDeleted).FirstOrDefault();
            if (linkedAccount == null)
            {
                result.ErrorMessage = "Linked Account not exist";
                return result;
            }
            if (user.Id != linkedAccount.UserId)
            {
                result.ErrorMessage = "You don't have permission to get Linked Account";
                return result;
            }

            result.Data = _mapper.Map<LinkedAccountModel>(linkedAccount);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }
}
