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
using Services.Utils;


namespace Services.Core;

public interface IAdminService
{
    Task<ResultModel> LoginAsManager(LoginModel model);
    Task<ResultModel> BanAccount(BanAccountModel model, Guid adminId);
    Task<ResultModel> UnBanAccount(BanAccountModel model, Guid adminId);
    Task<ResultModel> GetByIdForAdmin(Guid bookingId, Guid AdminId);
    Task<ResultModel> GetBookingsForAdmin(PagingParam<SortBookingCriteria> paginationModel, SearchModel searchModel, BookingFilterModel filterModel, Guid UserId);
    Task<ResultModel> GetUserByAdmin(PagingParam<UserSortByAdminCriteria> paginationModel, SearchModel searchModel, AccountFilterModel model, Guid AdminId);
    Task<ResultModel> RegisterDriverByAdmin(RegisterDriverByAdminModel model, Guid UserId);
    Task<ResultModel> RegisterStaffByAdmin(RegisterStaffByAdminModel model, Guid UserId);
    Task<ResultModel> AddByAdmin(DrivingLicenseCreateModel model, Guid DriverId, Guid AdminId);
    Task<ResultModel> GetByAdmin(Guid DriverId, Guid AdminId);
    Task<ResultModel> GetByIDByAdmin(Guid DrivingLicenseId, Guid DriverId, Guid AdminId);
    Task<ResultModel> UpdateByAdmin(DrivingLicenseUpdateModel model, Guid DrivingLicenseId, Guid DriverId, Guid AdminId);
    Task<ResultModel> DeleteByAdmin(Guid DrivingLicenseId, Guid DriverId, Guid AdminId);
    Task<ResultModel> AddImageByAdmin(DrivingLicenseImageCreateModel model, Guid AdminId);
    Task<ResultModel> GetImagesByDrivingLicenseIdByAdmin(Guid DrivingLicenseId, Guid AdminId);
    Task<ResultModel> UpdateImageByAdmin(DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImageId, Guid AdminId);
    Task<ResultModel> DeleteImageByAdmin(Guid DrivingLicenseImageId, Guid AdminId);
    Task<ResultModel> DownloadImageByAdmin(FileModel model, Guid AdminId);
    Task<ResultModel> AddByAdmin(IdentityCardCreateModel model, Guid UserId, Guid AdminId);
    Task<ResultModel> GetIdentitCardByAdmin(Guid UserId, Guid AdminId);
    Task<ResultModel> UpdateByAdmin(IdentityCardUpdateModel model, Guid UserId, Guid AdminId);
    Task<ResultModel> DeleteByAdmin(Guid UserId, Guid AdminId);
    Task<ResultModel> AddImageByAdmin(IdentityCardImageCreateModel model, Guid AdminId);
    Task<ResultModel> GetImagesByIdentityCardIdByAdmin(Guid IdentityCardId, Guid AdminId);
    Task<ResultModel> UpdateImageByAdmin(IdentityCardImageUpdateModel model, Guid IdentityCardImageId, Guid AdminId);
    Task<ResultModel> DeleteIdentityCardImageByAdmin(Guid IdentityCardImageId, Guid AdminId);
    Task<ResultModel> DownloadIdentityCardImageByAdmin(FileModel model, Guid AdminId);
    Task<ResultModel> GetAllByAdmin(Guid AdminId, Guid customerId);
    Task<ResultModel> UpdatePriceConfiguration(PriceConfigurationUpdateModel model, Guid AdminId);
    Task<ResultModel> GetWithdrawFundsRequest(PagingParam<SortWithdrawFundsTransactionCriteria> paginationModel, SearchModel searchModel, WithdrawFundsRequestFilterModel filterModel, Guid adminId);
    Task<ResultModel> UpdateUserPriorityById(UpdateUserPriorityModel model, Guid adminId);
    Task<ResultModel> UpdateStaffStatusOffline(Guid staffId);
    Task<ResultModel> UpdateStaffStatusOnline(Guid staffId);
    Task<ResultModel> GetAdminOverview(Guid adminId);
    Task<ResultModel> GetAdminRevenueMonthlyIncome(Guid adminId, int year);
    Task<ResultModel> GetAdminProfitMonthlyIncome(Guid adminId, int year);
    Task<ResultModel> GetStaffList(Guid adminId);
}

public class AdminService : IAdminService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IProducer<Null, string> _producer;

    public AdminService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, UserManager<User> userManager, SignInManager<User> signInManager, IProducer<Null, string> producer)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _producer = producer;
    }

    public async Task<ResultModel> LoginAsManager(LoginModel model)
    {

        var result = new ResultModel();
        try
        {
            var userByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userByEmail == null)
            {
                result.ErrorMessage = "Email not exists";
                result.Succeed = false;
                return result;
            }
            if (!userByEmail.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var check = await _signInManager.CheckPasswordSignInAsync(userByEmail, model.Password, false);
            if (!check.Succeeded)
            {
                result.Succeed = false;
                result.ErrorMessage = "Password isn't correct";
                return result;
            }
            var userRoles = _dbContext.UserRoles.Where(ur => ur.UserId == userByEmail.Id).ToList();
            var roles = new List<string>();
            foreach (var userRole in userRoles)
            {
                var role = await _dbContext.Roles.FindAsync(userRole.RoleId);
                if (role != null) roles.Add(role.Name);
            }
            if (!roles[0].Equals("Admin") && !roles[0].Equals("Staff"))
            {
                result.ErrorMessage = "You are not Manager";
                return result;
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_mapper.Map<UserModel>(userByEmail));
            await _producer.ProduceAsync("dbs-user-create-new", new Message<Null, string> { Value = json });
            _producer.Flush();

            var token = await MyFunction.GetAccessToken(userByEmail, roles, _configuration, _dbContext);
            result.Succeed = true;
            result.Data = token;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> BanAccount(BanAccountModel model, Guid adminId)
    {
        ResultModel result = new ResultModel();
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == adminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exists";
                result.Succeed = false;
                return result;
            }
            if (!admin.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "Admin has been deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                result.Succeed = false;
                return result;
            }
            var user = _dbContext.Users.Where(_ => _.Id == model.UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exists";
                result.Succeed = false;
                return result;
            }
            if (!user.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "User has been deactivated";
                return result;
            }

            user.IsActive = false;
            user.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var dataView = _mapper.Map<ProfileModel>(user);

            result.Succeed = true;
            result.Data = dataView;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }

    public async Task<ResultModel> UnBanAccount(BanAccountModel model, Guid adminId)
    {
        ResultModel result = new ResultModel();
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == adminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exists";
                result.Succeed = false;
                return result;
            }
            if (!admin.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "Admin has been deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                result.Succeed = false;
                return result;
            }
            var user = _dbContext.Users.Where(_ => _.Id == model.UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exists";
                result.Succeed = false;
                return result;
            }

            user.IsActive = true;
            user.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var dataView = _mapper.Map<ProfileModel>(user);

            result.Succeed = true;
            result.Data = dataView;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetByIdForAdmin(Guid BookingId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var bookingCancel = _dbContext.BookingCancels
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.SearchRequest)
                        .ThenInclude(sr => sr.Customer)
                .Include(_ => _.Booking)
                    .ThenInclude(booking => booking.Driver)
                .Include(_ => _.CancelPerson)
                    .ThenInclude(_ => _.UserRoles)
                        .ThenInclude(_ => _.Role)
                .Where(_ => _.BookingId == BookingId && !_.IsDeleted)
                .FirstOrDefault();

            var data = _mapper.Map<BookingCancelModel>(bookingCancel);
            data.CancelPerson.Role = bookingCancel.CancelPerson.UserRoles.FirstOrDefault().Role.Name;
            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetBookingsForAdmin(PagingParam<SortBookingCriteria> paginationModel, SearchModel searchModel, BookingFilterModel filterModel, Guid UserId)
    {
        ResultModel result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exist";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var data = _dbContext.Bookings
                    .Where(_ => !_.IsDeleted)
                    .AsQueryable();

            // Apply search filter
            if (searchModel != null && !string.IsNullOrEmpty(searchModel.SearchValue))
            {
                data = data.Where(booking => booking.Id.ToString().Contains(searchModel.SearchValue));
            }

            // Apply additional filters
            if (filterModel != null)
            {
                if (filterModel.Status != null && filterModel.Status.Any())
                {
                    data = data.Where(booking => filterModel.Status.Contains(booking.Status));
                }
            }

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var bookings = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            bookings = bookings.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewModels = _mapper.ProjectTo<BookingModel>(bookings);

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

    public async Task<ResultModel> GetUserByAdmin(PagingParam<UserSortByAdminCriteria> paginationModel, SearchModel searchModel, AccountFilterModel filterModel, Guid AdminId)
    {
        ResultModel result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }

            var data = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => !_.IsDeleted)
                    .Select(user => new UserModelByAdmin
                    {
                        Id = user.Id,
                        Name = user.Name,
                        PhoneNumber = user.PhoneNumber,
                        UserName = user.UserName,
                        Email = user.Email,
                        Address = user.Address,
                        Star = user.Star,
                        Avatar = user.Avatar,
                        Gender = user.Gender,
                        Dob = user.Dob,
                        DateCreated = user.DateCreated,
                        Role = user.UserRoles.FirstOrDefault().Role.Name,
                        IsActive = user.IsActive,
                        IsPublicGender = user.IsPublicGender
                    })
                    .AsQueryable();

            // Apply search filter if searchModel is not null
            if (searchModel != null && !string.IsNullOrEmpty(searchModel.SearchValue))
            {
                data = data.Where(user => user.Email.Contains(searchModel.SearchValue));
            }

            // Apply additional filters from AccountFilterModel
            if (filterModel != null)
            {
                if (filterModel.Gender != null && filterModel.Gender.Any())
                {
                    data = data.Where(user => filterModel.Gender.Contains(user.Gender.Value));
                }
                if (filterModel.IsActive != null && filterModel.IsActive.Any())
                {
                    data = data.Where(user => filterModel.IsActive.Contains(user.IsActive));
                }
                if (filterModel.Role != null && filterModel.Role.Any())
                {
                    data = data.Where(user => filterModel.Role.Contains(user.Role));
                }
            }

            var paging = new PagingModel(paginationModel.PageIndex, paginationModel.PageSize, data.Count());
            var uses = data.GetWithSorting(paginationModel.SortKey.ToString(), paginationModel.SortOrder);
            uses = uses.GetWithPaging(paginationModel.PageIndex, paginationModel.PageSize);
            var viewData = uses.ToList();

            paging.Data = viewData;
            result.Data = paging;
            result.Succeed = true;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
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

    public async Task<ResultModel> RegisterStaffByAdmin(RegisterStaffByAdminModel model, Guid UserId)
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
            if (!checkAdmin)
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

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.NormalizedName == RoleNormalizedName.Staff);
            if (role == null)
            {
                var newRole = new Role { Name = "Staff", NormalizedName = RoleNormalizedName.Staff };
                _dbContext.Roles.Add(newRole);
                userRole.RoleId = newRole.Id;
            }
            else
            {
                userRole.RoleId = role.Id;
            }

            var user = _mapper.Map<RegisterStaffByAdminModel, User>(model);

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

            var staffStatus = new DriverStatus { DriverId = user.Id, IsOnline = true, IsFree = true };
            _dbContext.DriverStatuses.Add(staffStatus);
            user.DriverStatuses.Add(staffStatus);

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

    public async Task<ResultModel> AddByAdmin(DrivingLicenseCreateModel model, Guid DriverId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist!";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be a driver";
                return result;
            }
            if (!driver.IsActive)
            {
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }
            var checkExistDrivingLicenseNumber = _dbContext.DrivingLicenses
                .Where(_ => _.DrivingLicenseNumber == model.DrivingLicenseNumber && !_.IsDeleted).FirstOrDefault();
            if (checkExistDrivingLicenseNumber != null)
            {
                result.ErrorMessage = $"Driving License with Number {model.DrivingLicenseNumber} existed";
            }
            var drivingLicense = _mapper.Map<DrivingLicenseCreateModel, DrivingLicense>(model);
            drivingLicense.DriverId = driver.Id;
            _dbContext.DrivingLicenses.Add(drivingLicense);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = drivingLicense.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> AddImageByAdmin(DrivingLicenseImageCreateModel model, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var drivingLicense = _dbContext.DrivingLicenses.Where(_ => _.Id == model.DrivingLicenseId && !_.IsDeleted).FirstOrDefault();
            if (drivingLicense == null)
            {
                result.ErrorMessage = "Driving License not exist!";
                return result;
            }
            var drivingLicenseImage = _mapper.Map<DrivingLicenseImageCreateModel, DrivingLicenseImage>(model);
            _dbContext.DrivingLicenseImages.Add(drivingLicenseImage);
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "DrivingLicenseImage", drivingLicenseImage.Id.ToString());
            drivingLicenseImage.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<DrivingLicenseImageModel>(drivingLicenseImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteByAdmin(Guid DrivingLicenseId, Guid DriverId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist!";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be a driver";
                return result;
            }
            if (!driver.IsActive)
            {
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }
            var drivingLicense = _dbContext.DrivingLicenses.Where(_ => _.Id == DrivingLicenseId && _.DriverId == DriverId && !_.IsDeleted).FirstOrDefault();
            if (drivingLicense == null)
            {
                result.ErrorMessage = "Driving License not exist!";
                return result;
            }
            _dbContext.DrivingLicenses.Remove(drivingLicense);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Driving License successful";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteImageByAdmin(Guid DrivingLicenseImageId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var drivingLicenseImage = _dbContext.DrivingLicenseImages.Where(_ => _.Id == DrivingLicenseImageId && !_.IsDeleted).FirstOrDefault();
            if (drivingLicenseImage == null)
            {
                result.ErrorMessage = "Driving License Image not exist!";
                return result;
            }
            string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            MyFunction.DeleteFile(dirPathDelete + drivingLicenseImage.ImageUrl);

            _dbContext.DrivingLicenseImages.Remove(drivingLicenseImage);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Driving License Image successful";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DownloadImageByAdmin(FileModel model, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var drivingLicenseImage = _dbContext.DrivingLicenseImages.Where(_ => _.Id == model.Id && !_.IsDeleted).FirstOrDefault();
            if (drivingLicenseImage == null)
            {
                result.Succeed = false;
                result.ErrorMessage = "Driving License Image not found";
            }
            else
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                if (drivingLicenseImage.ImageUrl == null || !drivingLicenseImage.ImageUrl.Contains(model.Path))
                {
                    result.ErrorMessage = "Image does not exist";
                    result.Succeed = false;
                    return result;
                }
                result.Data = await MyFunction.DownloadFile(dirPath + model.Path);
                result.Succeed = true;
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetByAdmin(Guid DriverId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist!";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be a driver";
                return result;
            }
            if (!driver.IsActive)
            {
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }
            var drivingLicense = _dbContext.DrivingLicenses
                .Include(_ => _.Driver)
                .Where(_ => _.DriverId == DriverId && !_.IsDeleted).ToList();
            if (drivingLicense == null)
            {
                result.ErrorMessage = "Driving License not exist!";
                return result;
            }
            result.Succeed = true;
            result.Data = _mapper.Map<List<DrivingLicenseModel>>(drivingLicense);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetByIDByAdmin(Guid DrivingLicenseId, Guid DriverId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist!";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be a driver";
                return result;
            }
            if (!driver.IsActive)
            {
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }
            var drivingLicense = _dbContext.DrivingLicenses
                .Include(_ => _.Driver)
                .Where(_ => _.Id == DrivingLicenseId && _.DriverId == DriverId && !_.IsDeleted).FirstOrDefault();
            if (drivingLicense == null)
            {
                result.ErrorMessage = "Driving License not exist!";
                return result;
            }
            result.Succeed = true;
            result.Data = _mapper.Map<DrivingLicenseModel>(drivingLicense);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetImagesByDrivingLicenseIdByAdmin(Guid DrivingLicenseId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var drivingLicenseImages = _dbContext.DrivingLicenseImages
                .Include(_ => _.DrivingLicense)
                .Where(_ => _.DrivingLicenseId == DrivingLicenseId && !_.IsDeleted)
                .ToList();
            if (drivingLicenseImages == null || drivingLicenseImages.Count == 0)
            {
                result.ErrorMessage = "Driving License Image not exist!";
                return result;
            }
            var data = _mapper.Map<List<DrivingLicenseImageModel>>(drivingLicenseImages);
            for (int i = 0; i < data.Count; i++)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + data[i].ImageUrl;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                data[i].ImageUrl = Convert.ToBase64String(imageBytes);
            }

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateByAdmin(DrivingLicenseUpdateModel model, Guid DrivingLicenseId, Guid DriverId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var driver = _dbContext.Users.Where(_ => _.Id == DriverId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exist!";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be a driver";
                return result;
            }
            if (!driver.IsActive)
            {
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }
            var drivingLicense = _dbContext.DrivingLicenses
                 .Include(_ => _.Driver)
                .Where(_ => _.Id == DrivingLicenseId && _.DriverId == DriverId && !_.IsDeleted).FirstOrDefault();
            if (drivingLicense == null)
            {
                result.ErrorMessage = "Driving License not exist!";
                return result;
            }
            if (model.DrivingLicenseNumber != null && drivingLicense.DrivingLicenseNumber != model.DrivingLicenseNumber)
            {
                drivingLicense.DrivingLicenseNumber = model.DrivingLicenseNumber;
            }
            if (model.Type != null)
            {
                drivingLicense.Type = model.Type;
            }
            if (model.IssueDate != null)
            {
                drivingLicense.IssueDate = (DateOnly)model.IssueDate;
            }
            if (model.ExpiredDate != null)
            {
                drivingLicense.ExpiredDate = (DateOnly)model.ExpiredDate;
            }
            drivingLicense.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<DrivingLicenseModel>(drivingLicense);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateImageByAdmin(DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImageId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var drivingLicenseImage = _dbContext.DrivingLicenseImages
                .Include(_ => _.DrivingLicense)
                .Where(_ => _.Id == DrivingLicenseImageId && !_.IsDeleted).FirstOrDefault();
            if (drivingLicenseImage == null)
            {
                result.ErrorMessage = "Driving License Image not exist!";
                return result;
            }
            if (model.File != null)
            {
                string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                MyFunction.DeleteFile(dirPathDelete + drivingLicenseImage.ImageUrl);
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "DrivingLicenseImage", drivingLicenseImage.Id.ToString());
                drivingLicenseImage.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            }
            drivingLicenseImage.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<DrivingLicenseImageModel>(drivingLicenseImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> AddByAdmin(IdentityCardCreateModel model, Guid UserId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }

            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exist!";
                return result;
            }
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            if (model.Dob != null)
            {
                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
                DateOnly dob = new DateOnly(model.Dob.Value.Year, model.Dob.Value.Month, model.Dob.Value.Day);

                int ageDifferenceInYears = currentDate.Year - dob.Year;

                if (dob > currentDate.AddYears(-ageDifferenceInYears))
                {
                    ageDifferenceInYears--;
                }
                if (ageDifferenceInYears < 18)
                {
                    result.ErrorMessage = "User must be at least 18 years old.";
                    result.Succeed = false;
                    return result;
                }
            }
            var checkExist = _dbContext.IdentityCards.Where(_ => _.UserId == UserId && !_.IsDeleted).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = "User has already added ID Card";
                return result;
            }
            if (model.IdentityCardNumber != null)
            {
                var checkExistCardNumber = _dbContext.IdentityCards.Where(_ => _.IdentityCardNumber == model.IdentityCardNumber && !_.IsDeleted).FirstOrDefault();
                if (checkExistCardNumber != null)
                {
                    result.ErrorMessage = "Identity Card Number has been existed";
                    return result;
                }
            }
            var identityCard = _mapper.Map<IdentityCardCreateModel, IdentityCard>(model);
            identityCard.UserId = user.Id;
            _dbContext.IdentityCards.Add(identityCard);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = identityCard.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteByAdmin(Guid UserId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exist!";
                return result;
            }
            if (!user.IsActive)
            {
                result.ErrorMessage = "Driver has been deactivated";
                return result;
            }
            var identityCard = _dbContext.IdentityCards.Where(_ => _.UserId == UserId && !_.IsDeleted).FirstOrDefault();
            if (identityCard == null)
            {
                result.ErrorMessage = "Identity Card not exist!";
                return result;
            }
            _dbContext.IdentityCards.Remove(identityCard);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Identity Card successful";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetIdentitCardByAdmin(Guid UserId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exist!";
                return result;
            }
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var identityCard = _dbContext.IdentityCards.Where(_ => _.UserId == UserId && !_.IsDeleted).FirstOrDefault();
            if (identityCard == null)
            {
                result.ErrorMessage = "Identity Card not exist!";
                return result;
            }
            result.Succeed = true;
            result.Data = _mapper.Map<IdentityCardModel>(identityCard);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateByAdmin(IdentityCardUpdateModel model, Guid UserId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exist!";
                return result;
            }
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var identityCard = _dbContext.IdentityCards.Where(_ => _.UserId == UserId && !_.IsDeleted).FirstOrDefault();
            if (identityCard == null)
            {
                result.ErrorMessage = "Identity Card not exist!";
                return result;
            }
            if (model.FullName != null)
            {
                identityCard.FullName = model.FullName;
            }
            if (model.Dob != null)
            {
                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
                DateOnly dob = new DateOnly(model.Dob.Value.Year, model.Dob.Value.Month, model.Dob.Value.Day);

                int ageDifferenceInYears = currentDate.Year - dob.Year;

                if (dob > currentDate.AddYears(-ageDifferenceInYears))
                {
                    ageDifferenceInYears--;
                }
                if (ageDifferenceInYears < 18)
                {
                    result.ErrorMessage = "User must be at least 18 years old.";
                    result.Succeed = false;
                    return result;
                }
                identityCard.Dob = (DateOnly)model.Dob;
            }
            if (model.Gender != null)
            {
                identityCard.Gender = (Data.Enums.Gender)model.Gender;
            }
            if (model.Nationality != null)
            {
                identityCard.Nationality = model.Nationality;
            }
            if (model.PlaceOrigin != null)
            {
                identityCard.PlaceOrigin = model.PlaceOrigin;
            }
            if (model.PlaceResidence != null)
            {
                identityCard.PlaceResidence = model.PlaceResidence;
            }
            if (model.PersonalIdentification != null)
            {
                identityCard.PersonalIdentification = model.PersonalIdentification;
            }
            if (model.ExpiredDate != null)
            {
                identityCard.ExpiredDate = (DateOnly)model.ExpiredDate;
            }
            if (model.IdentityCardNumber != null && identityCard.IdentityCardNumber != model.IdentityCardNumber)
            {
                identityCard.IdentityCardNumber = model.IdentityCardNumber;
            }
            identityCard.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<IdentityCardModel>(identityCard);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> AddImageByAdmin(IdentityCardImageCreateModel model, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var identityCard = _dbContext.IdentityCards.Where(_ => _.Id == model.IdentityCardId && !_.IsDeleted).FirstOrDefault();
            if (identityCard == null)
            {
                result.ErrorMessage = "Identity Card not exist!";
                return result;
            }
            var identityCardImage = _mapper.Map<IdentityCardImageCreateModel, IdentityCardImage>(model);
            _dbContext.IdentityCardImages.Add(identityCardImage);
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "IdentityCardImage", identityCardImage.Id.ToString());
            identityCardImage.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<IdentityCardImageModel>(identityCardImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DownloadIdentityCardImageByAdmin(FileModel model, Guid AdminId)
    {
        var result = new ResultModel();
        try
        {
            var identityCardImage = _dbContext.IdentityCardImages.Where(_ => _.Id == model.Id && !_.IsDeleted).FirstOrDefault();
            if (identityCardImage == null)
            {
                result.Succeed = false;
                result.ErrorMessage = "IdentityCardImage not found";
            }
            else
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                if (identityCardImage.ImageUrl == null || !identityCardImage.ImageUrl.Contains(model.Path))
                {
                    result.ErrorMessage = "File does not exist";
                    result.Succeed = false;
                    return result;
                }
                result.Data = await MyFunction.DownloadFile(dirPath + model.Path);
                result.Succeed = true;
            }
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetImagesByIdentityCardIdByAdmin(Guid IdentityCardId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }

            var identityCardImages = _dbContext.IdentityCardImages
                .Include(_ => _.IdentityCard)
                .Where(_ => _.IdentityCardId == IdentityCardId && !_.IsDeleted)
                .ToList();
            if (identityCardImages == null || identityCardImages.Count == 0)
            {
                result.ErrorMessage = "Identity Card Image not exist!";
                return result;
            }
            var data = _mapper.Map<List<IdentityCardImageModel>>(identityCardImages);
            for (int i = 0; i < data.Count; i++)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + data[i].ImageUrl;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                data[i].ImageUrl = Convert.ToBase64String(imageBytes);
            }

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateImageByAdmin(IdentityCardImageUpdateModel model, Guid IdentityCardImageId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }

            var identityCardImage = _dbContext.IdentityCardImages.Where(_ => _.Id == IdentityCardImageId && !_.IsDeleted).FirstOrDefault();
            if (identityCardImage == null)
            {
                result.ErrorMessage = "Identity Card Image not exist!";
                return result;
            }
            if (model.File != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "IdentityCardImage", identityCardImage.Id.ToString());
                identityCardImage.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            }
            identityCardImage.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<IdentityCardImageModel>(identityCardImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteIdentityCardImageByAdmin(Guid IdentityCardImageId, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not found";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var identityCardImage = _dbContext.IdentityCardImages.Where(_ => _.Id == IdentityCardImageId && !_.IsDeleted).FirstOrDefault();
            if (identityCardImage == null)
            {
                result.ErrorMessage = "Identity Card Image not exist!";
                return result;
            }
            _dbContext.IdentityCardImages.Remove(identityCardImage);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Identity Card Image successful";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetAllByAdmin(Guid AdminId, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist!";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be a Admin";
                return result;
            }
            if (!admin.IsActive)
            {
                result.ErrorMessage = "Admin has been deactivated";
                return result;
            }
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!customer.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var vehicles = _dbContext.Vehicles
                .Include(_ => _.Customer)
                .Where(_ => _.CustomerId == customerId && !_.IsDeleted)
                .ToList();
            if (vehicles == null)
            {
                result.ErrorMessage = "Vehicle not exist!";
                return result;
            }
            var data = _mapper.Map<List<VehicleModel>>(vehicles);
            foreach (var item in data)
            {
                var vehicleImage = _dbContext.VehicleImages.Where(_ => _.VehicleId == item.Id && !_.IsDeleted).FirstOrDefault();
                if (vehicleImage != null)
                {
                    item.ImagePath = vehicleImage.ImageUrl;
                    item.ImageUrl = vehicleImage.ImageUrl;
                }
            }

            result.Data = data;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdatePriceConfiguration(PriceConfigurationUpdateModel model, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;

        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist!";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be a Admin";
                return result;
            }
            if (!admin.IsActive)
            {
                result.ErrorMessage = "Admin has been deactivated";
                return result;
            }

            var priceConfiguration = _dbContext.PriceConfigurations.FirstOrDefault();
            if (priceConfiguration == null)
            {
                result.ErrorMessage = "Price Configuration not exist";
                return result;
            }
            if (model.BaseFareFirst3km != null)
            {
                priceConfiguration.BaseFareFirst3km.Price = model.BaseFareFirst3km.Price != null ? model.BaseFareFirst3km.Price : priceConfiguration.BaseFareFirst3km.Price;
                priceConfiguration.BaseFareFirst3km.IsPercent = model.BaseFareFirst3km.IsPercent != null ? model.BaseFareFirst3km.IsPercent : priceConfiguration.BaseFareFirst3km.IsPercent;
            }
            if (model.FareFerAdditionalKm != null)
            {
                priceConfiguration.FareFerAdditionalKm.Price = model.FareFerAdditionalKm.Price != null ? model.FareFerAdditionalKm.Price : priceConfiguration.FareFerAdditionalKm.Price;
                priceConfiguration.FareFerAdditionalKm.IsPercent = model.FareFerAdditionalKm.IsPercent != null ? model.FareFerAdditionalKm.IsPercent : priceConfiguration.FareFerAdditionalKm.IsPercent;
            }
            if (model.DriverProfit != null)
            {
                priceConfiguration.DriverProfit.Price = model.DriverProfit.Price != null ? model.DriverProfit.Price : priceConfiguration.DriverProfit.Price;
                priceConfiguration.DriverProfit.IsPercent = model.DriverProfit.IsPercent != null ? model.DriverProfit.IsPercent : priceConfiguration.DriverProfit.IsPercent; priceConfiguration.DriverProfit.Price = model.DriverProfit.Price != null ? model.DriverProfit.Price : priceConfiguration.DriverProfit.Price;
            }
            if (model.AppProfit != null)
            {
                priceConfiguration.AppProfit.Price = model.AppProfit.Price != null ? model.AppProfit.Price : priceConfiguration.AppProfit.Price;
                priceConfiguration.AppProfit.IsPercent = model.AppProfit.IsPercent != null ? model.AppProfit.IsPercent : priceConfiguration.AppProfit.IsPercent;
            }
            if (model.PeakHours != null)
            {
                priceConfiguration.PeakHours.Time = model.PeakHours.Time != null ? model.PeakHours.Time : priceConfiguration.PeakHours.Time;
                priceConfiguration.PeakHours.Price = model.PeakHours.Price != null ? model.PeakHours.Price : priceConfiguration.PeakHours.Price;
                priceConfiguration.PeakHours.IsPercent = model.PeakHours.IsPercent != null ? model.PeakHours.IsPercent : priceConfiguration.PeakHours.IsPercent;
            }
            if (model.NightSurcharge != null)
            {
                priceConfiguration.NightSurcharge.Time = model.NightSurcharge.Time != null ? model.NightSurcharge.Time : priceConfiguration.NightSurcharge.Time;
                priceConfiguration.NightSurcharge.Price = model.NightSurcharge.Price != null ? model.NightSurcharge.Price : priceConfiguration.NightSurcharge.Price;
                priceConfiguration.NightSurcharge.IsPercent = model.NightSurcharge.IsPercent != null ? model.NightSurcharge.IsPercent : priceConfiguration.NightSurcharge.IsPercent;
            }
            if (model.WaitingSurcharge != null)
            {
                priceConfiguration.WaitingSurcharge.PerMinutes = model.WaitingSurcharge.PerMinutes != null ? model.WaitingSurcharge.PerMinutes : priceConfiguration.WaitingSurcharge.PerMinutes;
                priceConfiguration.WaitingSurcharge.Price = model.WaitingSurcharge.Price != null ? model.WaitingSurcharge.Price : priceConfiguration.WaitingSurcharge.Price;
                priceConfiguration.WaitingSurcharge.IsPercent = model.WaitingSurcharge.IsPercent != null ? model.WaitingSurcharge.IsPercent : priceConfiguration.WaitingSurcharge.IsPercent;
            }
            if (model.WeatherFee != null)
            {
                priceConfiguration.WeatherFee.Price = model.WeatherFee.Price != null ? model.WeatherFee.Price : priceConfiguration.WeatherFee.Price;
                priceConfiguration.WeatherFee.IsPercent = model.WeatherFee.IsPercent != null ? model.WeatherFee.IsPercent : priceConfiguration.WeatherFee.IsPercent;
            }
            if (model.CustomerCancelFee != null)
            {
                priceConfiguration.CustomerCancelFee.Price = model.CustomerCancelFee.Price != null ? model.CustomerCancelFee.Price : priceConfiguration.CustomerCancelFee.Price;
                priceConfiguration.CustomerCancelFee.IsPercent = model.CustomerCancelFee.IsPercent != null ? model.CustomerCancelFee.IsPercent : priceConfiguration.CustomerCancelFee.IsPercent;
            }
            if (model.SearchRadius != null)
            {
                priceConfiguration.SearchRadius.Distance = model.SearchRadius.Distance != null ? model.SearchRadius.Distance : priceConfiguration.SearchRadius.Distance;
                priceConfiguration.SearchRadius.Unit = model.SearchRadius.Unit != null ? model.SearchRadius.Unit : priceConfiguration.SearchRadius.Unit;
            }

            priceConfiguration.DateUpdated = DateTime.Now;
            _dbContext.PriceConfigurations.Update(priceConfiguration);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = priceConfiguration;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetWithdrawFundsRequest(PagingParam<SortWithdrawFundsTransactionCriteria> paginationModel, SearchModel searchModel, WithdrawFundsRequestFilterModel filterModel, Guid adminId)
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
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var data = _dbContext.WalletTransactions
                .Include(_ => _.LinkedAccount)
                .Where(_ => _.TypeWalletTransaction == TypeWalletTransaction.WithdrawFunds);

            // Apply search filter
            if (searchModel != null && !string.IsNullOrEmpty(searchModel.SearchValue))
            {
                data = data.Where(request => request.Id.ToString().Contains(searchModel.SearchValue));
            }

            // Apply additional filters
            if (filterModel != null)
            {
                if (filterModel.Status != null && filterModel.Status.Any())
                {
                    data = data.Where(request => filterModel.Status.Contains(request.Status));
                }
            }

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

    public async Task<ResultModel> UpdateUserPriorityById(UpdateUserPriorityModel model, Guid adminId)
    {
        var result = new ResultModel();
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
                result.ErrorMessage = "Admin is deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
                return result;
            }
            var user = _dbContext.Users.Where(_ => _.Id == model.UserId && !_.IsDeleted).FirstOrDefault();
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
            user.Priority = model.Priority;
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

    public async Task<ResultModel> UpdateStaffStatusOnline(Guid staffId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var staff = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == staffId && !_.IsDeleted).FirstOrDefault();
            if (staff == null)
            {
                result.ErrorMessage = "Staff not exists";
                result.Succeed = false;
                return result;
            }
            var checkStaff = await _userManager.IsInRoleAsync(staff, RoleNormalizedName.Staff);
            if (!checkStaff)
            {
                result.ErrorMessage = "The user must be a Staff";
                result.Succeed = false;
                return result;
            }

            var staffStatus = staff.DriverStatuses.FirstOrDefault();

            staffStatus.IsOnline = true;
            staffStatus.IsFree = true;
            staffStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(staffStatus);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = staffStatus.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateStaffStatusOffline(Guid staffId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var staff = _dbContext.Users
                .Include(_ => _.DriverStatuses)
                .Where(_ => _.Id == staffId && !_.IsDeleted).FirstOrDefault();
            if (staff == null)
            {
                result.ErrorMessage = "Staff not exists";
                result.Succeed = false;
                return result;
            }
            var checkStaff = await _userManager.IsInRoleAsync(staff, RoleNormalizedName.Staff);
            if (!checkStaff)
            {
                result.ErrorMessage = "The user must be a Staff";
                result.Succeed = false;
                return result;
            }

            var staffStatus = staff.DriverStatuses.FirstOrDefault();

            staffStatus.IsOnline = false;
            staffStatus.IsFree = true;
            staffStatus.DateUpdated = DateTime.Now;
            _dbContext.DriverStatuses.Update(staffStatus);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = staffStatus.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetAdminOverview(Guid adminId)
    {
        ResultModel result = new ResultModel();
        try
        {
            var admin = await _dbContext.Users
                .Include(_ => _.DriverLocations)
                .FirstOrDefaultAsync(_ => _.Id == adminId && !_.IsDeleted);

            if (admin == null)
            {
                result.ErrorMessage = "Admin not exists";
                result.Succeed = false;
                return result;
            }

            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);

            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                result.Succeed = false;
                return result;
            }

            var totalAccounts = await _dbContext.Users
                .Include(_ => _.UserRoles)
                .ThenInclude(_ => _.Role)
                .CountAsync(u => !u.IsDeleted && u.UserRoles.Any(ur => ur.Role.Name != RoleNormalizedName.Admin));

            var totalTrips = await _dbContext.Bookings.CountAsync();
            var totalSupportRequests = await _dbContext.Supports.CountAsync();
            var totalEmergencyRequests = await _dbContext.Emergencies.CountAsync();

            var drivers = await _userManager.GetUsersInRoleAsync(RoleNormalizedName.Driver);
            var customers = await _userManager.GetUsersInRoleAsync(RoleNormalizedName.Customer);
            var staff = await _userManager.GetUsersInRoleAsync(RoleNormalizedName.Staff);

            var driverCount = drivers.Count;
            var customerCount = customers.Count;
            var staffCount = staff.Count;

            var canceledTrips = await _dbContext.Bookings.CountAsync(b => b.Status == BookingStatus.Cancel);
            var completedTrips = await _dbContext.Bookings.CountAsync(b => b.Status == BookingStatus.Complete);

            var canceledTripsRate = Math.Round(totalTrips == 0 ? 0 : ((double)canceledTrips / totalTrips * 100), 2);
            var completedTripsRate = Math.Round(totalTrips == 0 ? 0 : ((double)completedTrips / totalTrips * 100), 2);
            var orderTripsRate = Math.Round(100.00 - canceledTripsRate - completedTripsRate, 2);

            var newSupportRequests = await _dbContext.Supports.CountAsync(s => s.SupportStatus == SupportStatus.New);
            var inProcessSupportRequests = await _dbContext.Supports.CountAsync(s => s.SupportStatus == SupportStatus.InProcess);
            var solvedSupportRequests = await _dbContext.Supports.CountAsync(s => s.SupportStatus == SupportStatus.Solved);
            var pauseSupportRequests = await _dbContext.Supports.CountAsync(s => s.SupportStatus == SupportStatus.Pause);

            var pendingEmergencyRequests = await _dbContext.Emergencies.CountAsync(e => e.Status == EmergencyStatus.Pending);
            var processingEmergencyRequests = await _dbContext.Emergencies.CountAsync(e => e.Status == EmergencyStatus.Processing);
            var solvedEmergencyRequests = await _dbContext.Emergencies.CountAsync(e => e.Status == EmergencyStatus.Solved);

            AdminOverviewModel adminOverview = new AdminOverviewModel
            {
                TotalAccounts = totalAccounts,
                TotalTrips = totalTrips,
                TotalSupportRequests = totalSupportRequests,
                TotalEmergencyRequests = totalEmergencyRequests,
                AccountDetails = new List<int> { driverCount, customerCount, staffCount },
                TripStatistics = new List<double> { canceledTripsRate, completedTripsRate, orderTripsRate },
                SupportStatusDetails = new List<int> { newSupportRequests, inProcessSupportRequests, solvedSupportRequests, pauseSupportRequests },
                EmergencyStatusDetails = new List<int> { pendingEmergencyRequests, processingEmergencyRequests, solvedEmergencyRequests }
            };

            result.Succeed = true;
            result.Data = adminOverview;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            result.Succeed = false;
        }
        return result;
    }


    public async Task<ResultModel> GetAdminRevenueMonthlyIncome(Guid adminId, int year)
    {
        ResultModel result = new ResultModel();
        try
        {
            var admin = await _dbContext.Users
                .Include(_ => _.DriverLocations)
                .FirstOrDefaultAsync(_ => _.Id == adminId && !_.IsDeleted);

            if (admin == null)
            {
                result.ErrorMessage = "Admin not exists";
                result.Succeed = false;
                return result;
            }

            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);

            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                result.Succeed = false;
                return result;
            }

            var monthlyIncome = new List<long>(new long[12]);

            var bookings = await _dbContext.Bookings
                .Where(b => b.DateCreated.Year == year)
                .Include(b => b.SearchRequest)
                .ToListAsync();

            foreach (var booking in bookings)
            {
                int month = booking.DateCreated.Month - 1;
                monthlyIncome[month] += booking.SearchRequest.Price;
            }

            AdminLineChartModel adminLineChart = new AdminLineChartModel
            {
                MonthlyIncome = monthlyIncome
            };

            result.Succeed = true;
            result.Data = adminLineChart;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            result.Succeed = false;
        }
        return result;
    }


    public async Task<ResultModel> GetAdminProfitMonthlyIncome(Guid adminId, int year)
    {
        ResultModel result = new ResultModel();
        try
        {
            var admin = await _dbContext.Users
                .Include(_ => _.DriverLocations)
                .FirstOrDefaultAsync(_ => _.Id == adminId && !_.IsDeleted);

            if (admin == null)
            {
                result.ErrorMessage = "Admin not exists";
                result.Succeed = false;
                return result;
            }

            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);

            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                result.Succeed = false;
                return result;
            }

            var monthlyIncome = new List<long>(new long[12]);

            var wallet = await _dbContext.Wallets
                .FirstOrDefaultAsync(_ => _.UserId == admin.Id);

            if (wallet == null)
            {
                result.ErrorMessage = "Wallet not exists";
                result.Succeed = false;
                return result;
            }

            var walletTransactions = await _dbContext.WalletTransactions
                .Where(_ => _.WalletId == wallet.Id && _.DateCreated.Year == year)
                .ToListAsync();

            foreach (var walletTransaction in walletTransactions)
            {
                int month = walletTransaction.DateCreated.Month - 1;
                if (walletTransaction.TypeWalletTransaction == TypeWalletTransaction.Income)
                {
                    monthlyIncome[month] += walletTransaction.TotalMoney;
                }
                else
                {
                    monthlyIncome[month] -= walletTransaction.TotalMoney;
                }
            }

            AdminLineChartModel adminLineChart = new AdminLineChartModel
            {
                MonthlyIncome = monthlyIncome
            };

            result.Succeed = true;
            result.Data = adminLineChart;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
            result.Succeed = false;
        }
        return result;
    }

    public async Task<ResultModel> GetStaffList(Guid adminId)
    {
        ResultModel result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Include(_ => _.DriverLocations).Where(_ => _.Id == adminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exists";
                result.Succeed = false;
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                result.Succeed = false;
                return result;
            }
            var staffList = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Staff) && !_.IsDeleted)
                    .Select(user => new ListStaff
                    {
                        text = user.Name,
                        HandlerId = user.Id
                    })
                    .ToList();

            result.Data = staffList;
            result.Succeed = true;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }
}