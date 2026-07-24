using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.AAA.Application.Queries.GetActiveSessions;
using Obss.AAA.Application.Queries.GetSessionById;
using Obss.AAA.Application.Queries.GetSessions;
using Obss.AAA.Application.Queries.GetSessionsByUser;

namespace Obss.AAA.Api.Extensions;

public static class SessionEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/sessions/active", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetActiveSessionsQuery(), ct);
            return (IResult)TypedResults.Ok(result);
        });

        group.MapGet("/sessions", async (
            int page,
            int pageSize,
            string? status,
            Guid? nasId,
            string? username,
            DateTime? dateFrom,
            DateTime? dateTo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetSessionsQuery(
                Page: page > 0 ? page : 1,
                PageSize: pageSize > 0 ? pageSize : 20,
                Status: status,
                NasId: nasId,
                Username: username,
                DateFrom: dateFrom,
                DateTo: dateTo);

            var result = await mediator.Send(query, ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound();
        });

        group.MapGet("/sessions/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSessionByIdQuery(id), ct);
            return result is null ? (IResult)TypedResults.NotFound() : (IResult)TypedResults.Ok(result);
        });

        group.MapGet("/sessions/by-user/{username}", async (string username, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetSessionsByUserQuery(username), ct);
            return (IResult)TypedResults.Ok(result);
        });
    }
}
