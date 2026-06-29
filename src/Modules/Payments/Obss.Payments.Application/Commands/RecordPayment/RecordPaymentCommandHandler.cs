using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;
using Obss.Payments.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.RecordPayment;

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<RecordPaymentCommandHandler> _logger;

    public RecordPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<RecordPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<Result<PaymentDto>> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PaymentMethodType>(request.PaymentMethod, true, out var paymentMethod))
            return Result.Failure<PaymentDto>(Error.Validation($"Invalid payment method: '{request.PaymentMethod}'."));

        var paymentNumber = await _paymentRepository.GenerateNextPaymentNumberAsync(cancellationToken);

        var payment = Payment.Create(
            _currentTenant.TenantId!,
            paymentNumber,
            request.CustomerId,
            request.Amount,
            request.Currency,
            paymentMethod,
            request.PaymentReference,
            request.InvoiceId,
            request.Notes);

        payment.Complete();

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentNumber} recorded for customer {CustomerId}. Amount: {Amount} {Currency}",
            paymentNumber, request.CustomerId, request.Amount, request.Currency);

        return Result.Success(payment.Adapt<PaymentDto>());
    }
}
