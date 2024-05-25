using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MoMoController : ControllerBase
{
    private IMoMoService _momoService;

    public MoMoController(IMoMoService momoService)
    {
        _momoService = momoService;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("CreatePaymentUrl")]
    public async Task<ActionResult> CreatePaymentUrl(OrderInfoModel model)
    {
        var response = await _momoService.CreatePaymentAsync(model, Guid.Parse(User.GetId()));
        return Ok(response);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("CreatePaymentAddFundsBookingUrl")]
    public async Task<ActionResult> CreatePaymentAddFundsBookingAsync(OrderInfoModel model)
    {
        var response = await _momoService.CreatePaymentAddFundsBookingAsync(model, Guid.Parse(User.GetId()));
        return Ok(response);
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("CreatePaymentBookingUrl")]
    public async Task<ActionResult> CreatePaymentBookingAsync(OrderInfoBookingModel model)
    {
        var response = await _momoService.CreatePaymentBookingAsync(model, Guid.Parse(User.GetId()));
        return Ok(response);
    }

    [HttpGet("PaymentCallBack")]
    public async Task<ActionResult> PaymentCallBack()
    {
        var response = await _momoService.PaymentExecuteAsync(HttpContext.Request.Query);

        return Redirect(response);
    }
}
