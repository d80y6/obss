using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.RegisterPaymentGateway;

public sealed class RegisterPaymentGatewayCommandHandler : IRequestHandler<RegisterPaymentGatewayCommand, Result<PaymentGatewayDto>>
{
    private readonly IPaymentGatewayRepository _gatewayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<RegisterPaymentGatewayCommandHandler> _logger;

    public RegisterPaymentGatewayCommandHandler(
        IPaymentGatewayRepository gatewayRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<RegisterPaymentGatewayCommandHandler> logger)
    {
        _gatewayRepository = gatewayRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<Result<PaymentGatewayDto>> Handle(RegisterPaymentGatewayCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentProvider>(request.Provider, true, out var provider))
            return Result.Failure<PaymentGatewayDto>(Error.Validation($"Invalid payment provider: '{request.Provider}'."));

        if (!Enum.TryParse<FeeType>(request.FeeType, true, out var feeType))
            return Result.Failure<PaymentGatewayDto>(Error.Validation($"Invalid fee type: '{request.FeeType}'."));

        var gateway = PaymentGateway.Create(
            _currentTenant.TenantId!,
            request.Name,
            provider,
            request.Configuration,
            request.SupportedCurrencies,
            request.MinAmount,
            request.MaxAmount,
            request.TransactionFee,
            feeType);

        await _gatewayRepository.AddAsync(gateway, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment gateway '{Name}' ({Provider}) registered.", gateway.Name, gateway.Provider);

        return Result.Success(gateway.Adapt<PaymentGatewayDto>());
    }
}
