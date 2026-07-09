using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetListAsync(cancellationToken);
        return Result.Success(products.Adapt<List<ProductDto>>());
    }
}
