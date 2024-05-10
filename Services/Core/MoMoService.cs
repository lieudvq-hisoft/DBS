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
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using static Google.Apis.Requests.BatchRequest;

namespace Services.Core;

public interface IMoMoService
{
    Task<ResultModel> CreatePaymentAsync(OrderInfoModel model, Guid userId);
    Task<ResultModel> PaymentExecuteAsync(IQueryCollection collection);
}

public class MoMoService : IMoMoService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public MoMoService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager, IOptions<MomoOptionModel> options)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
        _options = options;
    }

    private readonly IOptions<MomoOptionModel> _options;

    public MoMoService(IOptions<MomoOptionModel> options)
    {
        _options = options;
    }

    public async Task<ResultModel> CreatePaymentAsync(OrderInfoModel model, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;

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

        var OrderId = $"{DateTime.UtcNow.Ticks},{userId}";
        var OrderInfo = "Nạp tiền vào SecureWallet";
        var rawData =
            $"partnerCode={_options.Value.PartnerCode}&accessKey={_options.Value.AccessKey}&requestId={OrderId}&amount={model.Amount}&orderId={OrderId}&orderInfo={OrderInfo}&returnUrl={_options.Value.ReturnUrl}&notifyUrl={_options.Value.NotifyUrl}&extraData=";

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
            requestId = OrderId,
            extraData = "",
            signature = signature
        };

        request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

        var response = await client.ExecuteAsync(request);

        var data = _mapper.Map<MomoCreatePaymentResponseModel>(JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content));
        result.Data = data;
        result.Succeed = true;

        return result;
    }

    public async Task<ResultModel> PaymentExecuteAsync(IQueryCollection collection)
    {
        var result = new ResultModel();
        result.Succeed = false;

        var amount = Convert.ToInt64(collection.First(s => s.Key == "amount").Value);
        var orderInfo = collection.First(s => s.Key == "orderInfo").Value;
        var orderId = Convert.ToString(collection.First(s => s.Key == "orderId").Value);
        var userId = orderId.Split(',')[1];

        var user = _dbContext.Users.Where(_ => _.Id == Guid.Parse(userId) && !_.IsDeleted).FirstOrDefault();
        var wallet = _dbContext.Wallets.Where(_ => _.UserId == user.Id).FirstOrDefault();
        if (wallet == null)
        {
            result.ErrorMessage = "Wallet not exist";
            return result;
        }
        wallet.TotalMoney += Convert.ToInt64(amount);
        _dbContext.Wallets.Update(wallet);

        var walletTransaction = new WalletTransaction
        {
            WalletId = wallet.Id,
            TotalMoney = amount,
            TypeWalletTransaction = TypeWalletTransaction.AddFunds,
            PaymentType = PaymentType.MoMo
        };
        _dbContext.WalletTransactions.Add(walletTransaction);
        await _dbContext.SaveChangesAsync();

        var data = new MomoExecuteResponseModel()
        {
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo
        };
        result.Data = data;
        result.Succeed = true;

        return result;
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
