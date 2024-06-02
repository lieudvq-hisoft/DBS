using AutoMapper;
using Confluent.Kafka;
using Data.DataAccess;
using Data.Entities;
using Data.Model;
using Data.Models;
using Data.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Core;

public interface IPriceConfigurationService
{
    Task<ResultModel> GetPriceConfiguration();
    Task<ResultModel> UpdatePriceConfiguration(PriceConfigurationUpdateModel model, Guid AdminId);
}

public class PriceConfigurationService : IPriceConfigurationService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;
    private readonly IConfiguration _configuration;
    private readonly IProducer<Null, string> _producer;
    private readonly UserManager<User> _userManager;

    public PriceConfigurationService(AppDbContext dbContext, IMapper mapper, IMailService mailService, IConfiguration configuration, IProducer<Null, string> producer, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _mailService = mailService;
        _configuration = configuration;
        _producer = producer;
        _userManager = userManager;
    }

    public async Task<ResultModel> GetPriceConfiguration()
    {
        var result = new ResultModel();
        result.Succeed = false;

        try
        {
            var priceConfiguration = _dbContext.PriceConfigurations.FirstOrDefault();
            if (priceConfiguration == null)
            {
                result.ErrorMessage = "Price Configuration not exist";
                return result;
            }

            result.Data = priceConfiguration;
            result.Succeed = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }

    public async Task<ResultModel> UpdatePriceConfiguration(PriceConfigurationUpdateModel model, Guid AdminId)
    {
        var result = new ResultModel();
        result.Succeed = false;

        try
        {
            var admin = _dbContext.Users.Where(_ => _.Id == AdminId && !_.IsDeleted).FirstOrDefault();
            if (admin == null)
            {
                result.ErrorMessage = "Admin not exist!";
                return result;
            }
            var checkAdmin = await _userManager.IsInRoleAsync(admin, RoleNormalizedName.Admin);
            if (!checkAdmin)
            {
                result.ErrorMessage = "The user must be a Admin";
                return result;
            }
            if (!admin.IsActive)
            {
                result.ErrorMessage = "Admin has been deactivated";
                return result;
            }

            var priceConfiguration = _dbContext.PriceConfigurations.FirstOrDefault();
            if (priceConfiguration == null)
            {
                result.ErrorMessage = "Price Configuration not exist";
                return result;
            }
            if (model.BaseFareFirst3km != null)
            {
                priceConfiguration.BaseFareFirst3km.Price = model.BaseFareFirst3km.Price != null ? model.BaseFareFirst3km.Price : priceConfiguration.BaseFareFirst3km.Price;
                priceConfiguration.BaseFareFirst3km.IsPercent = model.BaseFareFirst3km.IsPercent != null ? model.BaseFareFirst3km.IsPercent : priceConfiguration.BaseFareFirst3km.IsPercent;
            }
            if (model.FareFerAdditionalKm != null)
            {
                priceConfiguration.FareFerAdditionalKm.Price = model.FareFerAdditionalKm.Price != null ? model.FareFerAdditionalKm.Price : priceConfiguration.FareFerAdditionalKm.Price;
                priceConfiguration.FareFerAdditionalKm.IsPercent = model.FareFerAdditionalKm.IsPercent != null ? model.FareFerAdditionalKm.IsPercent : priceConfiguration.FareFerAdditionalKm.IsPercent;
            }
            if (model.DriverProfit != null)
            {
                priceConfiguration.DriverProfit.Price = model.DriverProfit.Price != null ? model.DriverProfit.Price : priceConfiguration.DriverProfit.Price;
                priceConfiguration.DriverProfit.IsPercent = model.DriverProfit.IsPercent != null ? model.DriverProfit.IsPercent : priceConfiguration.DriverProfit.IsPercent; priceConfiguration.DriverProfit.Price = model.DriverProfit.Price != null ? model.DriverProfit.Price : priceConfiguration.DriverProfit.Price;
            }
            if (model.AppProfit != null)
            {
                priceConfiguration.AppProfit.Price = model.AppProfit.Price != null ? model.AppProfit.Price : priceConfiguration.AppProfit.Price;
                priceConfiguration.AppProfit.IsPercent = model.AppProfit.IsPercent != null ? model.AppProfit.IsPercent : priceConfiguration.AppProfit.IsPercent;
            }
            if (model.PeakHours != null)
            {
                priceConfiguration.PeakHours.Time = model.PeakHours.Time != null ? model.PeakHours.Time : priceConfiguration.PeakHours.Time;
                priceConfiguration.PeakHours.Price = model.PeakHours.Price != null ? model.PeakHours.Price : priceConfiguration.PeakHours.Price;
                priceConfiguration.PeakHours.IsPercent = model.PeakHours.IsPercent != null ? model.PeakHours.IsPercent : priceConfiguration.PeakHours.IsPercent;
            }
            if (model.NightSurcharge != null)
            {
                priceConfiguration.NightSurcharge.Time = model.NightSurcharge.Time != null ? model.NightSurcharge.Time : priceConfiguration.NightSurcharge.Time;
                priceConfiguration.NightSurcharge.Price = model.NightSurcharge.Price != null ? model.NightSurcharge.Price : priceConfiguration.NightSurcharge.Price;
                priceConfiguration.NightSurcharge.IsPercent = model.NightSurcharge.IsPercent != null ? model.NightSurcharge.IsPercent : priceConfiguration.NightSurcharge.IsPercent;
            }
            if (model.WaitingSurcharge != null)
            {
                priceConfiguration.WaitingSurcharge.PerMinutes = model.WaitingSurcharge.PerMinutes != null ? model.WaitingSurcharge.PerMinutes : priceConfiguration.WaitingSurcharge.PerMinutes;
                priceConfiguration.WaitingSurcharge.Price = model.WaitingSurcharge.Price != null ? model.WaitingSurcharge.Price : priceConfiguration.WaitingSurcharge.Price;
                priceConfiguration.WaitingSurcharge.IsPercent = model.WaitingSurcharge.IsPercent != null ? model.WaitingSurcharge.IsPercent : priceConfiguration.WaitingSurcharge.IsPercent;
            }
            if (model.WeatherFee != null)
            {
                priceConfiguration.WeatherFee.Price = model.WeatherFee.Price != null ? model.WeatherFee.Price : priceConfiguration.WeatherFee.Price;
                priceConfiguration.WeatherFee.IsPercent = model.WeatherFee.IsPercent != null ? model.WeatherFee.IsPercent : priceConfiguration.WeatherFee.IsPercent;
            }
            if (model.CustomerCancelFee != null)
            {
                priceConfiguration.CustomerCancelFee.Price = model.CustomerCancelFee.Price != null ? model.CustomerCancelFee.Price : priceConfiguration.CustomerCancelFee.Price;
                priceConfiguration.CustomerCancelFee.IsPercent = model.CustomerCancelFee.IsPercent != null ? model.CustomerCancelFee.IsPercent : priceConfiguration.CustomerCancelFee.IsPercent;
            }
            if (model.SearchRadius != null)
            {
                priceConfiguration.SearchRadius.Distance = model.SearchRadius.Distance != null ? model.SearchRadius.Distance : priceConfiguration.SearchRadius.Distance;
                priceConfiguration.SearchRadius.Unit = model.SearchRadius.Unit != null ? model.SearchRadius.Unit : priceConfiguration.SearchRadius.Unit;
            }

            priceConfiguration.DateUpdated = DateTime.Now;
            _dbContext.PriceConfigurations.Update(priceConfiguration);
            await _dbContext.SaveChangesAsync();

            result.Succeed = true;
            result.Data = priceConfiguration;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }

        return result;
    }
}
