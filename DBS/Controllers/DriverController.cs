using Confluent.Kafka;
using Data.Common.PaginationModel;
using Data.Enums;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace UserController.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DriverController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IExternalAuthService _externalAuthService;
    private readonly IDriverService _driverService;

    public DriverController(IUserService userService, IExternalAuthService externalAuthService, IDriverService driverService)
    {
        _userService = userService;
        _externalAuthService = externalAuthService;
        _driverService = driverService;
    }

    [HttpPost("Login")]
    public async Task<ActionResult> LoginAsDriver([FromBody] LoginModel model)
    {
        var result = await _userService.LoginAsDriver(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("Register")]
    public async Task<ActionResult> Register([FromBody] RegisterModel model)
    {
        var result = await _driverService.RegisterDriver(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    public async Task<ActionResult> GetDriver([FromQuery] PagingParam<DriverSortCriteria> paginationModel, [FromQuery] SearchModel searchModel)
    {
        var result = await _driverService.GetDriver(paginationModel, searchModel);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("Location")]
    public async Task<ActionResult> UpdateLocation([FromBody] LocationModel model)
    {
        var result = await _driverService.UpdateLocation(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("TrackingDriverLocation")]
    public async Task<ActionResult> TrackingDriverLocation([FromBody] TrackingDriverLocationModel model)
    {
        var result = await _driverService.TrackingDriverLocation(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("Status/Online")]
    public async Task<ActionResult> UpdateStatusOnline()
    {
        var result = await _driverService.UpdateStatusOnline(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("Status/Offline")]
    public async Task<ActionResult> UpdateStatusOffline()
    {
        var result = await _driverService.UpdateStatusOffline(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("UpdateAllDriverStatusOffline")]
    public async Task<ActionResult> UpdateAllDriverStatusOffline()
    {
        var result = await _driverService.UpdateAllDriverStatusOffline();
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("Online")]
    public async Task<ActionResult> GetDriverOnline([FromQuery] LocationCustomer locationCustomer)
    {
        var result = await _driverService.GetDriverOnline(locationCustomer, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("Profile")]
    public async Task<ActionResult> GetProfile()
    {
        var result = await _userService.GetProfile(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("Profile")]
    public async Task<ActionResult> UpdateProfile([FromBody] ProfileUpdateModel model)
    {
        var result = await _userService.UpdateProfile(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("Avatar")]
    public async Task<ActionResult> UploadAvatar([FromForm] UpLoadAvatarModel model)
    {
        var result = await _userService.UploadAvatar(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete("Avatar")]
    public async Task<ActionResult> DeleteImage()
    {
        var result = await _userService.DeleteImage(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("ChangePassword")]
    public async Task<ActionResult> ChangePassword(ChangePasswordModel model)
    {
        var result = await _userService.ChangePassword(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("ForgotPassword")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        var result = await _userService.ForgotPassword(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("ResetPassword")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        var result = await _userService.ResetPassword(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("ChangePublicGender")]
    public async Task<ActionResult> ChangePublicGender([FromBody] ChangePublicGenderModel model)
    {
        var result = await _userService.ChangePublicGender(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }


    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("DriverStatistics/{Year}")]
    public async Task<ActionResult> ChangePublicGender(int Year)
    {
        var result = await _driverService.GetDriverStatistics(Guid.Parse(User.GetId()), Year);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("DriverMonthlyStatistics/{Month}/{Year}")]
    public async Task<ActionResult> GetDriverMonthlyStatistics(int Month, int Year)
    {
        var result = await _driverService.GetDriverMonthlyStatistics(Guid.Parse(User.GetId()), Month, Year);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
