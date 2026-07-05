using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Commands.CreateCatalog;
using Obss.ProductCatalog.Application.Commands.DeleteCatalog;
using Obss.ProductCatalog.Application.Commands.UpdateCatalog;
using Obss.ProductCatalog.Application.Queries.GetCatalogById;
using Obss.ProductCatalog.Application.Queries.GetCatalogs;

namespace Obss.ProductCatalog.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/catalogs", async ([AsParameters] GetCatalogsQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            httpContext.Response.Headers.Append("X-Total-Count", result.Value.TotalCount.ToString());
            httpContext.Response.Headers.Append("X-Result-Count", result.Value.Items.Count.ToString());
            return (IResult)TypedResults.Ok(result.Value.Items);
        });

        group.MapGet("/catalogs/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCatalogByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/catalogs", async (CreateCatalogCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/catalogs/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/catalogs/{id:guid}", async (Guid id, UpdateCatalogCommand command, IMediator mediator) =>
        {
            if (id != command.CatalogId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/catalogs/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteCatalogCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPatch("/catalogs/{id:guid}", async (Guid id, UpdateCatalogCommand command, IMediator mediator) =>
        {
            if (id != command.CatalogId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
