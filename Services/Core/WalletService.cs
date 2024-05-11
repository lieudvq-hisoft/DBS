using AutoMapper;
using Confluent.Kafka;
using Data.Common.PaginationModel;
using Data.DataAccess;
using Data.Entities;
using Data.Enums;
using Data.Model;
using Data.Models;
using Data.Utils.Paging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Core;

public interface IWalletService
{
    Task<ResultModel> CreateWallet(WalletCreateModel model);
    Task<ResultModel> GetWallet(Guid userId);
    Task<ResultModel> CheckExistWallet(Guid userId);
    Task<ResultModel> AddFunds(WalletTransactionCreateModel model, Guid userId);
    Task<ResultModel> WithdrawFunds(WalletTransactionCreateModel model, Guid userId);
    Task<ResultModel> Pay(WalletTransactionCreateModel model, Guid userId);
    Task<ResultModel> GetTransactions(PagingParam<SortWalletCriteria> paginationModel, Guid userId);

}

public class WalletService : IWalletService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public WalletService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> CreateWallet(WalletCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == model.UserId && !_.IsDeleted).FirstOrDefault();
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
            var wallet = _mapper.Map<WalletCreateModel, Wallet>(model);
            _dbContext.Wallets.Add(wallet);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<WalletModel>(wallet);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetWallet(Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == userId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }

            result.Succeed = true;
            result.Data = _mapper.Map<WalletModel>(wallet);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> CheckExistWallet(Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == userId).FirstOrDefault();
            if (wallet == null)
            {
                result.Data = false;
                result.Succeed = true;
                return result;
            }

            result.Succeed = true;
            result.Data = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> AddFunds(WalletTransactionCreateModel model, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            if (model.TotalMoney < 100000)
            {
                result.ErrorMessage = "The minimum is 100.000 (VNĐ)";
                return result;
            }
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == userId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }
            var walletTransaction = _mapper.Map<WalletTransactionCreateModel, WalletTransaction>(model);
            walletTransaction.TypeWalletTransaction = Data.Enums.TypeWalletTransaction.AddFunds;
            walletTransaction.WalletId = wallet.Id;
            _dbContext.WalletTransactions.Add(walletTransaction);

            wallet.TotalMoney += walletTransaction.TotalMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<WalletModel>(wallet);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> WithdrawFunds(WalletTransactionCreateModel model, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == userId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }
            var walletTransaction = _mapper.Map<WalletTransactionCreateModel, WalletTransaction>(model);
            walletTransaction.TypeWalletTransaction = Data.Enums.TypeWalletTransaction.WithdrawFunds;
            walletTransaction.WalletId = wallet.Id;
            _dbContext.WalletTransactions.Add(walletTransaction);

            wallet.TotalMoney -= walletTransaction.TotalMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<WalletModel>(wallet);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> Pay(WalletTransactionCreateModel model, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == userId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }
            var walletTransaction = _mapper.Map<WalletTransactionCreateModel, WalletTransaction>(model);
            walletTransaction.TypeWalletTransaction = TypeWalletTransaction.Pay;
            walletTransaction.WalletId = wallet.Id;
            walletTransaction.Status = WalletTransactionStatus.Success;
            _dbContext.WalletTransactions.Add(walletTransaction);

            wallet.TotalMoney -= walletTransaction.TotalMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<WalletModel>(wallet);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetTransactions(PagingParam<SortWalletCriteria> paginationModel, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == userId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }
            var data = _dbContext.WalletTransactions.Where(_ => _.WalletId == wallet.Id);

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var walletTransactions = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            walletTransactions = walletTransactions.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.Map<List<WalletTransactionModel>>(walletTransactions);

            if (walletTransactions == null)
            {
                result.ErrorMessage = "Wallet Transactions not exist";
                return result;
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
}

