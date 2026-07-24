using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Commands.AddCharacteristic;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;
using Obss.ProductCatalog.Application.Commands.AddCharacteristicValue;
using Obss.ProductCatalog.Application.Commands.AddSpecificationRelationship;
using Obss.ProductCatalog.Application.Commands.CreateProductSpecification;
using Obss.ProductCatalog.Application.Commands.DeleteProductSpecification;
using Obss.ProductCatalog.Application.Commands.PatchProductSpecification;
using Obss.ProductCatalog.Application.Commands.RemoveCharacteristic;
using Obss.ProductCatalog.Application.Commands.RemoveCharacteristicValue;
using Obss.ProductCatalog.Application.Commands.RemoveSpecificationRelationship;
using Obss.ProductCatalog.Application.Commands.UpdateCharacteristic;
using Obss.ProductCatalog.Application.Commands.UpdateCharacteristicValue;
using Obss.ProductCatalog.Application.Commands.UpdateProductSpecification;
using Obss.ProductCatalog.Application.Queries.GetProductSpecificationById;
using Obss.ProductCatalog.Application.Queries.GetProductSpecifications;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.ProductCatalog.Api.Endpoints;

public static class ProductSpecificationEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        // --- ProductSpecification CRUD ---
        group.MapPost("/product-specifications", async (CreateProductSpecificationCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapGet("/product-specifications/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductSpecificationByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationRead));

        group.MapGet("/product-specifications", async ([AsParameters] GetProductSpecificationsQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, result.Value.TotalCount);
            return (IResult)TypedResults.Ok(result.Value.Items);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationRead));

        group.MapPut("/product-specifications/{id:guid}", async (Guid id, UpdateProductSpecificationCommand command, IMediator mediator) =>
        {
            if (id != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapPatch("/product-specifications/{id:guid}", async (Guid id, PatchProductSpecificationCommand command, IMediator mediator) =>
        {
            if (id != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapDelete("/product-specifications/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteProductSpecificationCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        // --- Characteristics ---
        group.MapGet("/product-specifications/{specId:guid}/characteristics", async (Guid specId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductSpecificationByIdQuery(specId));
            if (!result.IsSuccess)
                return (IResult)TypedResults.NotFound(result.Error);
            return (IResult)TypedResults.Ok(result.Value.Characteristics);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationRead));

        group.MapPost("/product-specifications/{specId:guid}/characteristics", async (Guid specId, AddCharacteristicCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{specId}/characteristics/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapPut("/product-specifications/{specId:guid}/characteristics/{charId:guid}", async (Guid specId, Guid charId, UpdateCharacteristicCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId || charId != command.CharacteristicId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapDelete("/product-specifications/{specId:guid}/characteristics/{charId:guid}", async (Guid specId, Guid charId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveCharacteristicCommand(specId, charId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        // --- Characteristic Values ---
        group.MapGet("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values", async (Guid specId, Guid charId, IMediator mediator) =>
        {
            var specResult = await mediator.Send(new GetProductSpecificationByIdQuery(specId));
            if (!specResult.IsSuccess)
                return (IResult)TypedResults.NotFound(specResult.Error);

            var characteristic = specResult.Value.Characteristics.FirstOrDefault(c => c.Id == charId);
            if (characteristic is null)
                return (IResult)TypedResults.NotFound();

            return (IResult)TypedResults.Ok(characteristic.Values);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationRead));

        group.MapPost("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values", async (Guid specId, Guid charId, AddCharacteristicValueCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId || charId != command.CharacteristicId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{specId}/characteristics/{charId}/values/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapPut("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values/{valueId:guid}", async (Guid specId, Guid charId, Guid valueId, UpdateCharacteristicValueCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId || charId != command.CharacteristicId || valueId != command.ValueId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapDelete("/product-specifications/{specId:guid}/characteristics/{charId:guid}/values/{valueId:guid}", async (Guid specId, Guid charId, Guid valueId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveCharacteristicValueCommand(specId, charId, valueId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        // --- Relationships ---
        group.MapGet("/product-specifications/{specId:guid}/relationships", async (Guid specId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductSpecificationByIdQuery(specId));
            if (!result.IsSuccess)
                return (IResult)TypedResults.NotFound(result.Error);
            return (IResult)TypedResults.Ok(result.Value.Relationships);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationRead));

        group.MapPost("/product-specifications/{specId:guid}/relationships", async (Guid specId, AddSpecificationRelationshipCommand command, IMediator mediator) =>
        {
            if (specId != command.ProductSpecificationId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/product-specifications/{specId}/relationships/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));

        group.MapDelete("/product-specifications/{specId:guid}/relationships/{relId:guid}", async (Guid specId, Guid relId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveSpecificationRelationshipCommand(specId, relId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.SpecificationWrite));
    }
}
