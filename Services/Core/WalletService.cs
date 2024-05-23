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

namespace Services.Core;

public interface IWalletService
{
    Task<ResultModel> CreateWallet(WalletCreateModel model);
    Task<ResultModel> GetWallet(Guid userId);
    Task<ResultModel> CheckExistWallet(Guid userId);
    Task<ResultModel> AddFunds(WalletTransactionCreateModel model, Guid userId);
    Task<ResultModel> WithdrawFunds(WalletTransactionCreateModel model, Guid LinkedAccountId, Guid userId);
    Task<ResultModel> AcceptWithdrawFundsRequest(ResponeWithdrawFundsRequest model, Guid adminId);
    Task<ResultModel> RejectWithdrawFundsRequest(ResponeWithdrawFundsRequest model, Guid adminId);
    Task<ResultModel> Pay(WalletTransactionCreateModel model, Guid userId);
    Task<ResultModel> GetTransactions(PagingParam<SortWalletCriteria> paginationModel, Guid userId);
    Task<ResultModel> CheckFailureWalletTransaction(Guid walletTransactionId);
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

    public async Task<ResultModel> WithdrawFunds(WalletTransactionCreateModel model, Guid LinkedAccountId, Guid userId)
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

            var linkedAccount = _dbContext.LinkedAccounts.Where(_ => _.Id == LinkedAccountId).FirstOrDefault();
            if (linkedAccount == null)
            {
                result.ErrorMessage = "Linked Account not exist";
                return result;
            }
            if (linkedAccount.UserId != userId)
            {
                result.ErrorMessage = "You don't have permission with this Linked Account";
                return result;
            }

            var admin = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Admin) && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }

            var walletTransaction = _mapper.Map<WalletTransactionCreateModel, WalletTransaction>(model);
            walletTransaction.TypeWalletTransaction = TypeWalletTransaction.WithdrawFunds;
            walletTransaction.WalletId = wallet.Id;
            walletTransaction.LinkedAccount = _mapper.Map<LinkedAccountModel>(linkedAccount);
            _dbContext.WalletTransactions.Add(walletTransaction);

            wallet.TotalMoney -= walletTransaction.TotalMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            await _dbContext.SaveChangesAsync();

            var payload = _mapper.Map<WalletTransactionModel>(walletTransaction);
            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { admin.Id }, Payload = payload };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-wallet-withrawsfunds-request", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Succeed = true;
            result.Data = _mapper.Map<WalletModel>(wallet);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> AcceptWithdrawFundsRequest(ResponeWithdrawFundsRequest model, Guid adminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == adminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }
            if (!admin.IsActive)
            {
                result.ErrorMessage = "Admin has been deactivated";
                return result;
            }
            var walletTransaction = _dbContext.WalletTransactions.Where(_ => _.Id == model.WithdrawFundsId).FirstOrDefault();
            if (walletTransaction == null)
            {
                result.ErrorMessage = "Wallet Transaction not exist";
                return result;
            }
            walletTransaction.Status = WalletTransactionStatus.Success;
            walletTransaction.DateUpdated = DateTime.Now;

            var wallet = _dbContext.Wallets.Where(_ => _.Id == walletTransaction.WalletId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }

            var payload = _mapper.Map<WalletTransactionModel>(walletTransaction);
            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { wallet.UserId }, Payload = payload };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-wallet-withrawsfunds-success", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Succeed = true;
            result.Data = _mapper.Map<WalletModel>(wallet);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> RejectWithdrawFundsRequest(ResponeWithdrawFundsRequest model, Guid adminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == adminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }
            if (!admin.IsActive)
            {
                result.ErrorMessage = "Admin has been deactivated";
                return result;
            }
            var walletTransaction = _dbContext.WalletTransactions.Where(_ => _.Id == model.WithdrawFundsId).FirstOrDefault();
            if (walletTransaction == null)
            {
                result.ErrorMessage = "Wallet Transaction not exist";
                return result;
            }

            var wallet = _dbContext.Wallets.Where(_ => _.Id == walletTransaction.WalletId).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }

            walletTransaction.Status = WalletTransactionStatus.Failure;
            walletTransaction.DateUpdated = DateTime.Now;

            wallet.TotalMoney += walletTransaction.TotalMoney;
            wallet.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(wallet);

            await _dbContext.SaveChangesAsync();

            var payload = _mapper.Map<WalletTransactionModel>(walletTransaction);
            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { wallet.UserId }, Payload = payload };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-wallet-withrawsfunds-failure", new Message<Null, string> { Value = json });
            _producer.Flush();

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

            var admin = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Admin) && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }

            var walletAdmin = _dbContext.Wallets.Where(_ => _.UserId == admin.Id).FirstOrDefault();
            if (walletAdmin == null)
            {
                result.ErrorMessage = "Wallet Admin not exist";
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

            walletAdmin.TotalMoney += walletTransaction.TotalMoney;
            walletAdmin.DateUpdated = DateTime.Now;
            _dbContext.Wallets.Update(walletAdmin);

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

    public async Task<ResultModel> CheckFailureWalletTransaction(Guid walletTransactionId)
    {
        var result = new ResultModel();

        try
        {
            var walletTransaction = await _dbContext.WalletTransactions
                .FirstOrDefaultAsync(wt => wt.Id == walletTransactionId);

            if (walletTransaction != null && walletTransaction.Status == Data.Enums.WalletTransactionStatus.Waiting)
            {
                walletTransaction.Status = Data.Enums.WalletTransactionStatus.Failure;
                await _dbContext.SaveChangesAsync();
            }
            result.Succeed = true;
            result.Data = walletTransaction;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}

