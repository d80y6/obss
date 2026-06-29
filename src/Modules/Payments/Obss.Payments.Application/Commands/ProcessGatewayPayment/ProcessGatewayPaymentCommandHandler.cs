using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.Services;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.ProcessGatewayPayment;

public sealed class ProcessGatewayPaymentCommandHandler : IRequestHandler<ProcessGatewayPaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentGatewayService _gatewayService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<ProcessGatewayPaymentCommandHandler> _logger;

    public ProcessGatewayPaymentCommandHandler(
        IPaymentGatewayService gatewayService,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<ProcessGatewayPaymentCommandHandler> logger)
    {
        _gatewayService = gatewayService;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(ProcessGatewayPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentMethodType>(request.PaymentMethod, true, out var paymentMethod))
            return Result.Failure<PaymentDto>(Error.Validation($"Invalid payment method: '{request.PaymentMethod}'."));

        var gatewayRequest = new PaymentRequest(
            request.Amount,
            request.Currency,
            request.PaymentMethod,
            request.ReturnUrl,
            request.CancelUrl,
            request.CustomerId.ToString(),
            request.Description);

        var gatewayResult = await _gatewayService.ProcessPayment(gatewayRequest, cancellationToken);

        if (!gatewayResult.Success)
            return Result.Failure<PaymentDto>(Error.Validation(gatewayResult.Message ?? "Payment processing failed."));

        var paymentNumber = await _paymentRepository.GenerateNextPaymentNumberAsync(cancellationToken);

        var payment = Payment.Create(
            _currentTenant.TenantId!,
            paymentNumber,
            request.CustomerId,
            request.Amount,
            request.Currency,
            paymentMethod,
            gatewayResult.TransactionId,
            null,
            request.Description);

        if (gatewayResult.Status == PaymentStatus.Completed)
            payment.Complete();
        else
            payment.MarkAsPending();

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Gateway payment {PaymentNumber} processed for customer {CustomerId}. Transaction: {TransactionId}",
            paymentNumber, request.CustomerId, gatewayResult.TransactionId);

        return Result.Success(payment.Adapt<PaymentDto>());
    }
}
