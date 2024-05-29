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
using System.Data;

namespace Services.Core;

public interface IUserService
{
    Task<ResultModel> RegisterCustomer(RegisterModel model);
    Task<ResultModel> CheckExistUserWithPhoneNumber(CheckExistPhoneNumberModel model);
    Task<ResultModel> CheckExistUserWithEmail(ForgotPasswordModel model);
    Task<ResultModel> LoginAsCustomer(LoginModel model);
    Task<ResultModel> LoginAsDriver(LoginModel model);
    Task<ResultModel> LoginAsManager(LoginModel model);
    Task<ResultModel> GetCustomer(PagingParam<CustomerSortCriteria> paginationModel, SearchModel searchModel);
    Task<ResultModel> GetUserByAdmin(PagingParam<UserSortByAdminCriteria> paginationModel, SearchModel searchModel, Guid AdminId, string Role);
    Task<ResultModel> UpdateProfile(ProfileUpdateModel model, Guid userId);
    Task<ResultModel> UploadAvatar(UpLoadAvatarModel model, Guid userId);
    Task<ResultModel> DeleteImage(Guid userId);
    Task<ResultModel> ChangePublicGender(ChangePublicGenderModel model, Guid userId);
    Task<ResultModel> GetProfile(Guid id);
    Task<ResultModel> BanAccount(BanAccountModel model, Guid adminId);
    Task<ResultModel> UnBanAccount(BanAccountModel model, Guid adminId);
    Task<ResultModel> ChangePassword(ChangePasswordModel model, Guid userId);
    Task<ResultModel> ResetPassword(ResetPasswordModel model);
    Task<ResultModel> ForgotPassword(ForgotPasswordModel model);
    Task<ResultModel> RegisterStaffByAdmin(RegisterStaffByAdminModel model, Guid UserId);
    Task<ResultModel> UpdateCustomerPriority();
    Task<ResultModel> UpdateUserPriorityById(UpdateUserPriorityModel model, Guid adminId);
    Task<ResultModel> UpdateCustomerPriorityById(Guid userId);
    Task<ResultModel> UpdateStaffStatusOffline(Guid staffId);
    Task<ResultModel> UpdateStaffStatusOnline(Guid staffId);
    Task<ResultModel> UpdateLocation(LocationModel model, Guid customerId);

}
public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IProducer<Null, string> _producer;

    public UserService(AppDbContext dbContext, IMapper mapper, IConfiguration configuration, UserManager<User> userManager,
        SignInManager<User> signInManager,
        IMailService mailService, IProducer<Null, string> producer)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _configuration = configuration;
        _userManager = userManager;
        _signInManager = signInManager;
        _mailService = mailService;
        _producer = producer;
    }

    public async Task<ResultModel> LoginAsCustomer(LoginModel model)
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
            if (!roles[0].Equals("Customer"))
            {
                result.ErrorMessage = "You are not Customer";
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

    public async Task<ResultModel> LoginAsDriver(LoginModel model)
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
            if (!roles[0].Equals("Driver"))
            {
                result.ErrorMessage = "You are not Driver";
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

    public async Task<ResultModel> RegisterCustomer(RegisterModel model)
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

            var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.NormalizedName == RoleNormalizedName.Customer);
            if (role == null)
            {
                var newRole = new Role { Name = "Customer", NormalizedName = RoleNormalizedName.Customer };
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

    public async Task<ResultModel> GetCustomer(PagingParam<CustomerSortCriteria> paginationModel, SearchModel searchModel)
    {
        ResultModel result = new ResultModel();
        try
        {
            var data = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Customer) && !_.IsDeleted).AsQueryable();
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

    public async Task<ResultModel> GetUserByAdmin(PagingParam<UserSortByAdminCriteria> paginationModel, SearchModel searchModel, Guid AdminId, string Role)
    {
        ResultModel result = new ResultModel();
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
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
            var data = _dbContext.Users.Include(_ => _.UserRoles).ThenInclude(_ => _.Role)
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName != RoleNormalizedName.Admin && (Role == null || ur.Role.NormalizedName == Role)) && !_.IsDeleted)
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
                        Role = user.UserRoles.FirstOrDefault().Role.Name
                    })
                    .AsQueryable();

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

    public async Task<ResultModel> UpdateProfile(ProfileUpdateModel model, Guid userId)
    {
        ResultModel result = new ResultModel();
        try
        {
            var data = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
            if (data == null)
            {
                result.ErrorMessage = "User not exists";
                result.Succeed = false;
                return result;
            }
            if (!data.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            if (model.Address != null)
            {
                data.Address = model.Address;
            }
            if (model.PhoneNumber != null)
            {
                data.PhoneNumber = model.PhoneNumber;
            }
            if (model.Name != null)
            {
                data.Name = model.Name;
            }
            if (model.Gender != null)
            {
                data.Gender = model.Gender;
            }
            if (model.Dob != null)
            {
                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
                DateOnly dob = new DateOnly(model.Dob.Value.Year, model.Dob.Value.Month, model.Dob.Value.Day);

                // Calculate the age difference
                int ageDifferenceInYears = currentDate.Year - dob.Year;

                // Check if the birthday for this year has occurred yet
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
                data.Dob = model.Dob;
                var identityCard = _dbContext.IdentityCards.Where(_ => _.UserId == userId).FirstOrDefault();
                if (identityCard != null)
                {
                    identityCard.Dob = model.Dob;
                    identityCard.DateUpdated = DateTime.Now;
                }
            }
            data.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(_mapper.Map<UserModel>(data));
            await _producer.ProduceAsync("dbs-user-update", new Message<Null, string> { Value = json });
            _producer.Flush();

            result.Succeed = true;
            result.Data = _mapper.Map<UserModel>(data);
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }

    public async Task<ResultModel> UploadAvatar(UpLoadAvatarModel model, Guid userId)
    {
        ResultModel result = new ResultModel();
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
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

            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Avatar", user.Id.ToString());
            user.Avatar = await MyFunction.UploadImageAsync(model.File, dirPath);
            user.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<UserModel>(user);
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteImage(Guid userId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "User not exist!";
                return result;
            }
            string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            MyFunction.DeleteFile(dirPathDelete + user.Avatar);

            user.Avatar = null;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete User Avatar successful";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetProfile(Guid id)
    {
        ResultModel result = new ResultModel();
        try
        {
            var data = _dbContext.Users.Where(_ => _.Id == id && !_.IsDeleted).FirstOrDefault();
            if (data == null)
            {
                result.ErrorMessage = "User not exists";
                result.Succeed = false;
                return result;
            }
            if (!data.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "User has been deactivated";
                return result;
            }

            var dataView = _mapper.Map<ProfileModel>(data);

            dataView.Name = data.Name;
            result.Succeed = true;
            result.Data = dataView;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
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
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be Admin";
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

    public async Task<ResultModel> ChangePublicGender(ChangePublicGenderModel model, Guid userId)
    {
        ResultModel result = new ResultModel();
        try
        {
            var driver = _dbContext.Users.Where(_ => _.Id == userId && !_.IsDeleted).FirstOrDefault();
            if (driver == null)
            {
                result.ErrorMessage = "Driver not exists";
                result.Succeed = false;
                return result;
            }
            if (!driver.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var checkDriver = await _userManager.IsInRoleAsync(driver, RoleNormalizedName.Driver);
            if (!checkDriver)
            {
                result.ErrorMessage = "The user must be Driver";
                return result;
            }
            driver.IsPublicGender = model.IsPublicGender;
            driver.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            var dataView = _mapper.Map<ProfileModel>(driver);

            result.Succeed = true;
            result.Data = dataView;
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }
        return result;
    }

    public async Task<ResultModel> ChangePassword(ChangePasswordModel model, Guid userId)
    {
        var result = new ResultModel();

        try
        {
            var user = _dbContext.Users.Where(_ => _.Email == model.Email && _.Id == userId && !_.IsDeleted).FirstOrDefault();

            if (!user.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }

            var check = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!check.Succeeded)
            {
                result.ErrorMessage = check.ToString() ?? "Change password failed";
                result.Succeed = false;
                return result;
            }
            result.Succeed = check.Succeeded;
            result.Data = "Change password successful";

        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }


        return result;
    }

    public async Task<ResultModel> ResetPassword(ResetPasswordModel model)
    {
        var result = new ResultModel();

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                result.Succeed = false;
                result.ErrorMessage = "Customer not found";
                return result;
            }
            if (!user.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!resetPassResult.Succeeded)
            {
                result.Succeed = false;
                result.ErrorMessage = resetPassResult.ToString();
                return result;
            }
            result.Succeed = true;
            result.Data = "Reset password successful";
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }

        return result;
    }

    public async Task<ResultModel> ForgotPassword(ForgotPasswordModel model)
    {
        var result = new ResultModel();

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                result.Succeed = false;
                result.ErrorMessage = "User not found";
                return result;
            }
            if (!user.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //string url = $"https://digitalcapstone.hisoft.vn/resetPassword?token={token}";
            //var confirmTokenLink = $"<a href={url}>Please click for reset password</a><div></div>";

            var email = new EmailInfoModel { Subject = "Reset password", To = model.Email, Text = token };
            result.Succeed = await _mailService.SendEmail(email);
            result.Data = result.Succeed ? "Password reset email has been sent" : "Password reset email sent failed";
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }

        return result;
    }

    public async Task<ResultModel> CheckExistUserWithPhoneNumber(CheckExistPhoneNumberModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var checkExist = _dbContext.Users
                .Where(_ => _.PhoneNumber == model.PhoneNumber && !_.IsDeleted && _.IsActive).FirstOrDefault();
            if (checkExist != null)
            {
                result.Succeed = true;
                result.Data = true;
            }
            else
            {
                result.Succeed = true;
                result.Data = false;
            }
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
        }

        return result;
    }

    public async Task<ResultModel> CheckExistUserWithEmail(ForgotPasswordModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var checkExist = _dbContext.Users
                .Where(_ => _.Email == model.Email && !_.IsDeleted && _.IsActive).FirstOrDefault();
            if (checkExist != null)
            {
                result.Succeed = true;
                result.Data = true;
            }
            else
            {
                result.Succeed = true;
                result.Data = false;
            }
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
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

    public async Task<ResultModel> UpdateCustomerPriority()
    {
        var result = new ResultModel();
        try
        {
            var customers = await _dbContext.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == RoleNormalizedName.Customer)
                    && u.Priority < 2
                    && !u.IsDeleted)
                .ToListAsync();

            foreach (var customer in customers)
            {
                customer.Priority = 2;
            }
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = customers;
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

    public async Task<ResultModel> UpdateCustomerPriorityById(Guid userId)
    {
        var result = new ResultModel();
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
                user.IsActive = true;
            }
            user.Priority = 4;
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

    public async Task<ResultModel> UpdateLocation(LocationModel model, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Include(_ => _.DriverLocations).Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Driver not exists";
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

            var driverLocation = customer.DriverLocations.FirstOrDefault();

            if (driverLocation == null)
            {
                driverLocation = _mapper.Map<LocationModel, DriverLocation>(model);
                driverLocation.DriverId = customer.Id;
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
}