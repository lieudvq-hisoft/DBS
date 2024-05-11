using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Enums;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using static Google.Apis.Requests.BatchRequest;

namespace Services.Core;

public interface IVNPayService
{
    Task<ResultModel> CreatePaymentUrl(PaymentInformationModel model, HttpContext context, Guid userId);
    Task<ResultModel> PaymentExecute(IQueryCollection collections);
}

public class VNPayService : IVNPayService
{

    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public VNPayService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> CreatePaymentUrl(PaymentInformationModel model, HttpContext context, Guid userId)
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
            if (model.Amount < 100000 || model.Amount > 10000000)
            {
                result.ErrorMessage = "Ammont between 100000 and 10000000";
                return result;
            }

            var wallet = _dbContext.Wallets.Where(_ => _.UserId == user.Id).FirstOrDefault();
            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exist";
                return result;
            }

            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                TotalMoney = (int)model.Amount * 100,
                TypeWalletTransaction = TypeWalletTransaction.AddFunds,
                PaymentType = PaymentType.VNPay,
            };
            _dbContext.WalletTransactions.Add(walletTransaction);
            await _dbContext.SaveChangesAsync();

            var timeNow = DateTime.Now;
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _configuration["PaymentCallBack:ReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{DateTime.UtcNow.Ticks},{userId},{walletTransaction.Id}");
            pay.AddRequestData("vnp_OrderType", "Nạp tiền vào SecureWallet");
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);


            result.Data = paymentUrl;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }


        return result;
    }

    public async Task<ResultModel> PaymentExecute(IQueryCollection collections)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            var userId = response.OrderDescription.Split(',')[1];
            var walletTransactionId = response.OrderDescription.Split(',')[2];
            var data = _mapper.Map<PaymentResponseModel>(response);
            if (response.VnPayResponseCode == "00")
            {
                var wallet = _dbContext.Wallets.Where(_ => _.UserId == Guid.Parse(userId)).FirstOrDefault();
                if (wallet == null)
                {
                    result.ErrorMessage = "Wallet not exist";
                    return result;
                }
                wallet.TotalMoney += (response.Amount / 100);
                _dbContext.Wallets.Update(wallet);

                var walletTransaction = _dbContext.WalletTransactions.Where(_ => _.Id == Guid.Parse(walletTransactionId)).FirstOrDefault();
                walletTransaction.Status = WalletTransactionStatus.Success;
                _dbContext.WalletTransactions.Update(walletTransaction);

                await _dbContext.SaveChangesAsync();

                result.Data = response;
                result.Succeed = true;
            }
            else
            {
                var walletTransaction = _dbContext.WalletTransactions.Where(_ => _.Id == Guid.Parse(walletTransactionId)).FirstOrDefault();
                walletTransaction.Status = WalletTransactionStatus.Failure;
                _dbContext.WalletTransactions.Update(walletTransaction);

                await _dbContext.SaveChangesAsync();

                result.ErrorMessage = "Something when wrong with VNPay";
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }
}
