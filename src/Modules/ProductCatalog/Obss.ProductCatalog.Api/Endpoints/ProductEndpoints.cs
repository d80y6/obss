using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Commands.AddConfigurationRule;
using Obss.ProductCatalog.Application.Commands.CreateProduct;
using Obss.ProductCatalog.Application.Commands.UpdateProduct;
using Obss.ProductCatalog.Application.Commands.CreateProductOption;
using Obss.ProductCatalog.Application.Queries.GetProductById;
using Obss.ProductCatalog.Application.Queries.GetProductConfiguration;
using Obss.ProductCatalog.Application.Queries.GetProducts;
using Obss.ProductCatalog.Application.Queries.ValidateConfiguration;

namespace Obss.ProductCatalog.Api.Endpoints;

public static class ProductEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/products", async (CreateProductCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/products/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/products/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/products", async ([AsParameters] GetProductsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/products/{id:guid}", async (Guid id, UpdateProductCommand command, IMediator mediator) =>
        {
            if (id != command.ProductId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/products/{id:guid}/configuration-rules", async (Guid id, AddConfigurationRuleCommand command, IMediator mediator) =>
        {
            if (id != command.ProductId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/products/{id}/configuration-rules/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/products/{id:guid}/options", async (Guid id, CreateProductOptionCommand command, IMediator mediator) =>
        {
            if (id != command.ProductId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/products/{id}/options/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/products/{id:guid}/configuration", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductConfigurationQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/products/{id:guid}/validate-configuration", async (Guid id, ValidateConfigurationQuery query, IMediator mediator) =>
        {
            if (id != query.ProductId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

    }
}
