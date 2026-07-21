using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.OCS.Application.Commands.AdjustBalance;
using Obss.OCS.Application.Commands.CreateBalance;
using Obss.OCS.Application.Commands.ReserveCredit;
using Obss.OCS.Application.Queries.GetBalance;
using Obss.OCS.Application.Queries.GetCreditPools;

namespace Obss.OCS.Api.Extensions;

public static class OcsEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/balances", async (CreateBalanceCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/ocs/balances/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/balances/{subscriptionId:guid}", async (Guid subscriptionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBalanceQuery(subscriptionId));
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/balances/{balanceId:guid}/adjust", async (Guid balanceId, AdjustBalanceCommand command, IMediator mediator) =>
        {
            command = command with { BalanceId = balanceId };
            var result = await mediator.Send(command);
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/reserve", async (ReserveCreditCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/credit-pools/{subscriptionId:guid}", async (Guid subscriptionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCreditPoolsQuery(subscriptionId));
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.NotFound(result.Error);
        });
    }
}
