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

    [HttpGet("{DrivingLicenseId}")]
    public async Task<ActionResult> GetById(Guid DrivingLicenseId)
    {
        var result = await _drivingLicenseService.GetByID(DrivingLicenseId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("{DrivingLicenseId}")]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] DrivingLicenseUpdateModel model, Guid DrivingLicenseId)
    {
        var result = await _drivingLicenseService.Update(model, DrivingLicenseId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{DrivingLicenseId}")]
    public async Task<ActionResult> DeleteDrivingLicense(Guid DrivingLicenseId)
    {
        var result = await _drivingLicenseService.Delete(DrivingLicenseId, Guid.Parse(User.GetId()));
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


    [HttpPost("ByAdmin/{DriverId}")]
    public async Task<ActionResult> AddDrivingLicenseByAdmin([FromBody] DrivingLicenseCreateModel model, Guid DriverId)
    {
        var result = await _drivingLicenseService.AddByAdmin(model, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ByAdmin/{DriverId}")]
    public async Task<ActionResult> GetByAdmin(Guid DriverId)
    {
        var result = await _drivingLicenseService.GetByAdmin(DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ByAdmin/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> GetByIdByAdmin(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _drivingLicenseService.GetByIDByAdmin(DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ByAdmin/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] DrivingLicenseUpdateModel model, Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _drivingLicenseService.UpdateByAdmin(model, DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("ByAdmin/{DriverId}/{DrivingLicenseId}")]
    public async Task<ActionResult> DeleteDrivingLicenseByAdmin(Guid DrivingLicenseId, Guid DriverId)
    {
        var result = await _drivingLicenseService.DeleteByAdmin(DrivingLicenseId, DriverId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("ByAdmin/DrivingLicenseImage")]
    public async Task<ActionResult> AddImageByAdmin([FromForm] DrivingLicenseImageCreateModel model)
    {
        var result = await _drivingLicenseService.AddImageByAdmin(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("ByAdmin/DrivingLicenseImage/Download")]
    public async Task<ActionResult> DownloadImageByAdmin([FromBody] FileModel model)
    {
        var result = await _drivingLicenseService.DownloadImageByAdmin(model, Guid.Parse(User.GetId()));
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "DrivingLicenseImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ByAdmin/DrivingLicenseImage/{DrivingLicenseId}")]
    public async Task<ActionResult> GetImagesByDrivingLicenseIdByAdmin(Guid DrivingLicenseId)
    {
        var result = await _drivingLicenseService.GetImagesByDrivingLicenseIdByAdmin(DrivingLicenseId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ByAdmin/DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> UpdateImageByAdmin([FromForm] DrivingLicenseImageUpdateModel model, Guid DrivingLicenseImage)
    {
        var result = await _drivingLicenseService.UpdateImageByAdmin(model, DrivingLicenseImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("ByAdmin/DrivingLicenseImage/{DrivingLicenseImage}")]
    public async Task<ActionResult> DeleteImageByAdmin(Guid DrivingLicenseImage)
    {
        var result = await _drivingLicenseService.DeleteImageByAdmin(DrivingLicenseImage, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
