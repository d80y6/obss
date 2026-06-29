using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.GetProductConfiguration;

public sealed class GetProductConfigurationQueryHandler : IRequestHandler<GetProductConfigurationQuery, Result<ProductConfigurationDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductConfigurationRepository _configRepository;

    public GetProductConfigurationQueryHandler(
        IProductRepository productRepository,
        IProductConfigurationRepository configRepository)
    {
        _productRepository = productRepository;
        _configRepository = configRepository;
    }

    public async Task<Result<ProductConfigurationDto>> Handle(GetProductConfigurationQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<ProductConfigurationDto>(Error.NotFound(nameof(Product), request.ProductId));

        var rules = await _configRepository.GetRulesByProductAsync(request.ProductId, cancellationToken);
        var options = await _configRepository.GetOptionsByProductAsync(request.ProductId, cancellationToken);

        return Result.Success(new ProductConfigurationDto(
            request.ProductId,
            rules.Adapt<List<ProductConfigurationRuleDto>>(),
            options.Adapt<List<ProductOptionDto>>()));
    }
}
