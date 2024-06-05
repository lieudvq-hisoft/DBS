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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Services.Core;

public interface IDrivingLicenseService
{
    Task<ResultModel> Add(DrivingLicenseCreateModel model, Guid DriverId);
    Task<ResultModel> Get(Guid DriverId);
    Task<ResultModel> GetByID(Guid DrivingLicenseId, Guid DriverId);
    Task<ResultModel> Update(DrivingLicenseUpdateModel model, Guid DrivingLicenseId, Guid DriverId);
    Task<ResultModel> Delete(Guid DrivingLicenseId, Guid DriverId);
    Task<ResultModel> AddImage(DrivingLicenseImageCreateModel model);
    Task<ResultModel> GetImagesByDrivingLicenseId(Guid DrivingLicenseId);
    Task<ResultModel> UpdateImage(DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImageId);
    Task<ResultModel> DeleteImage(Guid DrivingLicenseImageId);
    Task<ResultModel> DownloadImage(FileModel model);

}
public class DrivingLicenseService : IDrivingLicenseService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public DrivingLicenseService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> Add(DrivingLicenseCreateModel model, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> AddImage(DrivingLicenseImageCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> Delete(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> DeleteImage(Guid DrivingLicenseImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> DownloadImage(FileModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> Get(Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> GetByID(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> GetImagesByDrivingLicenseId(Guid DrivingLicenseId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> Update(DrivingLicenseUpdateModel model, Guid DrivingLicenseId, Guid DriverId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

    public async Task<ResultModel> UpdateImage(DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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

}
