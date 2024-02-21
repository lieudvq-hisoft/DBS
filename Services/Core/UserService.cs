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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Services.Core;

public interface IUserService
{
    Task<ResultModel> RegisterCustomer(RegisterModel model);
    Task<ResultModel> Login(LoginModel model);
    Task<ResultModel> GetCustomer(PagingParam<CustomerSortCriteria> paginationModel, SearchModel searchModel);
    Task<ResultModel> UpdateProfile(ProfileUpdateModel model, Guid userId);
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
            var token = await GetAccessToken(userByEmail, roles);
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
            data.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<UserModel>(data);
        }
        catch (Exception e)
        {
            result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
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
            result.Succeed = true;
            var dataView = _mapper.Map<ProfileModel>(data);
            dataView.Name = data.Name;
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

    private async Task<Token> GetAccessToken(User user, List<string> role)
    {
        List<Claim> claims = GetClaims(user, role);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
          _configuration["Jwt:Issuer"],
          claims,
          expires: DateTime.Now.AddHours(int.Parse(_configuration["Jwt:ExpireTimes"])),
          //int.Parse(_configuration["Jwt:ExpireTimes"]) * 3600
          signingCredentials: creds);

        var serializedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new Token
        {
            Access_token = serializedToken,
            TokenType = "Bearer",
            ExpiresIn = int.Parse(_configuration["Jwt:ExpireTimes"]!) * 3600,
            UserID = user.Id.ToString(),
            UserName = user.UserName!,
            PhoneNumber = user.PhoneNumber!,
            Avatar = user.Avatar!,
            Name = user.Name!,
            Roles = role
        };
    }

    private List<Claim> GetClaims(User user, List<string> roles)
    {
        IdentityOptions _options = new IdentityOptions();
        var claims = new List<Claim> {
                new Claim("UserId", user.Id.ToString()),
                new Claim("Email", user.Email),
                new Claim("UserName", user.UserName)
            };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        if (!string.IsNullOrEmpty(user.PhoneNumber)) claims.Add(new Claim("PhoneNumber", user.PhoneNumber));
        return claims;
    }
}
