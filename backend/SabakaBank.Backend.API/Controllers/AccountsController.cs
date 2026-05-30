using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SabakaBank.Backend.API.Extensions;
using SabakaBank.Backend.Application.Accounts.Commands.CreateAccount;
using SabakaBank.Backend.Application.Accounts.Commands.Deposit;
using SabakaBank.Backend.Application.Accounts.Commands.Transfer;
using SabakaBank.Backend.Application.Accounts.Commands.Withdraw;
using SabakaBank.Backend.Application.Accounts.Queries.GetAccount;
using SabakaBank.Backend.Application.Accounts.Queries.GetUserAccounts;
using SabakaBank.Backend.Application.Transactions.Queries.GetAccountTransactions;
using SabakaBank.Backend.Domain.Enums;

namespace SabakaBank.Backend.API.Controllers;

[Authorize]
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IResult> GetMyAccounts(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetUserAccountsQuery(userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ApiResponse.FromError(result.Error!);
    }

    [HttpGet("{id:guid}")]
    public async Task<IResult> GetAccount(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetAccountQuery(id, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ApiResponse.FromError(result.Error!);
    }

    [HttpPost]
    public async Task<IResult> CreateAccount([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new CreateAccountCommand(userId, request.Type, request.Currency), ct);
        return result.IsSuccess
            ? Results.Created($"/api/accounts/{result.Value}", new { Id = result.Value })
            : ApiResponse.FromError(result.Error!);
    }

    [HttpPost("{id:guid}/deposit")]
    public async Task<IResult> Deposit(Guid id, [FromBody] MoneyRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var error  = await _mediator.Send(new DepositCommand(id, userId, request.Amount, request.Currency), ct);
        return error is null ? Results.NoContent() : ApiResponse.FromError(error);
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IResult> Withdraw(Guid id, [FromBody] MoneyRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var error  = await _mediator.Send(new WithdrawCommand(id, userId, request.Amount, request.Currency), ct);
        return error is null ? Results.NoContent() : ApiResponse.FromError(error);
    }

    [HttpPost("{id:guid}/transfer")]
    public async Task<IResult> Transfer(Guid id, [FromBody] TransferRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var error  = await _mediator.Send(new TransferCommand(id, request.ToAccountId, userId, request.Amount, request.Currency), ct);
        return error is null ? Results.NoContent() : ApiResponse.FromError(error);
    }

    [HttpGet("{id:guid}/transactions")]
    public async Task<IResult> GetTransactions(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var result = await _mediator.Send(new GetAccountTransactionsQuery(id, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : ApiResponse.FromError(result.Error!);
    }
}

public record CreateAccountRequest(AccountType Type, Currency Currency);
public record MoneyRequest(decimal Amount, Currency Currency);
public record TransferRequest(Guid ToAccountId, decimal Amount, Currency Currency);
