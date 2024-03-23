using Data.Entities;
using Data.Models;
using DBS.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class IdentityCardController : ControllerBase
{
    private readonly IIdentityCardService _identityCardService;

    public IdentityCardController(IIdentityCardService identityCardService)
    {
        _identityCardService = identityCardService;
    }

    [HttpPost]
    public async Task<ActionResult> AddIdentityCard([FromBody] IdentityCardCreateModel model)
    {
        var result = await _identityCardService.Add(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("/{IdentityCardId}")]
    public async Task<ActionResult> Get(Guid IdentityCardId)
    {
        var result = await _identityCardService.Get(IdentityCardId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("/{IdentityCardId}")]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] IdentityCardUpdateModel model, Guid IdentityCardId)
    {
        var result = await _identityCardService.Update(model, IdentityCardId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("/IdentityCardId")]
    public async Task<ActionResult> DeleteIdentityCard(Guid IdentityCardId)
    {
        var result = await _identityCardService.Delete(IdentityCardId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("IdentityCardImage")]
    public async Task<ActionResult> AddImage([FromForm] IdentityCardImageCreateModel model)
    {
        var result = await _identityCardService.AddImage(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("IdentityCardImage")]
    public async Task<ActionResult> GetImageByIdentityCardId(Guid IdentityCardId)
    {
        var result = await _identityCardService.GetImagesByIdentityCardId(IdentityCardId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> GetImageByIdentityCardId(IdentityCardImageUpdateModel model, Guid IdentityCardImage)
    {
        var result = await _identityCardService.UpdateImage(model, IdentityCardImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> DeleteImageByIdentityCardId(Guid IdentityCardImage)
    {
        var result = await _identityCardService.DeleteImage(IdentityCardImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

}
