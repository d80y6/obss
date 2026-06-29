using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.GenerateBillingCycle;

public sealed class GenerateBillingCycleCommandHandler : IRequestHandler<GenerateBillingCycleCommand, Result<BillingCycleDto>>
{
    private readonly IBillingCycleRepository _billingCycleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateBillingCycleCommandHandler> _logger;

    public GenerateBillingCycleCommandHandler(
        IBillingCycleRepository billingCycleRepository,
        IUnitOfWork unitOfWork,
        ILogger<GenerateBillingCycleCommandHandler> logger)
    {
        _billingCycleRepository = billingCycleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BillingCycleDto>> Handle(GenerateBillingCycleCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<BillingPeriod>(request.BillingPeriod, true, out var billingPeriod))
        {
            return Result.Failure<BillingCycleDto>(Error.Validation($"Invalid billing period: '{request.BillingPeriod}'."));
        }

        var existingCycle = await _billingCycleRepository.GetByCustomerAsync(request.CustomerId, cancellationToken);
        if (existingCycle is not null)
        {
            return Result.Failure<BillingCycleDto>(Error.Conflict($"A billing cycle already exists for customer '{request.CustomerId}'."));
        }

        var cycle = BillingCycle.Create(
            string.Empty,
            request.CustomerId,
            billingPeriod,
            request.LastBillingDate,
            request.NextBillingDate);

        await _billingCycleRepository.AddAsync(cycle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Billing cycle {CycleId} generated for customer {CustomerId} with period {BillingPeriod}",
            cycle.Id,
            cycle.CustomerId,
            cycle.BillingPeriod);

        return Result.Success(cycle.Adapt<BillingCycleDto>());
    }
}
