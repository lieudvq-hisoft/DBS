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
    Task<ResultModel> Add(IdentityCardCreateModel model, Guid UserId);
    Task<ResultModel> Get(Guid UserId);
    Task<ResultModel> Update(IdentityCardUpdateModel model, Guid UserId);
    Task<ResultModel> Delete(Guid UserId);
    Task<ResultModel> AddImage(IdentityCardImageCreateModel model);
    Task<ResultModel> GetImagesByIdentityCardId(Guid IdentityCardId);
    Task<ResultModel> UpdateImage(IdentityCardImageUpdateModel model, Guid IdentityCardImageId);
    Task<ResultModel> DeleteImage(Guid IdentityCardImageId);
    Task<ResultModel> DownloadImage(FileModel model);
    Task<ResultModel> CheckExistIdentityCard(Guid UserId);
    Task<ResultModel> CheckExistIdentityCardImage(bool IsFront, Guid UserId);
    Task<ResultModel> AddByAdmin(IdentityCardCreateModel model, Guid UserId, Guid AdminId);
    Task<ResultModel> GetByAdmin(Guid UserId, Guid AdminId);
    Task<ResultModel> UpdateByAdmin(IdentityCardUpdateModel model, Guid UserId, Guid AdminId);
    Task<ResultModel> DeleteByAdmin(Guid UserId, Guid AdminId);
    Task<ResultModel> AddImageByAdmin(IdentityCardImageCreateModel model, Guid AdminId);
    Task<ResultModel> GetImagesByIdentityCardIdByAdmin(Guid IdentityCardId, Guid AdminId);
    Task<ResultModel> UpdateImageByAdmin(IdentityCardImageUpdateModel model, Guid IdentityCardImageId, Guid AdminId);
    Task<ResultModel> DeleteImageByAdmin(Guid IdentityCardImageId, Guid AdminId);
    Task<ResultModel> DownloadImageByAdmin(FileModel model, Guid AdminId);
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

    public async Task<ResultModel> Add(IdentityCardCreateModel model, Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "Driver not exist!";
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
                    result.ErrorMessage = "Driver must be at least 18 years old.";
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

    public async Task<ResultModel> Delete(Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "Driver not exist!";
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

    public async Task<ResultModel> Get(Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "Driver not exist!";
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
            result.Succeed = true;
            result.Data = _mapper.Map<IdentityCardModel>(identityCard);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> Update(IdentityCardUpdateModel model, Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "Driver not exist!";
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
                    result.ErrorMessage = "Driver must be at least 18 years old.";
                    result.Succeed = false;
                    return result;
                }
                identityCard.Dob = (DateOnly)model.Dob;
                user.Dob = model.Dob;
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

    public async Task<ResultModel> CheckExistIdentityCard(Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "Driver not exist!";
                return result;
            }
            if (!user.IsActive)
            {
                result.ErrorMessage = "User has been deactivated";
                return result;
            }
            var checkExist = _dbContext.IdentityCards.Where(_ => _.UserId == UserId && !_.IsDeleted).FirstOrDefault();
            result.Succeed = true;
            result.Data = checkExist != null ? true : false;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> CheckExistIdentityCardImage(bool IsFront, Guid UserId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var user = _dbContext.Users.Where(_ => _.Id == UserId && !_.IsDeleted).FirstOrDefault();
            if (user == null)
            {
                result.ErrorMessage = "Driver not exist!";
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
                result.ErrorMessage = "Identity Card not exist";
                return result;
            }
            if (IsFront)
            {
                var checkExistImage = _dbContext.IdentityCardImages.Where(_ => _.IdentityCardId == identityCard.Id && _.IsFront == true).FirstOrDefault();
                result.Succeed = true;
                result.Data = checkExistImage != null ? true : false;
            }
            else
            {
                var checkExistImage = _dbContext.IdentityCardImages.Where(_ => _.IdentityCardId == identityCard.Id && _.IsFront == false).FirstOrDefault();
                result.Succeed = true;
                result.Data = checkExistImage != null ? true : false;
            }
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

    public async Task<ResultModel> GetByAdmin(Guid UserId, Guid AdminId)
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

    public async Task<ResultModel> DownloadImageByAdmin(FileModel model, Guid AdminId)
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

    public async Task<ResultModel> DeleteImageByAdmin(Guid IdentityCardImageId, Guid AdminId)
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
}
