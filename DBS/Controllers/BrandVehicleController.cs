using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class BrandVehicleController : ControllerBase
{
    private readonly IBrandVehicleService _brandVehicleService;

    public BrandVehicleController(IBrandVehicleService brandVehicleService)
    {
        _brandVehicleService = brandVehicleService;
    }

    [HttpPost]
    public async Task<ActionResult> AddBrandVehicle([FromBody] BrandVehicleCreateModel model)
    {
        var result = await _brandVehicleService.AddBrandVehicle(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllBrandVehicle()
    {
        var result = await _brandVehicleService.GetAllBrandVehicle();
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{BrandVehicleId}")]
    public async Task<ActionResult> GetBrandlVehicleById(Guid BrandVehicleId)
    {
        var result = await _brandVehicleService.GetBrandlVehicleById(BrandVehicleId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateBrandVehicle([FromBody] BrandVehicleUpdateModel model)
    {
        var result = await _brandVehicleService.UpdateBrandVehicle(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{BrandVehicleId}")]
    public async Task<ActionResult> DeleteBrandVehicle(Guid BrandVehicleId)
    {
        var result = await _brandVehicleService.DeleteBrandVehicle(BrandVehicleId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
