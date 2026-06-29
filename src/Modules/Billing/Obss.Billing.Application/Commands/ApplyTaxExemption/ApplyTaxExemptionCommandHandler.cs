using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.ApplyTaxExemption;

public sealed class ApplyTaxExemptionCommandHandler : IRequestHandler<ApplyTaxExemptionCommand, Result<TaxExemptionDto>>
{
    private readonly ITaxRuleRepository _taxRuleRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApplyTaxExemptionCommandHandler> _logger;

    public ApplyTaxExemptionCommandHandler(
        ITaxRuleRepository taxRuleRepository,
        ICurrentTenant currentTenant,
        IUnitOfWork unitOfWork,
        ILogger<ApplyTaxExemptionCommandHandler> logger)
    {
        _taxRuleRepository = taxRuleRepository;
        _currentTenant = currentTenant;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<TaxExemptionDto>> Handle(ApplyTaxExemptionCommand request, CancellationToken cancellationToken)
    {
        var taxRule = await _taxRuleRepository.GetByIdAsync(request.TaxRuleId, cancellationToken);
        if (taxRule is null)
        {
            return Result.Failure<TaxExemptionDto>(Error.NotFound(nameof(TaxRule), request.TaxRuleId));
        }

        var tenantId = _currentTenant.TenantId ?? string.Empty;

        var exemption = TaxExemption.Create(
            tenantId,
            request.CustomerId,
            request.TaxRuleId,
            request.ExemptionCertificate,
            request.ExemptionRate,
            request.ValidFrom,
            request.ValidTo,
            request.ApprovedBy);

        await _taxRuleRepository.AddExemptionAsync(exemption, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Tax exemption applied for customer {CustomerId} on rule {TaxRuleId} with rate {ExemptionRate}",
            request.CustomerId,
            request.TaxRuleId,
            request.ExemptionRate);

        return Result.Success(exemption.Adapt<TaxExemptionDto>());
    }
}
