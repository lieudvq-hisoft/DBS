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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;

namespace Services.Core;

public interface IMoMoService
{
    Task<ResultModel> CreatePaymentBookingAsync(OrderInfoBookingModel model, Guid userId);
    Task<ResultModel> CreatePaymentAsync(OrderInfoModel model, Guid userId);
    Task<string> PaymentExecuteAsync(IQueryCollection collection);
}

public class MoMoService : IMoMoService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IHangfireServices _hangfireServices;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;
    private readonly IOptions<MomoOptionModel> _options;

    public MoMoService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IHangfireServices hangfireServices, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager, IOptions<MomoOptionModel> options)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _hangfireServices = hangfireServices;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
        _options = options;
    }

    public async Task<ResultModel> CreatePaymentBookingAsync(OrderInfoBookingModel model, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var OrderId = $"{DateTime.UtcNow.Ticks},{userId}";
            var requestId = "Pay";
            var OrderInfo = $"Thanh toán chuyến đi đến {model.DropOffAddress}";
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={requestId}&amount={model.Amount}&orderId={OrderId}&orderInfo={OrderInfo}&returnUrl={_options.Value.ReturnUrl}&notifyUrl={_options.Value.NotifyUrl}&extraData=";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            // Create an object representing the request data
            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,
                returnUrl = _options.Value.ReturnUrl,
                orderId = OrderId,
                amount = model.Amount.ToString(),
                orderInfo = OrderInfo,
                requestId = requestId,
                extraData = "",
                signature = signature
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            var data = _mapper.Map<MomoCreatePaymentResponseModel>(JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content));
            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> CreatePaymentAsync(OrderInfoModel model, Guid userId)
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
                TotalMoney = Convert.ToInt64(model.Amount),
                TypeWalletTransaction = TypeWalletTransaction.AddFunds,
                PaymentType = PaymentType.MoMo,
            };
            _dbContext.WalletTransactions.Add(walletTransaction);
            await _dbContext.SaveChangesAsync();

            var OrderId = $"{DateTime.UtcNow.Ticks},{userId},{walletTransaction.Id}";
            var requestId = "AddFunds";
            var OrderInfo = "Nạp tiền vào SecureWallet";
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={requestId}&amount={model.Amount}&orderId={OrderId}&orderInfo={OrderInfo}&returnUrl={_options.Value.ReturnUrl}&notifyUrl={_options.Value.NotifyUrl}&extraData=";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            // Create an object representing the request data
            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,
                returnUrl = _options.Value.ReturnUrl,
                orderId = OrderId,
                amount = model.Amount.ToString(),
                orderInfo = OrderInfo,
                requestId = requestId,
                extraData = "",
                signature = signature
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            _hangfireServices.ScheduleCheckFailureWalletTransaction(walletTransaction.Id);

            var data = _mapper.Map<MomoCreatePaymentResponseModel>(JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content));
            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<string> PaymentExecuteAsync(IQueryCollection collection)
    {
        try
        {
            var amount = Convert.ToInt64(collection.First(s => s.Key == "amount").Value);
            var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
            var orderId = Convert.ToString(collection.First(s => s.Key == "orderId").Value);
            var requestId = Convert.ToString(collection.First(s => s.Key == "requestId").Value);
            var message = Convert.ToString(collection.First(s => s.Key == "message").Value);
            var userId = orderId.Split(',')[1];
            var wallet = _dbContext.Wallets.Where(_ => _.UserId == Guid.Parse(userId)).FirstOrDefault() ?? throw new Exception("Wallet not exist");

            if (message.Equals("Success"))
            {
                switch (requestId)
                {
                    case "AddFunds":
                        wallet.TotalMoney += amount;
                        wallet.DateUpdated = DateTime.Now;
                        _dbContext.Wallets.Update(wallet);

                        var walletTransactionId = orderId.Split(',')[2];
                        var walletTransactionAddFunds = _dbContext.WalletTransactions.Where(_ => _.Id == Guid.Parse(walletTransactionId)).FirstOrDefault();
                        walletTransactionAddFunds.Status = WalletTransactionStatus.Success;
                        _dbContext.Wallets.Update(wallet);

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

                        walletAdmin.TotalMoney += amount;
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
                return "srh://app.unilinks.com/viewWallet";
            }
            else
            {
                var payload = _mapper.Map<WalletModel>(wallet);
                var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { Guid.Parse(userId) }, Payload = payload };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
                await _producer.ProduceAsync("dbs-momo-transaction-fail", new Message<Null, string> { Value = json });
                _producer.Flush();

                return "srh://app.unilinks.com/viewWallet";
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        byte[] hashBytes;

        using (var hmac = new HMACSHA256(keyBytes))
        {
            hashBytes = hmac.ComputeHash(messageBytes);
        }

        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

        return hashString;
    }
}
