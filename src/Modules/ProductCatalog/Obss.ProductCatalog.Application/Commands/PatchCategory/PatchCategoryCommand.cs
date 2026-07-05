using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.PatchCategory;

public sealed record PatchCategoryCommand(
    Guid CategoryId,
    Optional<string> Name,
    Optional<string?> Description,
    Optional<Guid?> ParentCategoryId,
    Optional<int> SortOrder) : IRequest<Result<CategoryDto>>;
