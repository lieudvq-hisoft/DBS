using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Enums;
using Data.Model;
using Data.Models;
using Data.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static Google.Apis.Requests.BatchRequest;

namespace Services.Core;

public interface IVNPayService
{
    Task<ResultModel> CreatePaymentBookingUrl(PaymentInformationModel model, HttpContext context, Guid userId);
    Task<ResultModel> CreatePaymentUrl(PaymentInformationModel model, HttpContext context, Guid userId);
    Task<string> PaymentExecute(IQueryCollection collections);
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
    public async Task<ResultModel> CreatePaymentBookingUrl(PaymentInformationModel model, HttpContext context, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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
            pay.AddRequestData("vnp_OrderInfo", $"Pay,{DateTime.UtcNow.Ticks},{userId}");
            pay.AddRequestData("vnp_OrderType", $"Thanh toán chuyến đi");
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
            pay.AddRequestData("vnp_OrderInfo", $"AddFunds,{DateTime.UtcNow.Ticks},{userId},{walletTransaction.Id}");
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

    public async Task<string> PaymentExecute(IQueryCollection collections)
    {

        try
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            var transactionType = response.OrderDescription.Split(',')[0];
            var userId = response.OrderDescription.Split(',')[2];
            var data = _mapper.Map<PaymentResponseModel>(response);
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == Guid.Parse(userId)).FirstOrDefault();

            if (response.VnPayResponseCode == "00")
            {
                switch (transactionType)
                {
                    case "AddFunds":
                        wallet.TotalMoney += (response.Amount / 100);
                        wallet.DateUpdated = DateTime.Now;
                        _dbContext.Wallets.Update(wallet);

                        var walletTransactionId = response.OrderDescription.Split(',')[3];
                        var walletTransaction = _dbContext.WalletTransactions.Where(_ => _.Id == Guid.Parse(walletTransactionId)).FirstOrDefault();
                        walletTransaction.Status = WalletTransactionStatus.Success;
                        _dbContext.WalletTransactions.Update(walletTransaction);

                        await _dbContext.SaveChangesAsync();

                        var payloadAddFunds = _mapper.Map<WalletModel>(wallet);
                        var kafkaModelAddFunds = new KafkaModel { UserReceiveNotice = new List<Guid>() { Guid.Parse(userId) }, Payload = payloadAddFunds };
                        var jsonAddFunds = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelAddFunds);
                        await _producer.ProduceAsync("dbs-wallet-addfunds-success", new Message<Null, string> { Value = jsonAddFunds });
                        _producer.Flush();

                        return "srh://app.unilinks.com/viewWallet";
                        break;
                    case "Pay":
                        var admin = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                        .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Admin) && !_.IsDeleted).FirstOrDefault();
                        var walletAdmin = _dbContext.Wallets.Where(_ => _.UserId == admin.Id).FirstOrDefault();

                        walletAdmin.TotalMoney += (response.Amount / 100);
                        walletAdmin.DateUpdated = DateTime.Now;
                        _dbContext.Wallets.Update(walletAdmin);

                        await _dbContext.SaveChangesAsync();

                        var payloadPay = _mapper.Map<WalletModel>(wallet);
                        var kafkaModelPay = new KafkaModel { UserReceiveNotice = new List<Guid>() { Guid.Parse(userId) }, Payload = payloadPay };
                        var jsonPay = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelPay);
                        await _producer.ProduceAsync("dbs-payment-booking-success", new Message<Null, string> { Value = jsonPay });
                        _producer.Flush();

                        return "srh://app.unilinks.com/mapCustomer";
                        break;
                }
            }
            else
            {
                var payload = _mapper.Map<WalletModel>(wallet);
                var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { Guid.Parse(userId) }, Payload = payload };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
                await _producer.ProduceAsync("dbs-vnpay-transaction-fail", new Message<Null, string> { Value = json });
                _producer.Flush();

                return "Something when wrong with VNPay";
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

        return "srh://app.unilinks.com/viewWallet";
    }
}
