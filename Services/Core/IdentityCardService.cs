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

public interface IIdentityCardService
{
    Task<ResultModel> Add(IdentityCardCreateModel model, Guid DriverId);
    Task<ResultModel> Get(Guid IdentityCardId, Guid DriverId);
    Task<ResultModel> Update(IdentityCardUpdateModel model, Guid IdentityCardId, Guid DriverId);
    Task<ResultModel> Delete(Guid IdentityCardId, Guid DriverId);
    Task<ResultModel> AddImage(IdentityCardImageCreateModel model);
    Task<ResultModel> GetImagesByIdentityCardId(Guid IdentityCardId);
    Task<ResultModel> UpdateImage(IdentityCardImageUpdateModel model, Guid IdentityCardImageId);
    Task<ResultModel> DeleteImage(Guid IdentityCardImageId);
    Task<ResultModel> DownloadImage(FileModel model);
}

public class IdentityCardService : IIdentityCardService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public IdentityCardService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> Add(IdentityCardCreateModel model, Guid DriverId)
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
            if (model.Dob != null)
            {
                DateOnly currentDate = DateOnly.FromDateTime(DateTime.Today);
                DateOnly dob = new DateOnly(model.Dob.Year, model.Dob.Month, model.Dob.Day);

                // Calculate the age difference
                int ageDifferenceInYears = currentDate.Year - dob.Year;

                // Check if the birthday for this year has occurred yet
                if (dob > currentDate.AddYears(-ageDifferenceInYears))
                {
                    ageDifferenceInYears--;
                }
                if (ageDifferenceInYears < 18)
                {
                    result.ErrorMessage = "Driver must be at least 18 years old.";
                    result.Succeed = false;
                    return result;
                }
            }
            var identityCard = _mapper.Map<IdentityCardCreateModel, IdentityCard>(model);
            _dbContext.IdentityCards.Add(identityCard);
            driver.IdentityCardId = identityCard.Id;
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

    public async Task<ResultModel> Delete(Guid IdentityCardId, Guid DriverId)
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
            var identityCard = _dbContext.IdentityCards.Where(_ => _.Id == IdentityCardId && !_.IsDeleted).FirstOrDefault();
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

    public async Task<ResultModel> Get(Guid IdentityCardId, Guid DriverId)
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
            var identityCard = _dbContext.IdentityCards.Where(_ => _.Id == IdentityCardId && !_.IsDeleted).FirstOrDefault();
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

    public async Task<ResultModel> Update(IdentityCardUpdateModel model, Guid IdentityCardId, Guid DriverId)
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
            var identityCard = _dbContext.IdentityCards.Where(_ => _.Id == IdentityCardId && !_.IsDeleted).FirstOrDefault();
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

                // Calculate the age difference
                int ageDifferenceInYears = currentDate.Year - dob.Year;

                // Check if the birthday for this year has occurred yet
                if (dob > currentDate.AddYears(-ageDifferenceInYears))
                {
                    ageDifferenceInYears--;
                }
                if (ageDifferenceInYears < 18)
                {
                    result.ErrorMessage = "Driver must be at least 18 years old.";
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

    public async Task<ResultModel> AddImage(IdentityCardImageCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var identityCard = _dbContext.IdentityCards.Where(_ => _.Id == model.IdentityCardId && !_.IsDeleted).FirstOrDefault();
            if (identityCard == null)
            {
                result.ErrorMessage = "Identity Card not exist!";
                return result;
            }
            var identityCardImage = _mapper.Map<IdentityCardImageCreateModel, IdentityCardImage>(model);
            _dbContext.IdentityCardImages.Add(identityCardImage);
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "IdentityCardImage", identityCardImage.Id.ToString());
            identityCardImage.ImageData = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<IdentityCardImage, IdentityCardImageModel>(identityCardImage);
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
                if (identityCardImage.ImageData == null || !identityCardImage.ImageData.Contains(model.Path))
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

    public async Task<ResultModel> GetImagesByIdentityCardId(Guid IdentityCardId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var identityCardImages = _dbContext.IdentityCardImages
                .Include(_ => _.IdentityCard)
                .Where(_ => _.IdentityCardId == IdentityCardId && !_.IsDeleted)
                .ToList();
            if (identityCardImages == null || identityCardImages.Count == 0)
            {
                result.ErrorMessage = "Identity Card Image not exist!";
                return result;
            }

            result.Data = _mapper.Map<List<IdentityCardImage>>(identityCardImages);
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateImage(IdentityCardImageUpdateModel model, Guid IdentityCardImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var identityCardImage = _dbContext.IdentityCardImages.Where(_ => _.Id == IdentityCardImageId && !_.IsDeleted).FirstOrDefault();
            if (identityCardImage == null)
            {
                result.ErrorMessage = "Identity Card Image not exist!";
                return result;
            }
            if (model.File != null)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "IdentityCardImage", identityCardImage.Id.ToString());
                identityCardImage.ImageData = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            }
            identityCardImage.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<IdentityCardImage>(identityCardImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteImage(Guid IdentityCardImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
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
}
