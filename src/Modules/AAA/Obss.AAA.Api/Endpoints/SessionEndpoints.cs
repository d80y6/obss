using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.AAA.Application.Queries.GetActiveSessions;
using Obss.AAA.Application.Queries.GetSessionById;
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
