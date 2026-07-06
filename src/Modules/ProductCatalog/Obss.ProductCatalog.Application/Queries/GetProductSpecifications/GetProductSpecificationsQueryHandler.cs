using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.Contracts;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductSpecifications;

public sealed class GetProductSpecificationsQueryHandler : IRequestHandler<GetProductSpecificationsQuery, Result<PaginatedResult<ProductSpecificationDto>>>
{
    private readonly IProductSpecificationRepository _repository;

    public GetProductSpecificationsQueryHandler(IProductSpecificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<ProductSpecificationDto>>> Handle(GetProductSpecificationsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetFilteredAsync(
            request.SearchTerm,
            request.Status,
            request.Brand,
            request.Offset,
            request.Limit,
            cancellationToken);

        return Result.Success(new PaginatedResult<ProductSpecificationDto>(
            items.Adapt<List<ProductSpecificationDto>>(),
            totalCount));
    }
}
