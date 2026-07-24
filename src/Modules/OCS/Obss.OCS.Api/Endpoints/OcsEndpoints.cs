using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.OCS.Application.Commands.AdjustBalance;
using Obss.OCS.Application.Commands.CreateBalance;
using Obss.OCS.Application.Commands.DebitReservation;
using Obss.OCS.Application.Commands.ReleaseReservation;
using Obss.OCS.Application.Commands.ReserveCredit;
using Obss.OCS.Application.Queries.GetBalance;
using Obss.OCS.Application.Queries.GetCreditPools;
using Obss.SharedKernel.Application.Authorization;

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
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Ocs.BalanceWrite))
          .WithName("CreateBalance")
          .WithDescription("Create a new balance for a subscription");

        group.MapGet("/balances/{subscriptionId:guid}", async (Guid subscriptionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBalanceQuery(subscriptionId));
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Ocs.BalanceRead))
          .WithName("GetBalance")
          .WithDescription("Get the current balance for a subscription");

        group.MapPost("/balances/{balanceId:guid}/adjust", async (Guid balanceId, AdjustBalanceCommand command, IMediator mediator) =>
        {
            command = command with { BalanceId = balanceId };
            var result = await mediator.Send(command);
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Ocs.BalanceAdjust))
          .WithName("AdjustBalance")
          .WithDescription("Credit or debit a balance (manual adjustment)");

        group.MapPost("/reserve", async (ReserveCreditCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Ocs.ReserveCredit))
          .WithName("ReserveCredit")
          .WithDescription("Reserve credit from a balance (with concurrency conflict retry)");

        group.MapPost("/reservations/{reservationId:guid}/debit", async (Guid reservationId, IMediator mediator) =>
        {
            var result = await mediator.Send(new DebitReservationCommand(reservationId));
            return result.IsSuccess ? (IResult)TypedResults.Ok(result) : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Ocs.ReservationDebit))
          .WithName("DebitReservation")
          .WithDescription("Convert a reservation into a usage debit");

        group.MapPost("/reservations/{reservationId:guid}/release", async (Guid reservationId, IMediator mediator) =>
        {
            var result = await mediator.Send(new ReleaseReservationCommand(reservationId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Ocs.ReservationRelease))
          .WithName("ReleaseReservation")
          .WithDescription("Release a reservation without debiting");

        group.MapGet("/credit-pools/{subscriptionId:guid}", async (Guid subscriptionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCreditPoolsQuery(subscriptionId));
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Ocs.CreditPoolRead))
          .WithName("GetCreditPools")
          .WithDescription("Get active credit pools for a subscription");
    }
}
