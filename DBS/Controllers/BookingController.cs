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

    [HttpGet("ForAdmin")]
    public async Task<ActionResult> GetBookingsForAdmin([FromQuery] PagingParam<SortBookingCriteria> paginationModel)
    {
        var result = await _bookingService.GetBookingsForAdmin(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ForCustomer")]
    public async Task<ActionResult> GetBookingsForCustomer([FromQuery] PagingParam<SortBookingCriteria> paginationModel)
    {
        var result = await _bookingService.GetBookingForCustomer(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ForDriver")]
    public async Task<ActionResult> GetBookingsForDriver([FromQuery] PagingParam<SortBookingCriteria> paginationModel)
    {
        var result = await _bookingService.GetBookingForDriver(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ResetBooking")]
    public async Task<ActionResult> ResetBooking()
    {
        var result = await _bookingService.ResetBooking();
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ChangeStatusToArrived")]
    public async Task<ActionResult> ChangeStatusToArrived([FromBody] ChangeBookingStatusModel model)
    {
        var result = await _bookingService.ChangeStatusToArrived(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ChangeStatusToOnGoing")]
    public async Task<ActionResult> ChangeStatusToOnGoing([FromBody] ChangeBookingStatusModel model)
    {
        var result = await _bookingService.ChangeStatusToOnGoing(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("ChangeStatusToComplete")]
    public async Task<ActionResult> ChangeStatusToComplete([FromBody] ChangeBookingStatusModel model)
    {
        var result = await _bookingService.ChangeStatusToComplete(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("DriverCancelBooking")]
    public async Task<ActionResult> DriverCancelBooking([FromBody] ChangeBookingStatusModel model)
    {
        var result = await _bookingService.DriverCancelBooking(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("CustomerCancelBooking")]
    public async Task<ActionResult> CustomerCancelBooking([FromBody] ChangeBookingStatusModel model)
    {
        var result = await _bookingService.CustomerCancelBooking(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

}
