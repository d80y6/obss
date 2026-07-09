using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result.Failure<ProductDto>(Error.NotFound("Product", request.Id));

        return Result.Success(product.Adapt<ProductDto>());
    }
}
