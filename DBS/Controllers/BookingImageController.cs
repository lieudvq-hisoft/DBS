using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Core;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class BookingImageController : ControllerBase
{
    private readonly IBookingImageService _bookingImageService;

    public BookingImageController(IBookingImageService bookingImageService)
    {
        _bookingImageService = bookingImageService;
    }

    [HttpPost("CheckIn")]
    public async Task<ActionResult> AddImageCheckIn([FromForm] BookingImageCreateModel model)
    {
        var result = await _bookingImageService.AddImageCheckIn(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("CheckOut")]
    public async Task<ActionResult> AddImageCheckOut([FromForm] BookingImageCreateModel model)
    {
        var result = await _bookingImageService.AddImageCheckOut(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("Download")]
    public async Task<ActionResult> DownloadImage([FromBody] FileModel model)
    {
        var result = await _bookingImageService.DownloadImage(model);
        FileEModel file = (FileEModel)result.Data;
        if (result.Succeed) return File(file.Content, "application/octet-stream", "BookingImage" + file.Extension);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("CheckInImage/{BookingId}")]
    public async Task<ActionResult> GetCheckInImagesByBookingId(Guid BookingId)
    {
        var result = await _bookingImageService.GetCheckInImagesByBookingId(BookingId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("CheckOutImage/{BookingId}")]
    public async Task<ActionResult> GetCheckOutImagesByBookingId(Guid BookingId)
    {
        var result = await _bookingImageService.GetCheckOutImagesByBookingId(BookingId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("{BookingImage}")]
    public async Task<ActionResult> UpdateImage([FromForm] BookingImageUpdateModel model, Guid BookingImage)
    {
        var result = await _bookingImageService.UpdateImage(model, BookingImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpDelete("{BookingImage}")]
    public async Task<ActionResult> DeleteImage(Guid BookingImage)
    {
        var result = await _bookingImageService.DeleteImage(BookingImage);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
