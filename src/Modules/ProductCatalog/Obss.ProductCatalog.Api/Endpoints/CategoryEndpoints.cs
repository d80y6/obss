using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.Commands.CreateCategory;
using Obss.ProductCatalog.Application.Commands.DeleteCategory;
using Obss.ProductCatalog.Application.Commands.PatchCategory;
using Obss.ProductCatalog.Application.Commands.UpdateCategory;

using Obss.SharedKernel.Application.Authorization;

namespace Obss.ProductCatalog.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/categories", async (ICategoryRepository repository) =>
        {
            var categories = await repository.GetActiveCategoriesAsync();
            return (IResult)TypedResults.Ok(categories);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.CategoryRead));

        group.MapGet("/categories/{id:guid}", async (Guid id, ICategoryRepository repository) =>
        {
            var category = await repository.GetByIdAsync(id);
            return category is not null
                ? (IResult)TypedResults.Ok(category)
                : (IResult)TypedResults.NotFound();
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.CategoryRead));

        group.MapPost("/categories", async (CreateCategoryCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/catalog/categories/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.CategoryWrite));

        group.MapPut("/categories/{id:guid}", async (Guid id, UpdateCategoryCommand command, IMediator mediator) =>
        {
            if (id != command.CategoryId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.CategoryWrite));

        group.MapPatch("/categories/{id:guid}", async (Guid id, PatchCategoryCommand command, IMediator mediator) =>
        {
            if (id != command.CategoryId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.CategoryWrite));

        group.MapDelete("/categories/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteCategoryCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Catalog.CategoryWrite));
    }
}
