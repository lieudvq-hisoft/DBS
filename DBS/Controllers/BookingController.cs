using Data.Common.PaginationModel;
using Data.Entities;
using Data.Enums;
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
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateBooking([FromBody] BookingCreateModel model)
    {
        var result = await _bookingService.CreateBooking(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{BookingId}")]
    public async Task<ActionResult> GetBookingById(Guid BookingId)
    {
        var result = await _bookingService.GetBooking(BookingId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ForCustomer")]
    public async Task<ActionResult> GetBookingsForCustomer([FromQuery] PagingParam<SortCriteria> paginationModel)
    {
        var result = await _bookingService.GetBookingForCustomer(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ForDriver")]
    public async Task<ActionResult> GetBookingsForDriver([FromQuery] PagingParam<SortCriteria> paginationModel)
    {
        var result = await _bookingService.GetBookingForDriver(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ChangeStatus")]
    public async Task<ActionResult> ChangeBookingStatus([FromBody] ChangeBookingStatusModel model)
    {
        var result = await _bookingService.ChangeBookingStatus(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

}
