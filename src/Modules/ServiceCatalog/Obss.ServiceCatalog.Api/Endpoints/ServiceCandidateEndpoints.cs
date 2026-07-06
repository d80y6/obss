using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ServiceCatalog.Application.Commands.ServiceCandidate.CreateServiceCandidate;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;
using Obss.ServiceCatalog.Application.Commands.ServiceCandidate.DeleteServiceCandidate;
using Obss.ServiceCatalog.Application.Commands.ServiceCandidate.UpdateServiceCandidate;
using Obss.ServiceCatalog.Application.Queries.GetServiceCandidateById;
using Obss.ServiceCatalog.Application.Queries.GetServiceCandidates;

namespace Obss.ServiceCatalog.Api.Endpoints;

internal static class ServiceCandidateEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/service-candidates", async (CreateServiceCandidateCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Created($"/api/v1/service-catalog/service-candidates/{id}", id);
        });

        group.MapGet("/service-candidates", async ([AsParameters] GetServiceCandidatesQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var (items, total) = await mediator.Send(query);
            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, total);
            return Results.Ok(items);
        });

        group.MapGet("/service-candidates/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceCandidateByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapPatch("/service-candidates/{id:guid}", async (Guid id, UpdateServiceCandidateCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            await mediator.Send(command);
            return Results.NoContent();
        });

        group.MapDelete("/service-candidates/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteServiceCandidateCommand(id));
            return Results.NoContent();
        });
    }
}
