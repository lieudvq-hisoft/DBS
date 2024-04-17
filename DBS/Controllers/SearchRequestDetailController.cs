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

public class SearchRequestDetailController : ControllerBase
{
    private readonly ISearchRequestDetailService _searchRequestDetailService;

    public SearchRequestDetailController(ISearchRequestDetailService searchRequestDetailService)
    {
        _searchRequestDetailService = searchRequestDetailService;
    }

    [HttpPost()]
    public async Task<ActionResult> Create([FromForm] SearchRequestDetailCreateModel model)
    {
        var result = await _searchRequestDetailService.Create(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("{SearchRequestDetailId}")]
    public async Task<ActionResult> GetById(Guid SearchRequestDetailId)
    {
        var result = await _searchRequestDetailService.GetById(SearchRequestDetailId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("SearchRequest/{SearchRequestId}")]
    public async Task<ActionResult> GetBySearchRequestId(Guid SearchRequestId)
    {
        var result = await _searchRequestDetailService.GetBySearchRequestId(SearchRequestId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

}
