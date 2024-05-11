using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Data.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Services.Core;

public interface IModelVehicleService
{
    Task<ResultModel> AddModelVehicle(ModelVehicleCreateModel model, Guid userId);
    Task<ResultModel> GetAllModelVehicleOfBrand(Guid BrandVehicleId);
    Task<ResultModel> GetModelVehicleById(Guid modelVehicleId);
    Task<ResultModel> UpdateModelVehicle(ModelVehicleUpdateModel model, Guid userId);
    Task<ResultModel> DeleteModelVehicle(Guid modelVehicleId, Guid userId);
}

public class ModelVehicleService : IModelVehicleService
{

    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public ModelVehicleService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> AddModelVehicle(ModelVehicleCreateModel model, Guid userId)
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
                result.ErrorMessage = "The user must be Admin or Staff";
                return result;
            }
            var brandVehicle = _dbContext.BrandVehicles.Where(_ => _.Id == model.BrandVehicleId).FirstOrDefault();
            if (brandVehicle == null)
            {
                result.ErrorMessage = "Brand Vehicle not exist";
                return result;
            }
            var checkExist = _dbContext.ModelVehicles.Where(_ => _.ModelName == model.ModelName).FirstOrDefault();
            if (checkExist != null)
            {
                result.ErrorMessage = $"Model with Name {model.ModelName} is exist";
                return result;
            }

            var modelVehicle = _mapper.Map<ModelVehicleCreateModel, ModelVehicle>(model);
            _dbContext.ModelVehicles.Add(modelVehicle);
            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<ModelVehicleModel>(modelVehicle);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> DeleteModelVehicle(Guid modelVehicleId, Guid userId)
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
                result.ErrorMessage = "The user must be Admin or Staff";
                return result;
            }
            var modelVehicle = _dbContext.ModelVehicles.Where(_ => _.Id == modelVehicleId).FirstOrDefault();
            if (modelVehicle == null)
            {
                result.ErrorMessage = "Model Vehicle not exist";
                return result;
            }
            _dbContext.ModelVehicles.Remove(modelVehicle);
            await _dbContext.SaveChangesAsync();

            result.Data = "Delete Model Vehicle Successful";
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetAllModelVehicleOfBrand(Guid BrandVehicleId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var modelVehicles = _dbContext.ModelVehicles.Where(_ => _.BrandVehicleId == BrandVehicleId).ToList();
            if (modelVehicles == null)
            {
                result.ErrorMessage = "Model Vehicle not exist";
                return result;
            }

            result.Data = _mapper.Map<List<ModelVehicleModel>>(modelVehicles);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> GetModelVehicleById(Guid modelVehicleId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var modelVehicles = _dbContext.ModelVehicles.Where(_ => _.Id == modelVehicleId).FirstOrDefault();
            if (modelVehicles == null)
            {
                result.ErrorMessage = "Model Vehicle not exist";
                return result;
            }

            result.Data = _mapper.Map<ModelVehicleModel>(modelVehicles);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> UpdateModelVehicle(ModelVehicleUpdateModel model, Guid userId)
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
                result.ErrorMessage = "The user must be Admin or Staff";
                return result;
            }
            var modelVehicle = _dbContext.ModelVehicles.Where(_ => _.Id == model.ModelVehicleId).FirstOrDefault();
            if (modelVehicle == null)
            {
                result.ErrorMessage = "Model Vehicle not exist";
                return result;
            }
            modelVehicle.ModelName = model.ModelName;
            modelVehicle.DateUpdated = DateTime.Now;

            await _dbContext.SaveChangesAsync();

            result.Data = _mapper.Map<ModelVehicleModel>(modelVehicle);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }
}
