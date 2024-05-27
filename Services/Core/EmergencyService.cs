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
public interface IEmergencyService
{
    Task<ResultModel> CustomerCreateEmergency(EmergencyCreateModel model, Guid customerId);
    Task<ResultModel> DriverCreateEmergency(EmergencyCreateModel model, Guid driverId);
    Task<ResultModel> GetEmergencies(PagingParam<SortEmergencyCriteria> paginationModel, Guid userId);
    Task<ResultModel> GetEmergencyById(Guid emergencyId, Guid userId);
    Task<ResultModel> UpdateEmergencyStatusProcessing(Guid emergencyId, Guid userId);
    Task<ResultModel> UpdateEmergencyStatusSolved(EmergencyUpdateSolveModel model, Guid userId);
}

public class EmergencyService : IEmergencyService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public EmergencyService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> CustomerCreateEmergency(EmergencyCreateModel model, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var sender = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (sender == null)
            {
                result.ErrorMessage = "Sender is not exist";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(sender, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "Sender must be Customer";
                return result;
            }
            if (!sender.IsActive)
            {
                result.ErrorMessage = "The sender is deactivated";
                return result;
            }
            var handler = _dbContext.Users.Where(_ => _.Id == model.HandlerId && !_.IsDeleted).FirstOrDefault();
            if (handler == null)
            {
                result.ErrorMessage = "Handler is not exist";
                return result;
            }
            var checkStaff = await _userManager.IsInRoleAsync(handler, RoleNormalizedName.Staff);
            if (!checkStaff)
            {
                result.ErrorMessage = "Sender must be Staff";
                return result;
            }
            if (!handler.IsActive)
            {
                result.ErrorMessage = "The handler is deactivated";
                return result;
            }
            var booking = _dbContext.Bookings
                .Include(_ => _.SearchRequest)
                    .ThenInclude(sr => sr.Customer)
                .Include(_ => _.Driver)
                .Where(_ => _.Id == model.BookingId).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            if (booking.SearchRequest.Customer.Id != sender.Id)
            {
                result.ErrorMessage = "You don't have permission to send Emergency";
            }
            var emergency = _mapper.Map<EmergencyCreateModel, Emergency>(model);
            emergency.SenderId = customerId;
            _dbContext.Emergencies.Add(emergency);
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<EmergencyModel>(emergency);
            var kafkaModelPay = new KafkaModel { UserReceiveNotice = new List<Guid>() { handler.Id }, Payload = data };
            var jsonPay = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelPay);
            await _producer.ProduceAsync("dbs-emergency-customer-send", new Message<Null, string> { Value = jsonPay });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> DriverCreateEmergency(EmergencyCreateModel model, Guid driverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var sender = _dbContext.Users.Where(_ => _.Id == driverId && !_.IsDeleted).FirstOrDefault();
            if (sender == null)
            {
                result.ErrorMessage = "Sender is not exist";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(sender, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "Sender must be Driver";
                return result;
            }
            if (!sender.IsActive)
            {
                result.ErrorMessage = "The sender is deactivated";
                return result;
            }
            var handler = _dbContext.Users.Where(_ => _.Id == model.HandlerId && !_.IsDeleted).FirstOrDefault();
            if (handler == null)
            {
                result.ErrorMessage = "Handler is not exist";
                return result;
            }
            var checkStaff = await _userManager.IsInRoleAsync(handler, RoleNormalizedName.Staff);
            if (!checkStaff)
            {
                result.ErrorMessage = "Sender must be Staff";
                return result;
            }
            if (!handler.IsActive)
            {
                result.ErrorMessage = "The handler is deactivated";
                return result;
            }
            var booking = _dbContext.Bookings
                .Include(_ => _.SearchRequest)
                    .ThenInclude(sr => sr.Customer)
                .Include(_ => _.Driver)
                .Where(_ => _.Id == model.BookingId).FirstOrDefault();
            if (booking == null)
            {
                result.ErrorMessage = "Booking not exist";
                return result;
            }
            if (booking.SearchRequest.Customer.Id != sender.Id)
            {
                result.ErrorMessage = "You don't have permission to send Emergency";
            }
            var emergency = _mapper.Map<EmergencyCreateModel, Emergency>(model);
            emergency.SenderId = driverId;
            _dbContext.Emergencies.Add(emergency);
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<EmergencyModel>(emergency);
            var kafkaModelPay = new KafkaModel { UserReceiveNotice = new List<Guid>() { handler.Id }, Payload = data };
            var jsonPay = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelPay);
            await _producer.ProduceAsync("dbs-emergency-driver-send", new Message<Null, string> { Value = jsonPay });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetEmergencies(PagingParam<SortEmergencyCriteria> paginationModel, Guid userId)
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
                result.ErrorMessage = "User is deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "You don't have permission";
                return result;
            }
            var data = _dbContext.Emergencies.OrderByDescending(_ => _.DateCreated);
            if (data == null)
            {
                result.ErrorMessage = "Emergency not exist";
                return result;
            }

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var emergencies = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            emergencies = emergencies.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<EmergencyModel>(emergencies);

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

    public async Task<ResultModel> GetEmergencyById(Guid emergencyId, Guid userId)
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
                result.ErrorMessage = "User is deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "You don't have permission";
                return result;
            }
            var emergency = _dbContext.Emergencies
                .Include(_ => _.Sender)
                .Include(_ => _.Handler)
                .Include(_ => _.Booking)
                .Where(_ => _.Id == emergencyId).FirstOrDefault();
            if (emergency == null)
            {
                result.ErrorMessage = "Emergency not exist";
                return result;
            }

            result.Data = _mapper.Map<EmergencyModel>(emergency);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> UpdateEmergencyStatusProcessing(Guid emergencyId, Guid userId)
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
                result.ErrorMessage = "User is deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "You don't have permission";
                return result;
            }
            var emergency = _dbContext.Emergencies
                .Include(_ => _.Sender)
                .Include(_ => _.Handler)
                .Include(_ => _.Booking)
                .Where(_ => _.Id == emergencyId).FirstOrDefault();
            if (emergency == null)
            {
                result.ErrorMessage = "Emergency not exist";
                return result;
            }
            emergency.Status = EmergencyStatus.Processing;
            emergency.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<EmergencyModel>(emergency);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> UpdateEmergencyStatusSolved(EmergencyUpdateSolveModel model, Guid userId)
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
                result.ErrorMessage = "User is deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "You don't have permission";
                return result;
            }
            var emergency = _dbContext.Emergencies
                .Include(_ => _.Sender)
                .Include(_ => _.Handler)
                .Include(_ => _.Booking)
                .Where(_ => _.Id == model.EmergencyId).FirstOrDefault();
            if (emergency == null)
            {
                result.ErrorMessage = "Emergency not exist";
                return result;
            }
            emergency.Status = EmergencyStatus.Solved;
            emergency.DateUpdated = DateTime.Now;
            emergency.Solution = model.Solution;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<EmergencyModel>(emergency);
            var kafkaModelPay = new KafkaModel { UserReceiveNotice = new List<Guid>() { emergency.SenderId }, Payload = data };
            var jsonPay = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelPay);
            await _producer.ProduceAsync("dbs-emergency-staff-solve", new Message<Null, string> { Value = jsonPay });
            _producer.Flush();

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }
}
