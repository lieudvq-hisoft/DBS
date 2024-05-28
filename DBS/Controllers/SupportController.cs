using Data.Common.PaginationModel;
using Data.Enums;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SupportController : ControllerBase
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSupport([FromBody] SupportCreateModel model)
    {
        var result = await _supportService.Create(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("BookingIssue")]
    public async Task<ActionResult> CreateSupportBookingIssue([FromBody] SupportBookingIssueCreateModel model)
    {
        var result = await _supportService.CreateBookingIssue(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("{SupportId}")]
    public async Task<ActionResult> GetById(Guid SupportId)
    {
        var result = await _supportService.GetByID(SupportId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] PagingParam<SortSupportCriteria> paginationModel)
    {
        var result = await _supportService.GetAll(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("ChangeStatusToInProcess/{SupportId}")]
    public async Task<ActionResult> ChangeStatusToInProcess(Guid SupportId)
    {
        var result = await _supportService.ChangeStatusToInProcess(SupportId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("ChangeStatusToSolved/{SupportId}")]
    public async Task<ActionResult> ChangeStatusToSolved(Guid SupportId)
    {
        var result = await _supportService.ChangeStatusToSolved(SupportId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut("ChangeStatusToCantSolved")]
    public async Task<ActionResult> ChangeStatusToCantSolved([FromBody] UpdateCantSolveModel model)
    {
        var result = await _supportService.ChangeStatusToCantSolved(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpDelete("{SupportId}")]
    public async Task<ActionResult> Delete(Guid SupportId)
    {
        var result = await _supportService.Delete(SupportId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
