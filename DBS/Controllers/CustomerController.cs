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
public class CustomerController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IExternalAuthService _externalAuthService;

    public CustomerController(IUserService userService, IExternalAuthService externalAuthService)
    {
        _userService = userService;
        _externalAuthService = externalAuthService;
    }

    [HttpPost("Login")]
    public async Task<ActionResult> LoginAsCustomer([FromBody] LoginModel model)
    {
        var result = await _userService.LoginAsCustomer(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("ExternalLogin")]
    public async Task<ActionResult> LoginGoogle([FromBody] ExternalAuthModel model)
    {
        var result = await _externalAuthService.ExternalLogin(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("Register")]
    public async Task<ActionResult> Register([FromBody] RegisterModel model)
    {
        var result = await _userService.RegisterCustomer(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("CheckExistUserWithPhoneNumber")]
    public async Task<ActionResult> CheckExistUserWithPhoneNumber([FromBody] CheckExistPhoneNumberModel model)
    {
        var result = await _userService.CheckExistUserWithPhoneNumber(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("CheckExistUserWithEmail")]
    public async Task<ActionResult> CheckExistUserWithEmail([FromBody] ForgotPasswordModel model)
    {
        var result = await _userService.CheckExistUserWithEmail(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    public async Task<ActionResult> GetCustomer([FromQuery] PagingParam<CustomerSortCriteria> paginationModel, [FromQuery] SearchModel searchModel)
    {
        var result = await _userService.GetCustomer(paginationModel, searchModel);
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
    [HttpGet("GetForChat/{UserId}")]
    public async Task<ActionResult> GetForChat(Guid UserId)
    {
        var result = await _userService.GetForChat(UserId);
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
    [HttpPut("UpdateCustomerPriority")]
    public async Task<ActionResult> UpdateCustomerPriorityById()
    {
        var result = await _userService.UpdateCustomerPriorityById(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("UpdateLocation")]
    public async Task<ActionResult> UpdateLocation([FromBody] LocationModel model)
    {
        var result = await _userService.UpdateLocation(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

}
