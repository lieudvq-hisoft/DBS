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
public class DrivingLicenseController : ControllerBase
{
    private readonly IDrivingLicenseService _drivingLicenseService;

    public DrivingLicenseController(IDrivingLicenseService drivingLicenseService)
    {
        _drivingLicenseService = drivingLicenseService;
    }

    [HttpPost]
    public async Task<ActionResult> AddDrivingLicense([FromBody] DrivingLicenseCreateModel model)
    {
        var result = await _drivingLicenseService.Add(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet()]
    public async Task<ActionResult> Get()
    {
        var result = await _drivingLicenseService.Get(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut()]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] DrivingLicenseUpdateModel model)
    {
        var result = await _drivingLicenseService.Update(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete()]
    public async Task<ActionResult> DeleteDrivingLicense()
    {
        var result = await _drivingLicenseService.Delete(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("DrivingLicenseImage")]
    public async Task<ActionResult> AddImage([FromForm] DrivingLicenseImageCreateModel model)
    {
        var result = await _drivingLicenseService.AddImage(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("DrivingLicenseImage/Download")]
    public async Task<ActionResult> DownloadImage([FromBody] FileModel model)
    {
        var result = await _drivingLicenseService.DownloadImage(model);
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "DrivingLicenseImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("DrivingLicenseImage/{DrivingLicenseId}")]
    public async Task<ActionResult> GetImageByDrivingLicenseId(Guid DrivingLicenseId)
    {
        var result = await _drivingLicenseService.GetImagesByDrivingLicenseId(DrivingLicenseId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> UpdateImage([FromForm] DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImage)
    {
        var result = await _drivingLicenseService.UpdateImage(model, DrivingLicenseImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> DeleteImage(Guid DrivingLicenseImage)
    {
        var result = await _drivingLicenseService.DeleteImage(DrivingLicenseImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

}
