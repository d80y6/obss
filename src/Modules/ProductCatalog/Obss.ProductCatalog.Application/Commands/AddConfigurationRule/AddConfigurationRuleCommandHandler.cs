using Mapster;
using MediatR;
using Obss.ProductCatalog.Application.Abstractions;
using Obss.ProductCatalog.Application.DTOs;
using Obss.ProductCatalog.Domain.Domain.Entities;
using Obss.ProductCatalog.Domain.Domain.Exceptions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.AddConfigurationRule;

public sealed class AddConfigurationRuleCommandHandler : IRequestHandler<AddConfigurationRuleCommand, Result<ProductConfigurationRuleDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductConfigurationRepository _configRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddConfigurationRuleCommandHandler(
        IProductRepository productRepository,
        IProductConfigurationRepository configRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _configRepository = configRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductConfigurationRuleDto>> Handle(AddConfigurationRuleCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure<ProductConfigurationRuleDto>(Error.NotFound(nameof(Product), request.ProductId));

        var rule = ProductConfigurationRule.Create(
            request.ProductId,
            request.RuleType,
            request.TargetProductId,
            request.TargetOption,
            request.Condition);

        await _configRepository.AddRuleAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.Adapt<ProductConfigurationRuleDto>());
    }
}
