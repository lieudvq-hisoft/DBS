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
[Authorize(AuthenticationSchemes = "Bearer")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBookingCancelService _bookingCancelService;
    private readonly IBookingService _bookingService;
    private readonly IDriverService _driverService;
    private readonly IDrivingLicenseService _drivingLicenseService;
    private readonly IIdentityCardService _identityCardService;
    private readonly IVehicleService _vehicleService;

    public AdminController(IUserService userService, IBookingCancelService bookingCancelService, IBookingService bookingService, IDriverService driverService, IDrivingLicenseService drivingLicenseService, IIdentityCardService identityCardService, IVehicleService vehicleService)
    {
        _userService = userService;
        _bookingCancelService = bookingCancelService;
        _bookingService = bookingService;
        _driverService = driverService;
        _drivingLicenseService = drivingLicenseService;
        _identityCardService = identityCardService;
        _vehicleService = vehicleService;
    }

    [HttpPut("User/BanAccount")]
    public async Task<ActionResult> BanAccount([FromBody] BanAccountModel model)
    {
        var result = await _userService.BanAccount(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("User/UnBanAccount")]
    public async Task<ActionResult> UnBanAccount([FromBody] BanAccountModel model)
    {
        var result = await _userService.UnBanAccount(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("BookingCancel/All/{UserId}")]
    public async Task<ActionResult> GetByAdmin([FromQuery] PagingParam<SortCriteria> paginationModel, Guid UserId)
    {
        var result = await _bookingCancelService.GetForAdmin(paginationModel, UserId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("BookingCancel/{BookingCancelId}")]
    public async Task<ActionResult> GetByIdByAdmin(Guid BookingCancelId)
    {
        var result = await _bookingCancelService.GetByIdForAdmin(BookingCancelId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }


    [HttpGet("Booking/All")]
    public async Task<ActionResult> GetBookingsForAdmin([FromQuery] PagingParam<SortBookingCriteria> paginationModel)
    {
        var result = await _bookingService.GetBookingsForAdmin(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }


    [HttpGet("User/All")]
    public async Task<ActionResult> GetUserByAdmin(
        [FromQuery] PagingParam<UserSortByAdminCriteria> paginationModel,
        [FromQuery] SearchModel searchModel, [FromQuery] string Role)
    {
        var result = await _userService.GetUserByAdmin(paginationModel, searchModel, Guid.Parse(User.GetId()), Role);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }


    [HttpPost("Driver/Register")]
    public async Task<ActionResult> RegisterDriverByAdmin([FromForm] RegisterDriverByAdminModel model)
    {
        var result = await _driverService.RegisterDriverByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("DrivingLicense/{DriverId}")]
    public async Task<ActionResult> AddDrivingLicenseByAdmin([FromBody] DrivingLicenseCreateModel model, Guid DriverId)
    {
        var result = await _drivingLicenseService.AddByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("DrivingLicense/{DriverId}")]
    public async Task<ActionResult> GetByAdmin(Guid DriverId)
    {
        var result = await _drivingLicenseService.GetByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("DrivingLicense/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> GetByIdByAdmin(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _drivingLicenseService.GetByIDByAdmin(DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("DrivingLicense/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] DrivingLicenseUpdateModel model, Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _drivingLicenseService.UpdateByAdmin(model, DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("DrivingLicense/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> DeleteDrivingLicenseByAdmin(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _drivingLicenseService.DeleteByAdmin(DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("DrivingLicense/DrivingLicenseImage")]
    public async Task<ActionResult> AddImageByAdmin([FromForm] DrivingLicenseImageCreateModel model)
    {
        var result = await _drivingLicenseService.AddImageByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("DrivingLicense/DrivingLicenseImage/Download")]
    public async Task<ActionResult> DownloadImageByAdmin([FromBody] FileModel model)
    {
        var result = await _drivingLicenseService.DownloadImageByAdmin(model, Guid.Parse(User.GetId()));
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "DrivingLicenseImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("DrivingLicense/DrivingLicenseImage/{DrivingLicenseId}")]
    public async Task<ActionResult> GetImagesByDrivingLicenseIdByAdmin(Guid DrivingLicenseId)
    {
        var result = await _drivingLicenseService.GetImagesByDrivingLicenseIdByAdmin(DrivingLicenseId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("DrivingLicense/DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> UpdateImageByAdmin([FromForm] DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImage)
    {
        var result = await _drivingLicenseService.UpdateImageByAdmin(model, DrivingLicenseImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("DrivingLicense/DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> DeleteImageByAdmin(Guid DrivingLicenseImage)
    {
        var result = await _drivingLicenseService.DeleteImageByAdmin(DrivingLicenseImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("IdentityCard/{DriverId}")]
    public async Task<ActionResult> AddByAdmin([FromBody] IdentityCardCreateModel model, Guid DriverId)
    {
        var result = await _identityCardService.AddByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("IdentityCard/{DriverId}")]
    public async Task<ActionResult> GetIdentitCardByAdmin(Guid DriverId)
    {
        var result = await _identityCardService.GetByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("IdentityCard/{DriverId}")]
    public async Task<ActionResult> UpdateByAdmin([FromBody] IdentityCardUpdateModel model, Guid DriverId)
    {
        var result = await _identityCardService.UpdateByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("IdentityCard/{DriverId}")]
    public async Task<ActionResult> DeleteByAdmin(Guid DriverId)
    {
        var result = await _identityCardService.DeleteByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("IdentityCard/IdentityCardImage")]
    public async Task<ActionResult> AddImageByAdmin([FromForm] IdentityCardImageCreateModel model)
    {
        var result = await _identityCardService.AddImageByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("IdentityCard/Image/Download")]
    public async Task<ActionResult> DownloadIdentityCardImageByAdmin([FromBody] FileModel model)
    {
        var result = await _identityCardService.DownloadImageByAdmin(model, Guid.Parse(User.GetId()));
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "IdentityCardImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("IdentityCard/IdentityCardImage/{IdentityCardId}")]
    public async Task<ActionResult> GetImagesByIdentityCardIdByAdmin(Guid IdentityCardId)
    {
        var result = await _identityCardService.GetImagesByIdentityCardIdByAdmin(IdentityCardId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("IdentityCard/IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> UpdateImageByAdmin([FromForm] IdentityCardImageUpdateModel model, Guid IdentityCardImage)
    {
        var result = await _identityCardService.UpdateImageByAdmin(model, IdentityCardImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("IdentityCard/IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> DeleteIdentityCardImageByAdmin(Guid IdentityCardImage)
    {
        var result = await _identityCardService.DeleteImageByAdmin(IdentityCardImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }


    [HttpGet("Vehicle/{CustomerId}")]
    public async Task<ActionResult> GetAllByAdmin(Guid CustomerId)
    {
        var result = await _vehicleService.GetAllByAdmin(Guid.Parse(User.GetId()), CustomerId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
