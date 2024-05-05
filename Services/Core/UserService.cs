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
    Task<ResultModel> Login(LoginModel model);
    Task<ResultModel> GetCustomer(PagingParam<CustomerSortCriteria> paginationModel, SearchModel searchModel);
    Task<ResultModel> GetUserByAdmin(PagingParam<UserSortByAdminCriteria> paginationModel, SearchModel searchModel, Guid AdminId);
    Task<ResultModel> UpdateProfile(ProfileUpdateModel model, Guid userId);
    Task<ResultModel> UploadAvatar(UpLoadAvatarModel model, Guid userId);
    Task<ResultModel> DeleteImage(Guid userId);
    Task<ResultModel> ChangePublicGender(ChangePublicGenderModel model, Guid userId);
    Task<ResultModel> GetProfile(Guid id);
    Task<ResultModel> ChangePassword(ChangePasswordModel model, Guid userId);
    Task<ResultModel> ResetPassword(ResetPasswordModel model);
    Task<ResultModel> ForgotPassword(ForgotPasswordModel model);
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

    public async Task<ResultModel> Login(LoginModel model)
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
            var token = await MyFunction.GetAccessToken(userByEmail, roles, _configuration);
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

    public async Task<ResultModel> GetUserByAdmin(PagingParam<UserSortByAdminCriteria> paginationModel, SearchModel searchModel, Guid AdminId)
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
                    .Where(_ => _.UserRoles.Any(ur => ur.Role.NormalizedName != RoleNormalizedName.Admin) && !_.IsDeleted)
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
            foreach (var item in viewData)
            {
                if (item.Avatar != null)
                {
                    string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                    string stringPath = dirPath + item.Avatar;
                    byte[] imageBytes = File.ReadAllBytes(stringPath);
                    item.Avatar = Convert.ToBase64String(imageBytes);
                }
            }
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

            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "Avatar", user.Id.ToString());
            user.Avatar = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
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
            string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
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

            if (dataView.Avatar != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + dataView.Avatar;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                dataView.Avatar = Convert.ToBase64String(imageBytes);
            }

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

            if (dataView.Avatar != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + dataView.Avatar;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                dataView.Avatar = Convert.ToBase64String(imageBytes);
            }

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
}
