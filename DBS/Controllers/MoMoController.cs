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

    [HttpGet("PaymentCallBack")]
    public async void PaymentCallBack()
    {
        _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
    }
}
