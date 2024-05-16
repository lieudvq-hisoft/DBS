using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;


namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VNPayController : ControllerBase
{
    private readonly IVNPayService _vnPayService;

    public VNPayController(IVNPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("CreatePaymentBookingUrl")]
    public async Task<ActionResult> CreatePaymentBookingUrl(PaymentInformationModel model)
    {
        var result = await _vnPayService.CreatePaymentBookingUrl(model, HttpContext, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("CreatePaymentUrl")]
    public async Task<ActionResult> CreatePaymentUrl(PaymentInformationModel model)
    {
        var result = await _vnPayService.CreatePaymentUrl(model, HttpContext, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("PaymentCallback")]
    public async Task<ActionResult> PaymentCallback()
    {
        var response = await _vnPayService.PaymentExecute(Request.Query);

        return Ok(response);
    }
}
