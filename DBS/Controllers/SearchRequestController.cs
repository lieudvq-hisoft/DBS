using Confluent.Kafka;
using Data.Common.PaginationModel;
using Data.Enums;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace UserController.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class SearchRequestController : ControllerBase
{
    private readonly ISearchRequestService _searchRequestService;

    public SearchRequestController(ISearchRequestService searchRequestService)
    {
        _searchRequestService = searchRequestService;
    }

    [HttpPost]
    public async Task<ActionResult> Login([FromBody] SearchRequestCreateModel model)
    {
        var result = await _searchRequestService.Add(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("GetOfCustomer")]
    public async Task<ActionResult> GetOfCustomer([FromQuery] PagingParam<SortCriteria> paginationModel)
    {
        var result = await _searchRequestService.GetOfCustomer(paginationModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

}
