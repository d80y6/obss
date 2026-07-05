using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.Contracts;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetCatalogs;

public sealed class GetCatalogsQueryHandler : IRequestHandler<GetCatalogsQuery, Result<PaginatedResult<CatalogDto>>>
{
    private readonly ICatalogRepository _repository;

    public GetCatalogsQueryHandler(ICatalogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<CatalogDto>>> Handle(GetCatalogsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetFilteredAsync(
            request.SearchTerm,
            request.CatalogType,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _repository.GetTotalCountAsync(
            request.SearchTerm,
            request.CatalogType,
            cancellationToken);

        var dtos = items
            .Select(c => new CatalogDto(
                c.Id,
                c.TenantId,
                c.Name,
                c.Description,
                c.CatalogType,
                c.LifecycleStatus,
                c.Version,
                c.ValidFrom,
                c.ValidTo,
                c.CreatedAt,
                c.UpdatedAt))
            .ToList();

        return Result.Success(new PaginatedResult<CatalogDto>(dtos, totalCount));
    }
}
