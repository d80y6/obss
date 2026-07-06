using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;
using Obss.ServiceCatalog.Application.Commands.ServiceCategory.DeleteServiceCategory;
using Obss.ServiceCatalog.Application.Commands.ServiceCategory.UpdateServiceCategory;
using Obss.ServiceCatalog.Application.Queries.GetServiceCategories;
using Obss.ServiceCatalog.Application.Queries.GetServiceCategoryById;

namespace Obss.ServiceCatalog.Api.Endpoints;

internal static class ServiceCategoryEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/service-categories", async (CreateServiceCategoryCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Created($"/api/v1/service-catalog/service-categories/{id}", id);
        });

        group.MapGet("/service-categories", async ([AsParameters] GetServiceCategoriesQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var (items, total) = await mediator.Send(query);
            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, total);
            return Results.Ok(items);
        });

        group.MapGet("/service-categories/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceCategoryByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapPatch("/service-categories/{id:guid}", async (Guid id, UpdateServiceCategoryCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            await mediator.Send(command);
            return Results.NoContent();
        });

        group.MapDelete("/service-categories/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteServiceCategoryCommand(id));
            return Results.NoContent();
        });
    }
}
