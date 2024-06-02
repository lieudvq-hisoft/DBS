using Data.Common.PaginationModel;
using Data.Enums;
using Data.Model;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using Services.Utils;

namespace DBS.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateWallet([FromBody] WalletCreateModel model)
    {
        var result = await _walletService.CreateWallet(model);
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet]
    public async Task<ActionResult> GetWallet()
    {
        var result = await _walletService.GetWallet(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("CheckExistWallet")]
    public async Task<ActionResult> CheckExistWallet()
    {
        var result = await _walletService.CheckExistWallet(Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("AddFunds")]
    public async Task<ActionResult> AddFunds([FromBody] WalletTransactionCreateModel model)
    {
        var result = await _walletService.AddFunds(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("WithdrawFunds")]
    public async Task<ActionResult> WithdrawFunds([FromBody] WalletTransactionCreateModel model, [FromQuery] Guid LinkedAccountId)
    {
        var result = await _walletService.WithdrawFunds(model, LinkedAccountId, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("AcceptWithdrawFundsRequest")]
    public async Task<ActionResult> AcceptWithdrawFundsRequest([FromBody] ResponeWithdrawFundsRequest model)
    {
        var result = await _walletService.AcceptWithdrawFundsRequest(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPut("RejectWithdrawFundsRequest")]
    public async Task<ActionResult> RejectWithdrawFundsRequest([FromBody] ResponeWithdrawFundsRequest model)
    {
        var result = await _walletService.RejectWithdrawFundsRequest(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpPost("Pay")]
    public async Task<ActionResult> Pay([FromBody] WalletTransactionCreateModel model)
    {
        var result = await _walletService.Pay(model, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }

    [HttpGet("WalletTransaction")]
    public async Task<ActionResult> GetTransactions(
        [FromQuery] PagingParam<SortWalletTransactionCriteria> paginationModel,
        [FromQuery] SearchModel searchModel,
        [FromQuery] TransactionFilterModel filterModel
        )
    {
        var result = await _walletService.GetTransactions(paginationModel, searchModel, filterModel, Guid.Parse(User.GetId()));
        if (result.Succeed) return Ok(result.Data);
        return BadRequest(result.ErrorMessage);
    }
}
