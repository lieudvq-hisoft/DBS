using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Data.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Services.Utils;

namespace Services.Core;

public interface IBrandVehicleService
{
    Task<ResultModel> AddBrandVehicle(BrandVehicleCreateModel model, Guid userId);
    Task<ResultModel> GetAllBrandVehicle();
    Task<ResultModel> GetBrandlVehicleById(Guid brandVehicleId);
    Task<ResultModel> UpdateBrandVehicle(BrandVehicleUpdateModel model, Guid userId);
    Task<ResultModel> DeleteBrandVehicle(Guid brandVehicleId, Guid userId);
}

public class BrandVehicleService : IBrandVehicleService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public BrandVehicleService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> AddBrandVehicle(BrandVehicleCreateModel model, Guid userId)
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
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var checkExist = _dbContext.BrandVehicles.Where(_ => _.BrandName == model.BrandName).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = $"Brand with Name {model.BrandName} is exist";
                return result;
            }
            var brandVehicle = _mapper.Map<BrandVehicleCreateModel, BrandVehicle>(model);
            _dbContext.BrandVehicles.Add(brandVehicle);
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BrandVehicleImage", brandVehicle.Id.ToString());
            brandVehicle.BrandImg = await MyFunction.UploadImageAsync(model.File, dirPath);
            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<BrandVehicleModel>(brandVehicle);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> DeleteBrandVehicle(Guid brandVehicleId, Guid userId)
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
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var brandVehicle = _dbContext.BrandVehicles.Where(_ => _.Id == brandVehicleId && !_.IsDeleted).FirstOrDefault();
            if (brandVehicle == null)
            {
                result.ErrorMessage = "Brand Vehicle not exist";
                return result;
            }

            _dbContext.BrandVehicles.Remove(brandVehicle);
            await _dbContext.SaveChangesAsync();

            result.Data = "Delete Brand Vehicle successful";
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetAllBrandVehicle()
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var brandVehicles = _dbContext.BrandVehicles.ToList();
            if (brandVehicles == null)
            {
                result.ErrorMessage = "Brand Vehicle not exist";
                return result;
            }

            result.Data = _mapper.Map<List<BrandVehicleModel>>(brandVehicles);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetBrandlVehicleById(Guid brandVehicleId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var brandVehicle = _dbContext.BrandVehicles.Where(_ => _.Id == brandVehicleId && !_.IsDeleted).FirstOrDefault();
            if (brandVehicle == null)
            {
                result.ErrorMessage = "Brand Vehicle not exist";
                return result;
            }

            result.Data = _mapper.Map<BrandVehicleModel>(brandVehicle);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> UpdateBrandVehicle(BrandVehicleUpdateModel model, Guid userId)
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
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Admin);
            var checkStaff = await _userManager.IsInRoleAsync(user, RoleNormalizedName.Staff);
            if (!checkAdmin && !checkStaff)
            {
                result.ErrorMessage = "Chỉ có Quản trị viên và nhân viên có quyền thực hiện";
                return result;
            }
            var brandVehicle = _dbContext.BrandVehicles.Where(_ => _.Id == model.BrandVehicleId && !_.IsDeleted).FirstOrDefault();
            if (brandVehicle == null)
            {
                result.ErrorMessage = "Brand Vehicle not exist";
                return result;
            }
            var checkExist = _dbContext.BrandVehicles.Where(_ => _.BrandName == model.BrandName).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = $"Brand with Name {model.BrandName} is exist";
                return result;
            }
            brandVehicle.BrandName = model.BrandName;
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BrandVehicleImage", brandVehicle.Id.ToString());
            brandVehicle.BrandImg = await MyFunction.UploadImageAsync(model.File, dirPath);
            brandVehicle.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<BrandVehicleModel>(brandVehicle);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }
}
