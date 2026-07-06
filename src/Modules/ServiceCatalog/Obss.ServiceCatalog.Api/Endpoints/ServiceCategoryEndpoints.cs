using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ServiceCatalog.Application.Commands.ServiceCategory.CreateServiceCategory;
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

        group.MapGet("/service-categories", async (string? status, int page, int pageSize, HttpContext context, IMediator mediator) =>
        {
            var tenantId = context.RequestServices.GetRequiredService<Obss.SharedKernel.Application.Abstractions.ICurrentTenant>().TenantId!;
            var query = new GetServiceCategoriesQuery(tenantId, null, status, page, pageSize);
            var (items, total) = await mediator.Send(query);
            context.Response.Headers.Append("X-Total-Count", total.ToString());
            context.Response.Headers.Append("X-Result-Count", items.Count.ToString());
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
