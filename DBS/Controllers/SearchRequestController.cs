using Data.Common.PaginationModel;
using Data.Enums;
using Data.Model;
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

    [HttpPut("UpdateStatusToComplete/{SearchRequestId}")]
    public async Task<ActionResult> UpdateStatusToComplete(string SearchRequestId)
    {
        var result = await _searchRequestService.UpdateStatusToComplete(Guid.Parse(SearchRequestId), Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("UpdateStatusToCancel")]
    public async Task<ActionResult> UpdateStatusToCancelWithoutDriverNoti([FromQuery] Guid? SearchRequestId = null)
    {
        var result = await _searchRequestService.UpdateStatusToCancel(SearchRequestId.HasValue ? SearchRequestId.Value : Guid.Empty, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("UpdateStatusToCancel/{SearchRequestId}")]
    public async Task<ActionResult> UpdateStatusToCancel(string SearchRequestId, [FromQuery] Guid? DriverId = null)
    {
        var result = await _searchRequestService.UpdateStatusToCancel(Guid.Parse(SearchRequestId), Guid.Parse(User.GetId()), DriverId.HasValue ? DriverId.Value : Guid.Empty);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }


    [HttpPut("SearchRequestDriverMiss")]
    public async Task<ActionResult> DriverMissSearchRequest([FromQuery] Guid? CustomerId = null)
    {
        var result = await _searchRequestService.DriverMissSearchRequest(CustomerId.HasValue ? CustomerId.Value : Guid.Empty, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("NewDriver")]
    public async Task<ActionResult> NewDriver([FromBody] NewDriverModel model)
    {
        var result = await _searchRequestService.NewDriver(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("CheckExistSearchRequestProcessing")]
    public async Task<ActionResult> CheckExistSearchRequestProcessing()
    {
        var result = await _searchRequestService.CheckExistSearchRequestProcessing(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("SendSearchRequestToDriver/{SearchRequestId}/{DriverId}")]
    public async Task<ActionResult> SendSearchRequestToDriver(Guid SearchRequestId, Guid DriverId)
    {
        var result = await _searchRequestService.SendSearchRequestToDriver(SearchRequestId, DriverId);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
