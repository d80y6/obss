using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Subscriptions.Application.Abstractions;
using Obss.Subscriptions.Application.DTOs;

namespace Obss.Subscriptions.Application.Queries.GetProductsByCustomer;

public sealed class GetProductsByCustomerQueryHandler : IRequestHandler<GetProductsByCustomerQuery, Result<List<ProductDto>>>
{
    private readonly IProductRepository _repository;

    public GetProductsByCustomerQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<Result<List<ProductDto>>> Handle(GetProductsByCustomerQuery request, CancellationToken cancellationToken)
    {
        var all = await _repository.GetListAsync(cancellationToken);
        var filtered = all.Where(p => p.CustomerId == request.CustomerId);
        return Result.Success(filtered.Adapt<List<ProductDto>>());
    }
}
