using Data.Entities;
using Data.Models;
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

    [HttpGet()]
    public async Task<ActionResult> Get()
    {
        var result = await _identityCardService.Get(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut()]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] IdentityCardUpdateModel model)
    {
        var result = await _identityCardService.Update(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete()]
    public async Task<ActionResult> DeleteIdentityCard()
    {
        var result = await _identityCardService.Delete(Guid.Parse(User.GetId()));
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

    [HttpPost("Image/Download")]
    public async Task<ActionResult> DownloadImage([FromBody] FileModel model)
    {
        var result = await _identityCardService.DownloadImage(model);
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "IdentityCardImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("IdentityCardImage/{IdentityCardId}")]
    public async Task<ActionResult> GetImageByIdentityCardId(Guid IdentityCardId)
    {
        var result = await _identityCardService.GetImagesByIdentityCardId(IdentityCardId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> UpdateImage([FromForm] IdentityCardImageUpdateModel model, Guid IdentityCardImage)
    {
        var result = await _identityCardService.UpdateImage(model, IdentityCardImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("IdentityCardImage/{IdentityCardImage}")]
    public async Task<ActionResult> DeleteImage(Guid IdentityCardImage)
    {
        var result = await _identityCardService.DeleteImage(IdentityCardImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("CheckExist")]
    public async Task<ActionResult> CheckExistIdentityCard()
    {
        var result = await _identityCardService.CheckExistIdentityCard(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("ByAdmin/{DriverId}")]
    public async Task<ActionResult> AddByAdmin([FromBody] IdentityCardCreateModel model, Guid DriverId)
    {
        var result = await _identityCardService.AddByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ByAdmin/{DriverId}")]
    public async Task<ActionResult> GetByAdmin(Guid DriverId)
    {
        var result = await _identityCardService.GetByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ByAdmin/{DriverId}")]
    public async Task<ActionResult> UpdateByAdmin([FromBody] IdentityCardUpdateModel model, Guid DriverId)
    {
        var result = await _identityCardService.UpdateByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("ByAdmin/{DriverId}")]
    public async Task<ActionResult> DeleteByAdmin(Guid DriverId)
    {
        var result = await _identityCardService.DeleteByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("IdentityCardImage/ByAdmin")]
    public async Task<ActionResult> AddImageByAdmin([FromForm] IdentityCardImageCreateModel model)
    {
        var result = await _identityCardService.AddImageByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("ByAdmin/Image/Download")]
    public async Task<ActionResult> DownloadImageByAdmin([FromBody] FileModel model)
    {
        var result = await _identityCardService.DownloadImageByAdmin(model, Guid.Parse(User.GetId()));
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "IdentityCardImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("IdentityCardImage/ByAdmin/{IdentityCardId}")]
    public async Task<ActionResult> GetImagesByIdentityCardIdByAdmin(Guid IdentityCardId)
    {
        var result = await _identityCardService.GetImagesByIdentityCardIdByAdmin(IdentityCardId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("IdentityCardImage/ByAdmin/{IdentityCardImage}")]
    public async Task<ActionResult> UpdateImageByAdmin([FromForm] IdentityCardImageUpdateModel model, Guid IdentityCardImage)
    {
        var result = await _identityCardService.UpdateImageByAdmin(model, IdentityCardImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("IdentityCardImage/ByAdmin/{IdentityCardImage}")]
    public async Task<ActionResult> DeleteImageByAdmin(Guid IdentityCardImage)
    {
        var result = await _identityCardService.DeleteImageByAdmin(IdentityCardImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
