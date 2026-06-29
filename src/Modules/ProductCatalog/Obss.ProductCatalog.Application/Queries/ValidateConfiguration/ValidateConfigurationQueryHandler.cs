using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Queries.ValidateConfiguration;

public sealed class ValidateConfigurationQueryHandler : IRequestHandler<ValidateConfigurationQuery, Result<ConfigurationValidationResultDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductConfigurationRepository _configRepository;
    private readonly IProductConfigurationValidator _validator;

    public ValidateConfigurationQueryHandler(
        IProductRepository productRepository,
        IProductConfigurationRepository configRepository,
        IProductConfigurationValidator validator)
    {
        _productRepository = productRepository;
        _configRepository = configRepository;
        _validator = validator;
    }

    public async Task<Result<ConfigurationValidationResultDto>> Handle(ValidateConfigurationQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<ConfigurationValidationResultDto>(Error.NotFound(nameof(Product), request.ProductId));

        var rules = await _configRepository.GetRulesByProductAsync(request.ProductId, cancellationToken);
        var options = await _configRepository.GetOptionsByProductAsync(request.ProductId, cancellationToken);

        var result = _validator.Validate(request.SelectedOptions, rules, options);

        return Result.Success(result);
    }
}
