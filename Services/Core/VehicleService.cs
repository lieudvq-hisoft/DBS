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

public interface IVehicleService
{
    Task<ResultModel> Add(VehicleCreateModel model, Guid customerId);
    Task<ResultModel> GetAll(Guid customerId);
    Task<ResultModel> GetAllByAdmin(Guid AdminId, Guid customerId);
    Task<ResultModel> GetById(Guid vehicleId, Guid customerId);
    Task<ResultModel> Update(VehicleUpdateModel model, Guid vehicleId, Guid customerId);
    Task<ResultModel> Delete(Guid vehicleId, Guid customerId);
    Task<ResultModel> AddImage(VehicleImageCreateModel model);
    Task<ResultModel> GetImagesByVehicleId(Guid VehicleId);
    Task<ResultModel> UpdateImage(VehicleImageUpdateModel model, Guid VehicleImageId);
    Task<ResultModel> DeleteImage(Guid VehicleImageId);
    Task<ResultModel> DownloadImage(FileModel model);
}

public class VehicleService : IVehicleService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public VehicleService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> Add(VehicleCreateModel model, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!customer.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var vehicle = _mapper.Map<VehicleCreateModel, Vehicle>(model);
            vehicle.CustomerId = customer.Id;
            _dbContext.Vehicles.Add(vehicle);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = vehicle.Id;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> AddImage(VehicleImageCreateModel model)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var vehicle = _dbContext.Vehicles.Where(_ => _.Id == model.VehicleId && !_.IsDeleted).FirstOrDefault();
            if (vehicle == null)
            {
                result.ErrorMessage = "Vehicle not exist!";
                return result;
            }
            var vehicleImage = _mapper.Map<VehicleImageCreateModel, VehicleImage>(model);
            _dbContext.VehicleImages.Add(vehicleImage);
            string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "VehicleImage", vehicleImage.Id.ToString());
            vehicleImage.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<VehicleImage, VehicleImageModel>(vehicleImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> Delete(Guid vehicleId, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!customer.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var vehicle = _dbContext.Vehicles.Where(_ => _.Id == vehicleId && !_.IsDeleted).FirstOrDefault();
            if (vehicle == null)
            {
                result.ErrorMessage = "Vehicle not exist!";
                return result;
            }
            _dbContext.Vehicles.Remove(vehicle);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Vehicle successful";
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> DeleteImage(Guid VehicleImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var vehicleImage = _dbContext.VehicleImages.Where(_ => _.Id == VehicleImageId && !_.IsDeleted).FirstOrDefault();
            if (vehicleImage == null)
            {
                result.ErrorMessage = "Vehicle Image not exist!";
                return result;
            }
            string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            MyFunction.DeleteFile(dirPathDelete + vehicleImage.ImageUrl);

            _dbContext.VehicleImages.Remove(vehicleImage);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = "Delete Vehicle Image successful";
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
            var vehicleImage = _dbContext.VehicleImages.Where(_ => _.Id == model.Id && !_.IsDeleted).FirstOrDefault();
            if (vehicleImage == null)
            {
                result.Succeed = false;
                result.ErrorMessage = "Vehicle Image not found";
            }
            else
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                if (vehicleImage.ImageUrl == null || !vehicleImage.ImageUrl.Contains(model.Path))
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

    public async Task<ResultModel> GetAll(Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!customer.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var vehicles = _dbContext.Vehicles
                .Include(_ => _.Customer)
                .Where(_ => _.CustomerId == customerId && !_.IsDeleted)
                .ToList();
            if (vehicles == null)
            {
                result.ErrorMessage = "Vehicle not exist!";
                return result;
            }
            var data = _mapper.Map<List<VehicleModel>>(vehicles);
            foreach (var item in data)
            {
                var vehicleImage = _dbContext.VehicleImages.Where(_ => _.VehicleId == item.Id && !_.IsDeleted).FirstOrDefault();
                if (vehicleImage != null)
                {
                    item.ImagePath = vehicleImage.ImageUrl;
                    string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                    string stringPath = dirPath + vehicleImage.ImageUrl;
                    byte[] imageBytes = File.ReadAllBytes(stringPath);
                    item.ImageUrl = Convert.ToBase64String(imageBytes);
                }
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

    public async Task<ResultModel> GetAllByAdmin(Guid AdminId, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!admin.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!customer.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var vehicles = _dbContext.Vehicles
                .Include(_ => _.Customer)
                .Where(_ => _.CustomerId == customerId && !_.IsDeleted)
                .ToList();
            if (vehicles == null)
            {
                result.ErrorMessage = "Vehicle not exist!";
                return result;
            }
            var data = _mapper.Map<List<VehicleModel>>(vehicles);
            foreach (var item in data)
            {
                var vehicleImage = _dbContext.VehicleImages.Where(_ => _.VehicleId == item.Id && !_.IsDeleted).FirstOrDefault();
                if (vehicleImage != null)
                {
                    item.ImagePath = vehicleImage.ImageUrl;
                    string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                    string stringPath = dirPath + vehicleImage.ImageUrl;
                    byte[] imageBytes = File.ReadAllBytes(stringPath);
                    item.ImageUrl = Convert.ToBase64String(imageBytes);
                }
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

    public async Task<ResultModel> GetById(Guid vehicleId, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!customer.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var vehicle = _dbContext.Vehicles
                .Include(_ => _.Customer)
                .Where(_ => _.Id == vehicleId && !_.IsDeleted).FirstOrDefault();
            if (vehicle == null)
            {
                result.ErrorMessage = "Vehicle not exist!";
                return result;
            }
            result.Succeed = true;
            result.Data = _mapper.Map<VehicleModel>(vehicle);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> GetImagesByVehicleId(Guid VehicleId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var vehicleImages = _dbContext.VehicleImages
                .Include(_ => _.Vehicle)
                .Where(_ => _.VehicleId == VehicleId && !_.IsDeleted)
                .ToList();
            if (vehicleImages == null || vehicleImages.Count == 0)
            {
                result.ErrorMessage = "Vehicle Image not exist!";
                return result;
            }
            var data = _mapper.Map<List<VehicleImageModel>>(vehicleImages);
            foreach (var item in data)
            {
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                string stringPath = dirPath + item.ImageUrl;
                byte[] imageBytes = File.ReadAllBytes(stringPath);
                item.ImageUrl = Convert.ToBase64String(imageBytes);
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

    public async Task<ResultModel> Update(VehicleUpdateModel model, Guid vehicleId, Guid customerId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var customer = _dbContext.Users.Where(_ => _.Id == customerId && !_.IsDeleted).FirstOrDefault();
            if (customer == null)
            {
                result.ErrorMessage = "Customer not exist!";
                return result;
            }
            var checkCustomer = await _userManager.IsInRoleAsync(customer, RoleNormalizedName.Customer);
            if (!checkCustomer)
            {
                result.ErrorMessage = "The user must be a Customer";
                return result;
            }
            if (!customer.IsActive)
            {
                result.ErrorMessage = "Customer has been deactivated";
                return result;
            }
            var vehicle = _dbContext.Vehicles
                 .Include(_ => _.Customer)
                .Where(_ => _.Id == vehicleId && !_.IsDeleted).FirstOrDefault();
            if (vehicle == null)
            {
                result.ErrorMessage = "Vehicle not exist!";
                return result;
            }
            if (model.Brand != null)
            {
                vehicle.Brand = model.Brand;
            }
            if (model.Model != null)
            {
                vehicle.Model = model.Model;
            }
            if (model.Color != null)
            {
                vehicle.Color = model.Color;
            }
            vehicle.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<VehicleModel>(vehicle);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }

    public async Task<ResultModel> UpdateImage(VehicleImageUpdateModel model, Guid VehicleImageId)
    {
        var result = new ResultModel();
        result.Succeed = false;
        try
        {
            var vehicleImage = _dbContext.VehicleImages
                .Include(_ => _.Vehicle)
                .Where(_ => _.Id == VehicleImageId && !_.IsDeleted).FirstOrDefault();
            if (vehicleImage == null)
            {
                result.ErrorMessage = "Vehicle Image not exist!";
                return result;
            }
            if (model.File != null)
            {
                string dirPathDelete = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
                MyFunction.DeleteFile(dirPathDelete + vehicleImage.ImageUrl);
                string dirPath = Path.Combine(Directory.GetCurrentDirectory(), "Storage", "VehicleImage", vehicleImage.Id.ToString());
                vehicleImage.ImageUrl = await MyFunction.UploadFileAsync(model.File, dirPath, "/app/Storage");
            }
            vehicleImage.DateUpdated = DateTime.Now;
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = _mapper.Map<VehicleImageModel>(vehicleImage);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        return result;
    }
}
