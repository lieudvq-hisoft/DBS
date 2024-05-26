using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Data.Utils;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Services.Utils;

namespace Services.Core;

public interface IExternalAuthService
{
    Task<ResultModel> ExternalLogin(ExternalAuthModel model);
}
public class ExternalAuthService : IExternalAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IConfigurationSection _goolgeSettings;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IProducer<Null, string> _producer;

    public ExternalAuthService(AppDbContext dbContext, IMapper mapper, IConfiguration configuration, UserManager<User> userManager,
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
        _goolgeSettings = _configuration.GetSection("GoogleAuthSettings");
    }

    public async Task<ResultModel> ExternalLogin(ExternalAuthModel externalAuth)
    {

        var result = new ResultModel();
        try
        {
            var decodedToken = await VerifyFirebaseToken(externalAuth);
            if (decodedToken == null)
            {
                result.ErrorMessage = "Invalid External Authentication";
                result.Succeed = false;
                return result;
            }
            var info = new UserLoginInfo(externalAuth.Provider, decodedToken.Subject, externalAuth.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user == null)
            {
                switch (info.LoginProvider)
                {
                    case "phone":
                        var phone = decodedToken.Claims.FirstOrDefault(x => x.Key == "phone_number").Value.ToString();
                        user = _dbContext.Users.FirstOrDefault(_ => _.PhoneNumber == phone);
                        if (user == null)
                        {
                            user = new User { UserName = phone, PhoneNumber = phone, Email = phone + "@gmail.com" };
                            await _userManager.CreateAsync(user);
                            await _userManager.AddToRoleAsync(user, RoleNormalizedName.Customer);
                            //await _userManager.AddLoginAsync(user, info);
                        }
                        else
                        {
                            //await _userManager.AddLoginAsync(user, info);
                        }
                        break;
                    default:
                        var email = decodedToken.Claims.FirstOrDefault(x => x.Key == "email").Value;
                        if (email != null)
                        {
                            user = await _userManager.FindByEmailAsync(email.ToString());
                            if (user == null)
                            {
                                user = new User { Email = email.ToString(), UserName = email.ToString() };
                                await _userManager.CreateAsync(user);
                                await _userManager.AddToRoleAsync(user, RoleNormalizedName.Customer);
                                //await _userManager.AddLoginAsync(user, info);
                            }
                            else
                            {
                                //await _userManager.AddLoginAsync(user, info);
                            }
                        }
                        break;
                }


            }
            if (!user.IsActive)
            {
                result.Succeed = false;
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var userRoles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
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
            var token = await MyFunction.GetAccessToken(user, roles, _configuration);
            result.Succeed = true;
            result.Data = token;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<FirebaseToken> VerifyFirebaseToken(ExternalAuthModel externalAuth)
    {
        try
        {
            FirebaseApp firebaseApp = FirebaseApp.DefaultInstance;
            FirebaseAuth auth = FirebaseAuth.GetAuth(firebaseApp);
            FirebaseToken decodedToken = await auth.VerifyIdTokenAsync(externalAuth.IdToken);
            return decodedToken;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
