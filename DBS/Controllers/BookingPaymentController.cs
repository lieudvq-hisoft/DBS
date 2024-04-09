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
public class BookingPaymentController : ControllerBase
{
    private readonly IBookingPaymentService _bookingPaymentService;

    public BookingPaymentController(IBookingPaymentService bookingPaymentService)
    {
        _bookingPaymentService = bookingPaymentService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateBookingPayment([FromBody] BookingPaymentCreateModel model)
    {
        var result = await _bookingPaymentService.Create(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("Confirm/{BookingPaymentId}")]
    public async Task<ActionResult> ConfirmPaid(Guid BookingPaymentId)
    {
        var result = await _bookingPaymentService.ConfirmPaid(BookingPaymentId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{BookingId}")]
    public async Task<ActionResult> GetByBookingId(Guid BookingId)
    {
        var result = await _bookingPaymentService.GetByBookingId(BookingId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
