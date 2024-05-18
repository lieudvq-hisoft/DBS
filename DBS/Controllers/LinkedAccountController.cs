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
public class LinkedAccountController : ControllerBase
{
    public readonly ILinkedAccountService _linkedAccountService;

    public LinkedAccountController(ILinkedAccountService linkedAccountService)
    {
        _linkedAccountService = linkedAccountService;
    }

    [HttpPost]
    public async Task<ActionResult> AddLinkedAccount([FromBody] LinkedAccountCreateModel model)
    {
        var result = await _linkedAccountService.AddLinkedAccount(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("All")]
    public async Task<ActionResult> GetAllLinkedAccount()
    {
        var result = await _linkedAccountService.GetAllLinkedAccount(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{LinkedAccountId}")]
    public async Task<ActionResult> GetLinkedAccountById(Guid LinkedAccountId)
    {
        var result = await _linkedAccountService.GetLinkedAccountById(LinkedAccountId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{LinkedAccountId}")]
    public async Task<ActionResult> DeleteLinkedAccount(Guid LinkedAccountId)
    {
        var result = await _linkedAccountService.DeleteLinkedAccount(LinkedAccountId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
