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
public class RatingController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    public async Task<ActionResult> AddRating([FromForm] RatingCreateModel model)
    {
        var result = await _ratingService.Add(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("ForDriver")]
    public async Task<ActionResult> GetByDriverId()
    {
        var result = await _ratingService.GetByDriverId(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{RatingId}")]
    public async Task<ActionResult> GetById(Guid RatingId)
    {
        var result = await _ratingService.GetById(RatingId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("Booking/{BookingId}")]
    public async Task<ActionResult> GetByBookingId(Guid BookingId)
    {
        var result = await _ratingService.GetByBookingId(BookingId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("{RatingId}")]
    public async Task<ActionResult> UpdateRating([FromForm] RatingUpdateModel model, Guid RatingId)
    {
        var result = await _ratingService.UpdateRating(model, RatingId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
