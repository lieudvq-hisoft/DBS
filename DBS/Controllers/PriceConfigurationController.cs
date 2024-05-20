using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PriceConfigurationController : ControllerBase
{
    private readonly IPriceConfigurationService _priceConfigurationService;

    public PriceConfigurationController(IPriceConfigurationService priceConfigurationService)
    {
        _priceConfigurationService = priceConfigurationService;
    }

    [HttpGet]
    public async Task<ActionResult> GetPriceConfiguration()
    {
        var result = await _priceConfigurationService.GetPriceConfiguration();
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
