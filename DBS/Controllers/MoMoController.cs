using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(AuthenticationSchemes = "Bearer")]
public class MoMoController : ControllerBase
{
    private IMoMoService _momoService;

    public MoMoController(IMoMoService momoService)
    {
        _momoService = momoService;
    }

    [HttpPost("CreatePaymentUrl")]
    public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
    {
        var response = await _momoService.CreatePaymentAsync(model);
        return Ok(response);
    }

    [HttpGet("PaymentCallBack")]
    public IActionResult PaymentCallBack()
    {
        var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
        return Ok(response);
    }
}
