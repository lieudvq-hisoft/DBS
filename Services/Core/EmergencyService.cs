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
    Task<ResultModel> IsHaveEmergency(Guid bookingId, Guid userId);
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
            var sender = _dbContext.Users
                .Include(_ => _.DriverLocations)
                .Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
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
            if (booking.Status != BookingStatus.OnGoing && booking.Status != BookingStatus.CheckOut)
            {
                result.ErrorMessage = "Booking Status is not suitable for Emergency";
                return result;
            }
            var handler = new User();
            handler = _dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(_ => _.DriverStatuses)
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Staff)
                    && u.DriverStatuses.Any(ds => ds.IsFree == true && ds.IsOnline == true)
                    && !u.IsDeleted)
                .GroupJoin(
                    _dbContext.Emergencies,
                    user => user.Id,
                    emergency => emergency.HandlerId,
                    (user, emergencies) => new
                    {
                        User = user,
                        EmergencyCount = emergencies.Count()
                    })
                .OrderBy(x => x.EmergencyCount)
                .Select(x => x.User)
                .FirstOrDefault();
            handler ??= _dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(_ => _.DriverStatuses)
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Staff)
                    && u.DriverStatuses.Any(ds => ds.IsOnline == true)
                    && !u.IsDeleted)
                .GroupJoin(
                    _dbContext.Emergencies,
                    user => user.Id,
                    emergency => emergency.HandlerId,
                    (user, emergencies) => new
                    {
                        User = user,
                        EmergencyCount = emergencies.Count()
                    })
                .OrderBy(x => x.EmergencyCount)
                .Select(x => x.User)
                .FirstOrDefault();
            var emergency = _mapper.Map<EmergencyCreateModel, Emergency>(model);
            emergency.SenderId = customerId;
            emergency.HandlerId = handler.Id;
            var senderLocation = sender.DriverLocations.FirstOrDefault();
            if (senderLocation != null)
            {
                emergency.SenderLatitude = senderLocation.Latitude;
                emergency.SenderLongitude = senderLocation.Longitude;
            }
            _dbContext.Emergencies.Add(emergency);
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<EmergencyModel>(emergency);
            var staffStatus = handler.DriverStatuses.FirstOrDefault();
            data.StaffStatus = _mapper.Map<StaffStatus>(staffStatus);

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
            var sender = _dbContext.Users
                .Include(_ => _.DriverLocations)
                .Where(_ => _.Id == driverId && !_.IsDeleted).FirstOrDefault();
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
            if (booking.Status != BookingStatus.OnGoing && booking.Status != BookingStatus.CheckOut)
            {
                result.ErrorMessage = "Booking Status is not suitable for Emergency";
                return result;
            }
            var handler = new User();
            handler = _dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(_ => _.DriverStatuses)
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Staff)
                    && u.DriverStatuses.Any(ds => ds.IsFree == true && ds.IsOnline == true)
                    && !u.IsDeleted)
                .GroupJoin(
                    _dbContext.Emergencies,
                    user => user.Id,
                    emergency => emergency.HandlerId,
                    (user, emergencies) => new
                    {
                        User = user,
                        EmergencyCount = emergencies.Count()
                    })
                .OrderBy(x => x.EmergencyCount)
                .Select(x => x.User)
                .FirstOrDefault();
            handler ??= _dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(_ => _.DriverStatuses)
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Staff)
                    && u.DriverStatuses.Any(ds => ds.IsOnline == true)
                    && !u.IsDeleted)
                .GroupJoin(
                    _dbContext.Emergencies,
                    user => user.Id,
                    emergency => emergency.HandlerId,
                    (user, emergencies) => new
                    {
                        User = user,
                        EmergencyCount = emergencies.Count()
                    })
                .OrderBy(x => x.EmergencyCount)
                .Select(x => x.User)
                .FirstOrDefault();
            var emergency = _mapper.Map<EmergencyCreateModel, Emergency>(model);
            emergency.SenderId = driverId;
            emergency.HandlerId = handler.Id;
            var senderLocation = sender.DriverLocations.FirstOrDefault();
            if (senderLocation != null)
            {
                emergency.SenderLatitude = senderLocation.Latitude;
                emergency.SenderLongitude = senderLocation.Longitude;
            }
            _dbContext.Emergencies.Add(emergency);
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<EmergencyModel>(emergency);
            var staffStatus = handler.DriverStatuses.FirstOrDefault();
            data.StaffStatus = _mapper.Map<StaffStatus>(staffStatus);

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

    public async Task<ResultModel> IsHaveEmergency(Guid bookingId, Guid userId)
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
                .Where(_ => _.BookingId == bookingId).FirstOrDefault();
            if (emergency == null)
            {
                result.Data = false;
                result.Succeed = true;
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
            var user = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            if (checkStaff && emergency.HandlerId != userId)
            {
                result.ErrorMessage = "You don't have permission to process this emergency";
                return result;
            }
            else
            {
                var staffStatus = user.DriverStatuses.FirstOrDefault();
                staffStatus.IsFree = false;
                staffStatus.DateUpdated = DateTime.Now;
                _dbContext.DriverStatuses.Update(staffStatus);
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
            if (model.IsStopTrip && model.BookingCancelReason == null)
            {
                result.ErrorMessage = "Stop Trip need Cancel Reason";
                return result;
            }
            var user = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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
            if (checkStaff && emergency.HandlerId != userId)
            {
                result.ErrorMessage = "You don't have permission to solve this emergency";
                return result;
            }
            else
            {
                var staffStatus = user.DriverStatuses.FirstOrDefault();
                staffStatus.IsFree = true;
                staffStatus.DateUpdated = DateTime.Now;
                _dbContext.DriverStatuses.Update(staffStatus);
            }
            if (model.IsStopTrip)
            {
                var booking = _dbContext.Bookings
                    .Include(_ => _.Driver)
                    .Include(_ => _.SearchRequest)
                        .ThenInclude(sr => sr.Customer)
                    .Where(_ => _.Id == emergency.BookingId && !_.IsDeleted).FirstOrDefault();
                booking.Status = BookingStatus.Cancel;
                booking.DateUpdated = DateTime.Now;

                var bookingCancel = new BookingCancel
                {
                    BookingId = booking.Id,
                    CancelPersonId = user.Id,
                    CancelReason = model.BookingCancelReason
                };
                _dbContext.BookingCancels.Add(bookingCancel);

                var bookingPayload = _mapper.Map<BookingModel>(booking);
                var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { booking.DriverId, booking.SearchRequest.CustomerId }, Payload = bookingPayload };
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
                await _producer.ProduceAsync("dbs-emergency-stop-trip", new Message<Null, string> { Value = json });
            }
            emergency.Status = EmergencyStatus.Solved;
            emergency.DateUpdated = DateTime.Now;
            emergency.Solution = model.Solution;
            await _dbContext.SaveChangesAsync();

            var data = _mapper.Map<EmergencyModel>(emergency);
            var kafkaModelSolve = new KafkaModel { UserReceiveNotice = new List<Guid>() { emergency.SenderId }, Payload = data };
            var jsonSolve = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModelSolve);
            await _producer.ProduceAsync("dbs-emergency-staff-solve", new Message<Null, string> { Value = jsonSolve });
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
