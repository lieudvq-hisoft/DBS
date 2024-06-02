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
public class EmergencyController : ControllerBase
{
    private readonly IEmergencyService _emergencyService;

    public EmergencyController(IEmergencyService emergencyService)
    {
        _emergencyService = emergencyService;
    }

    [HttpPost("Customer")]
    public async Task<ActionResult> CustomerCreateEmergency([FromBody] EmergencyCreateModel model)
    {
        var result = await _emergencyService.CustomerCreateEmergency(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("Driver")]
    public async Task<ActionResult> DriverCreateEmergency([FromBody] EmergencyCreateModel model)
    {
        var result = await _emergencyService.DriverCreateEmergency(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet]
    public async Task<ActionResult> GetEmergencies(
        [FromQuery] PagingParam<SortEmergencyCriteria> paginationModel,
        [FromQuery] SearchModel searchModel,
        [FromQuery] EmergencyFilterModel filterModel
        )
    {
        var result = await _emergencyService.GetEmergencies(paginationModel, searchModel, filterModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{EmergencyId}")]
    public async Task<ActionResult> GetEmergencyById(Guid EmergencyId)
    {
        var result = await _emergencyService.GetEmergencyById(EmergencyId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("IsHaveEmergency/{BookingId}")]
    public async Task<ActionResult> IsHaveEmergency(Guid BookingId)
    {
        var result = await _emergencyService.IsHaveEmergency(BookingId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("Processing/{EmergencyId}")]
    public async Task<ActionResult> UpdateEmergencyStatusProcessing(Guid EmergencyId)
    {
        var result = await _emergencyService.UpdateEmergencyStatusProcessing(EmergencyId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("Solved")]
    public async Task<ActionResult> UpdateEmergencyStatusSolved([FromBody] EmergencyUpdateSolveModel model)
    {
        var result = await _emergencyService.UpdateEmergencyStatusSolved(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
