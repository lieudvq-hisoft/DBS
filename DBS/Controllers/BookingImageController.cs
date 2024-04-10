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

    [HttpPost()]
    public async Task<ActionResult> AddImage([FromForm] BookingImageCreateModel model)
    {
        var result = await _bookingImageService.AddImage(model);
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

    [HttpGet("{BookingId}")]
    public async Task<ActionResult> GetImageByBookingId(Guid BookingId)
    {
        var result = await _bookingImageService.GetImagesByBookingId(BookingId);
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
