using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CreateTaxRule;

public sealed class CreateTaxRuleCommandHandler : IRequestHandler<CreateTaxRuleCommand, Result<TaxRuleDto>>
{
    private readonly ITaxRuleRepository _taxRuleRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateTaxRuleCommandHandler> _logger;

    public CreateTaxRuleCommandHandler(
        ITaxRuleRepository taxRuleRepository,
        ICurrentTenant currentTenant,
        IUnitOfWork unitOfWork,
        ILogger<CreateTaxRuleCommandHandler> logger)
    {
        _taxRuleRepository = taxRuleRepository;
        _currentTenant = currentTenant;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TaxRuleDto>> Handle(CreateTaxRuleCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TaxType>(request.TaxType, true, out var taxType))
        {
            return Result.Failure<TaxRuleDto>(Error.Validation($"Invalid tax type: '{request.TaxType}'. Must be 'Percentage' or 'Fixed'."));
        }

        var tenantId = _currentTenant.TenantId ?? string.Empty;

        var taxRule = TaxRule.Create(
            tenantId,
            request.Name,
            request.Description,
            request.TaxRate,
            taxType,
            request.TaxCategory,
            request.Country,
            request.Region,
            request.IsCompound,
            request.Priority,
            request.EffectiveFrom,
            request.EffectiveTo);

        await _taxRuleRepository.AddAsync(taxRule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tax rule {TaxRuleId} '{Name}' created with rate {TaxRate}",
            taxRule.Id,
            taxRule.Name,
            taxRule.TaxRate);

        return Result.Success(taxRule.Adapt<TaxRuleDto>());
    }
}
