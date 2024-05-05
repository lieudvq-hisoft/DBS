﻿using Data.Common.PaginationModel;
using Data.Enums;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class BookingCancelController : ControllerBase
{
    private readonly IBookingCancelService _bookingCancelService;

    public BookingCancelController(IBookingCancelService bookingCancelService)
    {
        _bookingCancelService = bookingCancelService;
    }

    [HttpPost("Customer")]
    public async Task<ActionResult> CustomerCancel([FromBody] BookingCancelCreateModel model)
    {
        var result = await _bookingCancelService.CustomerCancel(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("Driver")]
    public async Task<ActionResult> DriverCancel([FromBody] BookingCancelCreateModel model)
    {
        var result = await _bookingCancelService.DriverCancel(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] PagingParam<SortCriteria> paginationModel)
    {
        var result = await _bookingCancelService.Get(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{BookingCancelId}")]
    public async Task<ActionResult> GetById(Guid BookingCancelId)
    {
        var result = await _bookingCancelService.GetByID(BookingCancelId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("BookingCancelImage")]
    public async Task<ActionResult> AddImage([FromForm] BookingCancelImageCreateModel model)
    {
        var result = await _bookingCancelService.AddImage(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("BookingCancelImage/Download")]
    public async Task<ActionResult> DownloadImage([FromBody] FileModel model)
    {
        var result = await _bookingCancelService.DownloadImage(model);
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "BookingCancelImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("BookingCancelImage/{BookingCancelId}")]
    public async Task<ActionResult> GetImageByBookingCancelId(Guid BookingCancelId)
    {
        var result = await _bookingCancelService.GetImagesByBookingCancelId(BookingCancelId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ByAdmin/All/{UserId}")]
    public async Task<ActionResult> GetByAdmin([FromQuery] PagingParam<SortCriteria> paginationModel, Guid UserId)
    {
        var result = await _bookingCancelService.GetForAdmin(paginationModel, UserId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ByAdmin/{BookingCancelId}")]
    public async Task<ActionResult> GetByIdByAdmin(Guid BookingCancelId)
    {
        var result = await _bookingCancelService.GetByIdForAdmin(BookingCancelId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
