using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddCharacteristic;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddCharacteristicValue;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.AddSpecRelationship;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.CreateServiceSpecification;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.DeleteServiceSpecification;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveCharacteristic;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveCharacteristicValue;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.RemoveSpecRelationship;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateCharacteristic;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateCharacteristicValue;
using Obss.ServiceCatalog.Application.Commands.ServiceSpecification.UpdateServiceSpecification;
using Obss.ServiceCatalog.Application.Queries.GetCharacteristicValues;
using Obss.ServiceCatalog.Application.Queries.GetCharacteristics;
using Obss.ServiceCatalog.Application.Queries.GetServiceSpecificationById;
using Obss.ServiceCatalog.Application.Queries.GetServiceSpecifications;
using Obss.ServiceCatalog.Application.Queries.GetSpecRelationships;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceCatalog.Api.Endpoints;

internal static class ServiceSpecificationEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        // CRUD
        group.MapPost("/service-specifications", async (CreateServiceSpecificationCommand command, IMediator mediator) =>
        {
            var id = await mediator.Send(command);
            return Results.Created($"/api/v1/service-catalog/service-specifications/{id}", id);
        });

        group.MapGet("/service-specifications", async (string? status, string? brand, int page, int pageSize, HttpContext context, IMediator mediator) =>
        {
            var tenantId = context.RequestServices.GetRequiredService<ICurrentTenant>().TenantId!;
            var query = new GetServiceSpecificationsQuery(tenantId, status, brand, page, pageSize);
            var (items, total) = await mediator.Send(query);
            context.Response.Headers.Append("X-Total-Count", total.ToString());
            context.Response.Headers.Append("X-Result-Count", items.Count.ToString());
            return Results.Ok(items);
        });

        group.MapGet("/service-specifications/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceSpecificationByIdQuery(id));
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapPatch("/service-specifications/{id:guid}", async (Guid id, UpdateServiceSpecificationCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("Id mismatch");
            await mediator.Send(command);
            return Results.NoContent();
        });

        group.MapDelete("/service-specifications/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteServiceSpecificationCommand(id));
            return Results.NoContent();
        });

        // Characteristics
        group.MapGet("/service-specifications/{specId:guid}/characteristics", async (Guid specId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCharacteristicsQuery(specId));
            return Results.Ok(result);
        });

        group.MapPost("/service-specifications/{specId:guid}/characteristics", async (Guid specId, AddCharacteristicCommand command, IMediator mediator) =>
        {
            command.GetType().GetProperty("ServiceSpecificationId")?.SetValue(command, specId);
            var id = await mediator.Send(command);
            return Results.Created($"/api/v1/service-catalog/service-specifications/{specId}/characteristics/{id}", id);
        });

        group.MapPut("/service-specifications/{specId:guid}/characteristics/{charId:guid}", async (Guid specId, Guid charId, UpdateCharacteristicCommand command, IMediator mediator) =>
        {
            if (specId != command.ServiceSpecificationId || charId != command.CharacteristicId)
                return Results.BadRequest("Id mismatch");
            await mediator.Send(command);
            return Results.NoContent();
        });

        group.MapDelete("/service-specifications/{specId:guid}/characteristics/{charId:guid}", async (Guid specId, Guid charId, IMediator mediator) =>
        {
            await mediator.Send(new RemoveCharacteristicCommand(specId, charId));
            return Results.NoContent();
        });

        // Characteristic Values
        group.MapGet("/service-specifications/{specId:guid}/characteristics/{charId:guid}/values", async (Guid specId, Guid charId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCharacteristicValuesQuery(specId, charId));
            return Results.Ok(result);
        });

        group.MapPost("/service-specifications/{specId:guid}/characteristics/{charId:guid}/values", async (Guid specId, Guid charId, AddCharacteristicValueCommand command, IMediator mediator) =>
        {
            if (specId != command.ServiceSpecificationId || charId != command.CharacteristicId)
                return Results.BadRequest("Id mismatch");
            var id = await mediator.Send(command);
            return Results.Created($"/api/v1/service-catalog/service-specifications/{specId}/characteristics/{charId}/values/{id}", id);
        });

        group.MapPut("/service-specifications/{specId:guid}/characteristics/{charId:guid}/values/{valueId:guid}", async (Guid specId, Guid charId, Guid valueId, UpdateCharacteristicValueCommand command, IMediator mediator) =>
        {
            if (specId != command.ServiceSpecificationId || charId != command.CharacteristicId || valueId != command.ValueId)
                return Results.BadRequest("Id mismatch");
            await mediator.Send(command);
            return Results.NoContent();
        });

        group.MapDelete("/service-specifications/{specId:guid}/characteristics/{charId:guid}/values/{valueId:guid}", async (Guid specId, Guid charId, Guid valueId, IMediator mediator) =>
        {
            await mediator.Send(new RemoveCharacteristicValueCommand(specId, charId, valueId));
            return Results.NoContent();
        });

        // Relationships
        group.MapGet("/service-specifications/{specId:guid}/relationships", async (Guid specId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSpecRelationshipsQuery(specId));
            return Results.Ok(result);
        });

        group.MapPost("/service-specifications/{specId:guid}/relationships", async (Guid specId, AddSpecRelationshipCommand command, IMediator mediator) =>
        {
            if (specId != command.ServiceSpecificationId)
                return Results.BadRequest("Id mismatch");
            var id = await mediator.Send(command);
            return Results.Created($"/api/v1/service-catalog/service-specifications/{specId}/relationships/{id}", id);
        });

        group.MapDelete("/service-specifications/{specId:guid}/relationships/{relId:guid}", async (Guid specId, Guid relId, IMediator mediator) =>
        {
            await mediator.Send(new RemoveSpecRelationshipCommand(specId, relId));
            return Results.NoContent();
        });
    }
}
