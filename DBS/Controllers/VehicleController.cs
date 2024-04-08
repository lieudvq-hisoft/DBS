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
public class VehicleController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehicleController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpPost]
    public async Task<ActionResult> AddVehicle([FromBody] VehicleCreateModel model)
    {
        var result = await _vehicleService.Add(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet()]
    public async Task<ActionResult> GetAll()
    {
        var result = await _vehicleService.GetAll(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{VehicleId}")]
    public async Task<ActionResult> Get(Guid VehicleId)
    {
        var result = await _vehicleService.GetById(VehicleId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("{VehicleId}")]
    public async Task<ActionResult> UpdateIdenttiyCard([FromBody] VehicleUpdateModel model, Guid VehicleId)
    {
        var result = await _vehicleService.Update(model, VehicleId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{VehicleId}")]
    public async Task<ActionResult> DeleteVehicle(Guid VehicleId)
    {
        var result = await _vehicleService.Delete(VehicleId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("VehicleImage")]
    public async Task<ActionResult> AddImage([FromForm] VehicleImageCreateModel model)
    {
        var result = await _vehicleService.AddImage(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("VehicleImage/Download")]
    public async Task<ActionResult> DownloadImage([FromBody] FileModel model)
    {
        var result = await _vehicleService.DownloadImage(model);
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "VehicleImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("VehicleImage/{VehicleId}")]
    public async Task<ActionResult> GetImageByVehicleId(Guid VehicleId)
    {
        var result = await _vehicleService.GetImagesByVehicleId(VehicleId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("VehicleImage/{VehicleImage}")]
    public async Task<ActionResult> UpdateImage([FromForm] VehicleImageUpdateModel model, Guid VehicleImage)
    {
        var result = await _vehicleService.UpdateImage(model, VehicleImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("VehicleImage/{VehicleImage}")]
    public async Task<ActionResult> DeleteImage(Guid VehicleImage)
    {
        var result = await _vehicleService.DeleteImage(VehicleImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
