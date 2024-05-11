using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ModelVehicleController : ControllerBase
{
    private readonly IModelVehicleService _modelVehicleService;

    public ModelVehicleController(IModelVehicleService modelVehicleService)
    {
        _modelVehicleService = modelVehicleService;
    }

    [HttpPost]
    public async Task<ActionResult> AddBrandVehicle([FromBody] ModelVehicleCreateModel model)
    {
        var result = await _modelVehicleService.AddModelVehicle(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("BrandVehicle/{BrandVehicleId}")]
    public async Task<ActionResult> GetAllModelVehicleOfBrand(Guid BrandVehicleId)
    {
        var result = await _modelVehicleService.GetAllModelVehicleOfBrand(BrandVehicleId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{ModelVehicleId}")]
    public async Task<ActionResult> GetBrandlVehicleById(Guid ModelVehicleId)
    {
        var result = await _modelVehicleService.GetModelVehicleById(ModelVehicleId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateBrandVehicle([FromBody] ModelVehicleUpdateModel model)
    {
        var result = await _modelVehicleService.UpdateModelVehicle(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{ModelVehicleId}")]
    public async Task<ActionResult> DeleteBrandVehicle(Guid ModelVehicleId)
    {
        var result = await _modelVehicleService.DeleteModelVehicle(ModelVehicleId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
