using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;


namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(AuthenticationSchemes = "Bearer")]
public class VNPayController : ControllerBase
{
    private readonly IVNPayService _vnPayService;

    public VNPayController(IVNPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    [HttpPost("CreatePaymentUrl")]
    public async Task<ActionResult> CreatePaymentUrl(PaymentInformationModel model)
    {
        var result = await _vnPayService.CreatePaymentUrl(model, HttpContext);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("PaymentCallback")]
    public IActionResult PaymentCallback()
    {
        var response = _vnPayService.PaymentExecute(Request.Query);

        return null;
    }
}
