using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.AAA.Application.Commands.RegisterNas;
using Obss.AAA.Application.Commands.UpdateNasStatus;
using Obss.AAA.Application.Queries.GetAllNasDevices;
using Obss.AAA.Application.Queries.GetNasById;

namespace Obss.AAA.Api.Extensions;

public static class NasEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/nas", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllNasDevicesQuery(), ct);
            return (IResult)TypedResults.Ok(result);
        });

        group.MapGet("/nas/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetNasByIdQuery(id), ct);
            return result is null ? (IResult)TypedResults.NotFound() : (IResult)TypedResults.Ok(result);
        });

        group.MapPost("/nas", async (RegisterNasCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/aaa/nas/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPatch("/nas/{id:guid}/status", async (Guid id, UpdateNasStatusCommand command, IMediator mediator, CancellationToken ct) =>
        {
            command = command with { NasId = id };
            var result = await mediator.Send(command, ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
