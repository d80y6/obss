using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ProductCatalog.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/categories", async (ICategoryRepository repository) =>
        {
            var categories = await repository.GetActiveCategoriesAsync();
            return (IResult)TypedResults.Ok(categories);
        });

        group.MapGet("/categories/{id:guid}", async (Guid id, ICategoryRepository repository) =>
        {
            var category = await repository.GetByIdAsync(id);
            return category is not null
                ? (IResult)TypedResults.Ok(category)
                : (IResult)TypedResults.NotFound();
        });

        group.MapPost("/categories", async (CreateCategoryRequest request, ICategoryRepository repository, IUnitOfWork unitOfWork) =>
        {
            var category = Category.Create(request.TenantId, request.Name, request.Description, request.ParentCategoryId, request.SortOrder);
            await repository.AddAsync(category);
            await unitOfWork.SaveChangesAsync();
            return (IResult)TypedResults.Created($"/api/v1/catalog/categories/{category.Id}", category);
        });

        group.MapPut("/categories/{id:guid}", async (Guid id, UpdateCategoryRequest request, ICategoryRepository repository, IUnitOfWork unitOfWork) =>
        {
            var category = await repository.GetByIdAsync(id);
            if (category is null)
                return (IResult)TypedResults.NotFound();

            category.UpdateDetails(request.Name, request.Description, request.SortOrder);
            await repository.UpdateAsync(category);
            await unitOfWork.SaveChangesAsync();
            return (IResult)TypedResults.Ok(category);
        });

        group.MapDelete("/categories/{id:guid}", async (Guid id, ICategoryRepository repository, IUnitOfWork unitOfWork) =>
        {
            var category = await repository.GetByIdAsync(id);
            if (category is null)
                return (IResult)TypedResults.NotFound();

            await repository.DeleteAsync(category);
            await unitOfWork.SaveChangesAsync();
            return (IResult)TypedResults.NoContent();
        });
    }
}

public record CreateCategoryRequest(string TenantId, string Name, string? Description, Guid? ParentCategoryId, int SortOrder);
public record UpdateCategoryRequest(string Name, string? Description, int SortOrder);
