﻿using AutoMapper;
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
using Services.Utils;

namespace Services.Core;

public interface IDriverService
{
    Task<ResultModel> RegisterDriver(RegisterModel model);
    Task<ResultModel> RegisterDriverByAdmin(RegisterDriverByAdminModel model, Guid UserId);
    Task<ResultModel> GetDriver(PagingParam<DriverSortCriteria> paginationModel, SearchModel searchModel);
    Task<ResultModel> UpdateLocation(LocationModel model, Guid driverId);
    Task<ResultModel> TrackingDriverLocation(TrackingDriverLocationModel model, Guid driverId);
    Task<ResultModel> UpdateStatusOffline(Guid driverId);
    Task<ResultModel> UpdateAllDriverStatusOffline();
    Task<ResultModel> UpdateStatusOnline(Guid driverId);
    Task<ResultModel> GetDriverOnline(LocationCustomer locationCustomer, Guid userId);
    Task<ResultModel> GetDriverStatistics(Guid driverId, int year);
    Task<ResultModel> GetDriverMonthlyStatistics(Guid driverId, int month, int year);


}
public class DriverService : IDriverService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public DriverService(AppDbContext dbContext, IMapper mapper, IConfiguration configuration,
        UserManager<User> userManager,
        IMailService mailService, IProducer<Null, string> producer)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _configuration = configuration;
        _mailService = mailService;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> RegisterDriver(RegisterModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var checkEmailExisted = await _userManager.FindByEmailAsync(model.Email);
            if (checkEmailExisted != null)
            {
                result.ErrorMessage = "Email already existed";
                result.Succeed = false;
                return result;
            }
            var checkUserNameExisted = await _userManager.FindByNameAsync(model.UserName);
            if (checkUserNameExisted != null)
            {
                result.ErrorMessage = "UserName already existed";
                result.Succeed = false;
                return result;
            }

            var userRole = new UserRole { };

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.NormalizedName == RoleNormalizedName.Driver);
            if (role == null)
            {
                var newRole = new Role { Name = "Driver", NormalizedName = RoleNormalizedName.Driver };
                _dbContext.Roles.Add(newRole);
                userRole.RoleId = newRole.Id;
            }
            else
            {
                userRole.RoleId = role.Id;
            }

            var user = _mapper.Map<RegisterModel, User>(model);

            var checkCreateSuccess = await _userManager.CreateAsync(user, model.Password);

            if (!checkCreateSuccess.Succeeded)
            {
                result.ErrorMessage = checkCreateSuccess.ToString();
                result.Succeed = false;
                return result;
            }
            userRole.UserId = user.Id;
            _dbContext.UserRoles.Add(userRole);
            await _dbContext.SaveChangesAsync();
            result.Succeed = true;
            result.Data = user.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> RegisterDriverByAdmin(RegisterDriverByAdminModel model, Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }

            var checkEmailExisted = await _userManager.FindByEmailAsync(model.Email);
            if (checkEmailExisted != null)
            {
                result.ErrorMessage = "Email already existed";
                result.Succeed = false;
                return result;
            }
            var userRole = new UserRole { };

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.NormalizedName == RoleNormalizedName.Driver);
            if (role == null)
            {
                var newRole = new Role { Name = "Driver", NormalizedName = RoleNormalizedName.Driver };
                _dbContext.Roles.Add(newRole);
                userRole.RoleId = newRole.Id;
            }
            else
            {
                userRole.RoleId = role.Id;
            }

            var user = _mapper.Map<RegisterDriverByAdminModel, User>(model);

            var checkCreateSuccess = await _userManager.CreateAsync(user, model.PhoneNumber);

            if (!checkCreateSuccess.Succeeded)
            {
                result.ErrorMessage = checkCreateSuccess.ToString();
                result.Succeed = false;
                return result;
            }
            if (model.File != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Avatar", user.Id.ToString());
                user.Avatar = await MyFunction.UploadImageAsync(model.File, dirPath);
            }
            userRole.UserId = user.Id;
            _dbContext.UserRoles.Add(userRole);
            await _dbContext.SaveChangesAsync();
            result.Succeed = true;
            result.Data = _mapper.Map<UserModel>(user);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetDriver(PagingParam<DriverSortCriteria> paginationModel, SearchModel searchModel)
    {
        ResultModel result = new ResultModel();
        try
        {
            var data = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Driver) && !_.IsDeleted).AsQueryable();
            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var uses = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            uses = uses.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<UserModel>(uses);
            paging.Data = viewModels;
            result.Data = paging;
            result.Succeed = true;

        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateLocation(LocationModel model, Guid driverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users.Include(_ => _.DriverLocations).Where(_ => _.Id == driverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be a driver";
                result.Succeed = false;
                return result;
            }

            var driverLocation = driver.DriverLocations.FirstOrDefault();

            if (driverLocation == null)
            {
                driverLocation = _mapper.Map<LocationModel, DriverLocation>(model);
                driverLocation.DriverId = driver.Id;
                _dbContext.DriverLocations.Add(driverLocation);
            }
            else
            {
                driverLocation.Latitude = model.Latitude;
                driverLocation.Longitude = model.Longitude;
                driverLocation.DateUpdated = DateTime.Now;
                _dbContext.DriverLocations.Update(driverLocation);
            }
            await _dbContext.SaveChangesAsync();
            result.Succeed = true;
            result.Data = driverLocation.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> TrackingDriverLocation(TrackingDriverLocationModel model, Guid driverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == model.CustomerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exists";
                result.Succeed = false;
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                result.Succeed = false;
                return result;
            }
            var driver = _dbContext.Users.Include(_ => _.DriverLocations).Where(_ => _.Id == driverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be a driver";
                result.Succeed = false;
                return result;
            }

            var driverLocation = driver.DriverLocations.FirstOrDefault();

            if (driverLocation == null)
            {
                driverLocation = _mapper.Map<TrackingDriverLocationModel, DriverLocation>(model);
                driverLocation.DriverId = driver.Id;
                _dbContext.DriverLocations.Add(driverLocation);
            }
            else
            {
                driverLocation.Latitude = model.Latitude;
                driverLocation.Longitude = model.Longitude;
                driverLocation.DateUpdated = DateTime.Now;
                _dbContext.DriverLocations.Update(driverLocation);
            }
            await _dbContext.SaveChangesAsync();

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { model.CustomerId }, Payload = model };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-tracking-driver-location", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Succeed = true;
            result.Data = model;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateStatusOnline(Guid driverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Include(_ => _.DriverLocations)
                .Where(_ => _.Id == driverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a driver";
                result.Succeed = false;
                return result;
            }

            var driverStatus = driver.DriverStatuses.FirstOrDefault();

            if (driverStatus == null)
            {
                driverStatus = new DriverStatus { DriverId = driver.Id, IsOnline = true, IsFree = true };
                _dbContext.DriverStatuses.Add(driverStatus);
            }
            else
            {
                driverStatus.IsOnline = true;
                driverStatus.IsFree = true;
                driverStatus.DateUpdated = DateTime.Now;
                _dbContext.DriverStatuses.Update(driverStatus);
            }
            await _dbContext.SaveChangesAsync();

            var driverLocations = _mapper.Map<LocationModel>(driver.DriverLocations.FirstOrDefault());
            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { driverId }, Payload = driverLocations };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-driver-status-online", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Succeed = true;
            result.Data = driverStatus.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateStatusOffline(Guid driverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var driver = _dbContext.Users
                .Include(_ => _.DriverLocations)
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == driverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a driver";
                result.Succeed = false;
                return result;
            }

            var driverStatus = driver.DriverStatuses.FirstOrDefault();

            if (driverStatus == null)
            {
                driverStatus = new DriverStatus { DriverId = driver.Id, IsOnline = false, IsFree = false };
                _dbContext.DriverStatuses.Add(driverStatus);
            }
            else
            {
                driverStatus.IsOnline = false;
                driverStatus.IsFree = false;
                driverStatus.DateUpdated = DateTime.Now;
                _dbContext.DriverStatuses.Update(driverStatus);
            }
            await _dbContext.SaveChangesAsync();

            var driverLocations = _mapper.Map<LocationModel>(driver.DriverLocations.FirstOrDefault());
            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { driverId }, Payload = driverLocations };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-driver-status-offline", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Succeed = true;
            result.Data = driverStatus.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateAllDriverStatusOffline()
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var drivers = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Where(_ => !_.IsDeleted).ToList();
            if (drivers == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }

            foreach (var driver in drivers)
            {
                var driverStatus = driver.DriverStatuses.FirstOrDefault();

                if (driverStatus == null)
                {
                    driverStatus = new DriverStatus { DriverId = driver.Id, IsOnline = false, IsFree = false };
                    _dbContext.DriverStatuses.Add(driverStatus);
                }
                else
                {
                    driverStatus.IsOnline = false;
                    driverStatus.IsFree = false;
                    _dbContext.DriverStatuses.Update(driverStatus);
                }
            }

            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetDriverOnline(LocationCustomer locationCustomer, Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            FormattableString fsQuery = null;
            if (locationCustomer.IsFemaleDriver)
            {
                fsQuery =
                    $@"
                select distinct  anu.""Id"", anu.""Email"", ds.""IsOnline"", dl.""Latitude"", dl.""Longitude"", anu.""Priority"", anu.""Star""
                from
                ""AspNetUsers"" anu 
                join ""DriverLocations"" dl on anu.""Id"" = dl.""DriverId"" 
                join ""DriverStatuses"" ds on ds.""DriverId"" = anu.""Id"" 
                where ds.""IsOnline"" = true 	
                    AND ds.""IsFree"" = true
                    AND anu.""Gender"" = 1
                    AND anu.""IsPublicGender"" = true
	                AND (
                    6371 * acos(
                        cos(radians({locationCustomer.Latitude}))
                        * cos(radians(dl.""Latitude""))
                        * cos(radians(dl.""Longitude"") - radians({locationCustomer.Longitude}))
                        + sin(radians({locationCustomer.Latitude}))
                        * sin(radians(dl.""Latitude""))
                    )
                ) <= {locationCustomer.Radius} 
                order by 
                    anu.""Priority"" desc, 
                    anu.""Star"" desc";
            }
            else
            {
                fsQuery =
                    $@"
                select distinct  anu.""Id"", anu.""Email"", ds.""IsOnline"", dl.""Latitude"", dl.""Longitude"", anu.""Priority"", anu.""Star""
                from
                ""AspNetUsers"" anu 
                join ""DriverLocations"" dl on anu.""Id"" = dl.""DriverId"" 
                join ""DriverStatuses"" ds on ds.""DriverId"" = anu.""Id"" 
                where ds.""IsOnline"" = true 	
                    AND ds.""IsFree"" = true
	                AND (
                    6371 * acos(
                        cos(radians({locationCustomer.Latitude}))
                        * cos(radians(dl.""Latitude""))
                        * cos(radians(dl.""Longitude"") - radians({locationCustomer.Longitude}))
                        + sin(radians({locationCustomer.Latitude}))
                        * sin(radians(dl.""Latitude""))
                    )
                ) <= {locationCustomer.Radius} 
                order by 
                    anu.""Priority"" desc,
                    anu.""Star"" desc";
            }
            var data = _dbContext.Set<DriverOnlineModel>().FromSqlRaw(fsQuery.ToString()).AsQueryable();
            result.Data = data.ToList();
            result.Succeed = true;
            await _dbContext.SaveChangesAsync();

            var driverOnlineSignalR = new DriverOnlineSignalRModel
            {
                CustomerId = userId,
                Latitude = locationCustomer.Latitude,
                Longitude = locationCustomer.Longitude,
                Radius = locationCustomer.Radius,
            };
            var driverOnlines = data.ToList();
            foreach (var item in driverOnlines)
            {
                driverOnlineSignalR.ListDrivers.Add(item.Id);
            }

            var kafkaModel = new KafkaModel { UserReceiveNotice = new List<Guid>() { userId }, Payload = driverOnlineSignalR };
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(kafkaModel);
            await _producer.ProduceAsync("dbs-get-driver-online", new Message<Null, string> { Value = json });
            _producer.Flush();
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetDriverStatistics(Guid driverId, int year)
    {
        ResultModel result = new ResultModel();
        try
        {
            var driver = _dbContext.Users.Where(d => d.Id == driverId && !d.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }
            if (!driver.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be Driver";
                return result;
            }

            var bookings = _dbContext.Bookings.Where(_ => _.DriverId == driverId && _.DateCreated.Year == year).ToList();

            int totalBookings = bookings.Count;
            int canceledBookings = bookings.Count(b => b.Status == BookingStatus.Cancel);
            int completedBookings = bookings.Count(b => b.Status == BookingStatus.Complete);

            DriverStatisticModel driverStatistics = new()
            {
                BookingAcceptanceRate = driver.TotalRequest == 0 ? "100%" : ((float)(driver.TotalRequest - driver.DeclineRequest) / driver.TotalRequest * 100).ToString("0.##") + "%",
                BookingCancellationRate = totalBookings == 0 ? "0%" : ((float)canceledBookings / totalBookings * 100).ToString("0.##") + "%",
                BookingCompletionRate = totalBookings == 0 ? "0%" : ((float)completedBookings / totalBookings * 100).ToString("0.##") + "%",
                OperationalMonths = bookings.Select(_ => _.DateCreated.Month).Distinct().ToList()
            };

            result.Succeed = true;
            result.Data = driverStatistics;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }


    public async Task<ResultModel> GetDriverMonthlyStatistics(Guid driverId, int month, int year)
    {
        ResultModel result = new ResultModel();
        try
        {
            var driver = await _dbContext.Users
                .Where(d => d.Id == driverId && !d.IsDeleted)
                .FirstOrDefaultAsync();

            if (driver == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }

            if (!driver.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }

            var monthlyBookings = await _dbContext.Bookings
                .Include(_ => _.SearchRequest)
                .Where(b => b.DriverId == driverId && b.DateCreated.Month == month && b.DateCreated.Year == year)
                .ToListAsync();
            var totalTrips = monthlyBookings.Count;
            var totalTripsCompleted = monthlyBookings.Count(_ => _.Status == BookingStatus.Complete);

            var totalOperatingTimeSpan = monthlyBookings
                .Select(b => b.DateUpdated - b.DateCreated)
                .Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t));

            string totalOperatingTime = $"{(int)totalOperatingTimeSpan.TotalHours} giờ {totalOperatingTimeSpan.Minutes} phút";

            var wallet = _dbContext.Wallets.Where(_ => _.UserId == driver.Id).FirstOrDefault();
            var walletTransactions = await _dbContext.WalletTransactions
                .Where(_ => _.WalletId == wallet.Id && _.TypeWalletTransaction == TypeWalletTransaction.Income && _.DateCreated.Month == month && _.DateCreated.Year == year)
                .ToListAsync();

            long totalMoney = walletTransactions.Sum(b => b.TotalMoney);

            var dailyStatistics = walletTransactions
            .GroupBy(wt => wt.DateCreated.Day)
            .Select(g => new DriverStatisticDaylyModel
            {
                Day = g.Key,
                TotalTrip = monthlyBookings.Count(b => b.DateCreated.Day == g.Key),
                TotalTripCompleted = monthlyBookings.Count(b => b.DateCreated.Day == g.Key && b.Status == BookingStatus.Complete),
                TotalIncome = g.Sum(wt => wt.TotalMoney),
                TotalOperatiingTime = $"{(int)monthlyBookings.Where(b => b.DateCreated.Day == g.Key).Select(b => b.DateUpdated - b.DateCreated).Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t)).TotalHours} giờ {monthlyBookings.Where(b => b.DateCreated.Day == g.Key).Select(b => b.DateUpdated - b.DateCreated).Aggregate(TimeSpan.Zero, (subtotal, t) => subtotal.Add(t)).Minutes} phút"
            })
            .ToList();

            DriverStatisticMonthlyModel driverMonthlyStatistics = new DriverStatisticMonthlyModel
            {
                Month = month,
                TotalMoney = totalMoney,
                TotalOperatingTime = totalOperatingTime,
                TotalTrips = totalTrips,
                TotalTripsCompleted = totalTripsCompleted,
                DriverStatisticDayly = dailyStatistics
            };

            result.Succeed = true;
            result.Data = driverMonthlyStatistics;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }


}
