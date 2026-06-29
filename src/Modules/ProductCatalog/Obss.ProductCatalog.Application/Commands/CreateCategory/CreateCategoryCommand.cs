using MediatR;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string TenantId,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    int SortOrder) : IRequest<Result<CategoryDto>>;
