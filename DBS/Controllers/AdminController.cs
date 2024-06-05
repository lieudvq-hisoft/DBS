using Data.Common.PaginationModel;
using Data.Enums;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("Login")]
    public async Task<ActionResult> LoginAsManager([FromBody] LoginModel model)
    {
        var result = await _adminService.LoginAsManager(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("User/BanAccount")]
    public async Task<ActionResult> BanAccount([FromBody] BanAccountModel model)
    {
        var result = await _adminService.BanAccount(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("User/UnBanAccount")]
    public async Task<ActionResult> UnBanAccount([FromBody] BanAccountModel model)
    {
        var result = await _adminService.UnBanAccount(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("BookingCancel/{BookingId}")]
    public async Task<ActionResult> GetByIdByAdmin(Guid BookingId)
    {
        var result = await _adminService.GetByIdForAdmin(BookingId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("Booking/All")]
    public async Task<ActionResult> GetBookingsForAdmin(
        [FromQuery] PagingParam<SortBookingCriteria> paginationModel,
        [FromQuery] SearchModel searchModel,
        [FromQuery] BookingFilterModel filterModel
        )
    {
        var result = await _adminService.GetBookingsForAdmin(paginationModel, searchModel, filterModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("User/All")]
    public async Task<ActionResult> GetUserByAdmin(
        [FromQuery] PagingParam<UserSortByAdminCriteria> paginationModel,
        [FromQuery] SearchModel searchModel,
        [FromQuery] AccountFilterModel filterModel)
    {
        var result = await _adminService.GetUserByAdmin(paginationModel, searchModel, filterModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("Driver/Register")]
    public async Task<ActionResult> RegisterDriverByAdmin([FromForm] RegisterDriverByAdminModel model)
    {
        var result = await _adminService.RegisterDriverByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("Staff/Register")]
    public async Task<ActionResult> RegisterStaffByAdmin([FromForm] RegisterStaffByAdminModel model)
    {
        var result = await _adminService.RegisterStaffByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("DrivingLicense/{DriverId}")]
    public async Task<ActionResult> AddDrivingLicenseByAdmin([FromBody] DrivingLicenseCreateModel model, Guid DriverId)
    {
        var result = await _adminService.AddByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("DrivingLicense/{DriverId}")]
    public async Task<ActionResult> GetByAdmin(Guid DriverId)
    {
        var result = await _adminService.GetByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("DrivingLicense/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> GetByIdByAdmin(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _adminService.GetByIDByAdmin(DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("DrivingLicense/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] DrivingLicenseUpdateModel model, Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _adminService.UpdateByAdmin(model, DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete("DrivingLicense/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> DeleteDrivingLicenseByAdmin(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _adminService.DeleteByAdmin(DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("DrivingLicense/DrivingLicenseImage")]
    public async Task<ActionResult> AddImageByAdmin([FromForm] DrivingLicenseImageCreateModel model)
    {
        var result = await _adminService.AddImageByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("DrivingLicense/DrivingLicenseImage/Download")]
    public async Task<ActionResult> DownloadImageByAdmin([FromBody] FileModel model)
    {
        var result = await _adminService.DownloadImageByAdmin(model, Guid.Parse(User.GetId()));
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "DrivingLicenseImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("DrivingLicense/DrivingLicenseImage/{DrivingLicenseId}")]
    public async Task<ActionResult> GetImagesByDrivingLicenseIdByAdmin(Guid DrivingLicenseId)
    {
        var result = await _adminService.GetImagesByDrivingLicenseIdByAdmin(DrivingLicenseId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("DrivingLicense/DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> UpdateImageByAdmin([FromForm] DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImage)
    {
        var result = await _adminService.UpdateImageByAdmin(model, DrivingLicenseImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete("DrivingLicense/DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> DeleteImageByAdmin(Guid DrivingLicenseImage)
    {
        var result = await _adminService.DeleteImageByAdmin(DrivingLicenseImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("IdentityCard/{DriverId}")]
    public async Task<ActionResult> AddByAdmin([FromBody] IdentityCardCreateModel model, Guid DriverId)
    {
        var result = await _adminService.AddByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("IdentityCard/{DriverId}")]
    public async Task<ActionResult> GetIdentitCardByAdmin(Guid DriverId)
    {
        var result = await _adminService.GetIdentitCardByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("IdentityCard/{DriverId}")]
    public async Task<ActionResult> UpdateByAdmin([FromBody] IdentityCardUpdateModel model, Guid DriverId)
    {
        var result = await _adminService.UpdateByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete("IdentityCard/{DriverId}")]
    public async Task<ActionResult> DeleteByAdmin(Guid DriverId)
    {
        var result = await _adminService.DeleteByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("IdentityCard/IdentityCardImage")]
    public async Task<ActionResult> AddImageByAdmin([FromForm] IdentityCardImageCreateModel model)
    {
        var result = await _adminService.AddImageByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("IdentityCard/Image/Download")]
    public async Task<ActionResult> DownloadIdentityCardImageByAdmin([FromBody] FileModel model)
    {
        var result = await _adminService.DownloadIdentityCardImageByAdmin(model, Guid.Parse(User.GetId()));
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "IdentityCardImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("IdentityCard/IdentityCardImage/{IdentityCardId}")]
    public async Task<ActionResult> GetImagesByIdentityCardIdByAdmin(Guid IdentityCardId)
    {
        var result = await _adminService.GetImagesByIdentityCardIdByAdmin(IdentityCardId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("IdentityCard/IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> UpdateImageByAdmin([FromForm] IdentityCardImageUpdateModel model, Guid IdentityCardImage)
    {
        var result = await _adminService.UpdateImageByAdmin(model, IdentityCardImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete("IdentityCard/IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> DeleteIdentityCardImageByAdmin(Guid IdentityCardImage)
    {
        var result = await _adminService.DeleteIdentityCardImageByAdmin(IdentityCardImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("Vehicle/{CustomerId}")]
    public async Task<ActionResult> GetAllByAdmin(Guid CustomerId)
    {
        var result = await _adminService.GetAllByAdmin(Guid.Parse(User.GetId()), CustomerId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("PriceConfiguration")]
    public async Task<ActionResult> UpdatePriceConfiguration([FromBody] PriceConfigurationUpdateModel model)
    {
        var result = await _adminService.UpdatePriceConfiguration(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("WithdrawFundsRequest")]
    public async Task<ActionResult> GetWithdrawFundsRequest(
        [FromQuery] PagingParam<SortWithdrawFundsTransactionCriteria> paginationModel,
        [FromQuery] SearchModel searchModel,
        [FromQuery] WithdrawFundsRequestFilterModel filterModel
        )
    {
        var result = await _adminService.GetWithdrawFundsRequest(paginationModel, searchModel, filterModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("UpdateUserPriorityById")]
    public async Task<ActionResult> UpdateUserPriorityById(UpdateUserPriorityModel model)
    {
        var result = await _adminService.UpdateUserPriorityById(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("UpdateStaffStatusOffline")]
    public async Task<ActionResult> UpdateStaffStatusOffline()
    {
        var result = await _adminService.UpdateStaffStatusOffline(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("UpdateStaffStatusOnline")]
    public async Task<ActionResult> UpdateStaffStatusOnline()
    {
        var result = await _adminService.UpdateStaffStatusOnline(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("Statistic/GetAdminOverview")]
    public async Task<ActionResult> GetAdminOverview()
    {
        var result = await _adminService.GetAdminOverview(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("Statistic/GetAdminRevenueMonthlyIncome/{Year}")]
    public async Task<ActionResult> GetAdminRevenueMonthlyIncome(int Year)
    {
        var result = await _adminService.GetAdminRevenueMonthlyIncome(Guid.Parse(User.GetId()), Year);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("Statistic/GetAdminProfitMonthlyIncome/{Year}")]
    public async Task<ActionResult> GetAdminProfitMonthlyIncome(int Year)
    {
        var result = await _adminService.GetAdminProfitMonthlyIncome(Guid.Parse(User.GetId()), Year);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("User/GetStaffList")]
    public async Task<ActionResult> GetStaffList()
    {
        var result = await _adminService.GetStaffList(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
