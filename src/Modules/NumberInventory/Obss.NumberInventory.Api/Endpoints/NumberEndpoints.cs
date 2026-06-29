using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.NumberInventory.Application.Commands.AddNumber;
using Obss.NumberInventory.Application.Commands.AssignNumber;
using Obss.NumberInventory.Application.Commands.ReleaseNumber;
using Obss.NumberInventory.Application.Queries.GetAvailableNumbers;
using Obss.NumberInventory.Application.Queries.GetNumberById;
using Obss.NumberInventory.Application.Queries.SearchNumbers;

namespace Obss.NumberInventory.Api.Endpoints;

public static class NumberEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/numbers", async (AddNumberCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/number-inventory/numbers/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/numbers/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetNumberByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/numbers", async ([AsParameters] SearchNumbersQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/numbers/available", async ([AsParameters] GetAvailableNumbersQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/numbers/{id:guid}/assign", async (Guid id, AssignNumberCommand command, IMediator mediator) =>
        {
            if (id != command.NumberId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/numbers/{id:guid}/release", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ReleaseNumberCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
