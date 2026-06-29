using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetFilteredAsync(
            request.CategoryId,
            request.ProductType,
            request.Status,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = products.Adapt<List<ProductDto>>();
        return Result.Success<IReadOnlyList<ProductDto>>(result);
    }
}
