using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetCatalogById;

public sealed class GetCatalogByIdQueryHandler : IRequestHandler<GetCatalogByIdQuery, Result<CatalogDto>>
{
    private readonly ICatalogRepository _repository;

    public GetCatalogByIdQueryHandler(ICatalogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CatalogDto>> Handle(GetCatalogByIdQuery request, CancellationToken cancellationToken)
    {
        var catalog = await _repository.GetByIdAsync(request.CatalogId, cancellationToken);
        if (catalog is null)
            return Result.Failure<CatalogDto>(Error.NotFound("Catalog", request.CatalogId));

        return Result.Success(new CatalogDto(
            catalog.Id,
            catalog.TenantId,
            catalog.Name,
            catalog.Description,
            catalog.CatalogType,
            catalog.LifecycleStatus,
            catalog.Version,
            catalog.ValidFrom,
            catalog.ValidTo,
            catalog.CreatedAt,
            catalog.UpdatedAt));
    }
}
