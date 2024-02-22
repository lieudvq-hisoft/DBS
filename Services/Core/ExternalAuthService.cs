using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Data.Utils;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json.Linq;
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
            if(decodedToken == null)
            {
                result.ErrorMessage = "Invalid External Authentication";
                result.Succeed = false;
                return result;
            }
            var info = new UserLoginInfo(externalAuth.Provider, decodedToken.Subject, externalAuth.Provider);
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            var email = decodedToken.Claims.First(x => x.Key == "email").Value.ToString();

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new User { Email = email, UserName = email };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, RoleNormalizedName.Customer);
                    await _userManager.AddLoginAsync(user, info);
                }
                else
                {
                    await _userManager.AddLoginAsync(user, info);
                }

            }
            var userRoles = _dbContext.UserRoles.Where(ur => ur.UserId == user.Id).ToList();
            var roles = new List<string>();
            foreach (var userRole in userRoles)
            {
                var role = await _dbContext.Roles.FindAsync(userRole.RoleId);
                if (role != null) roles.Add(role.Name);
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
