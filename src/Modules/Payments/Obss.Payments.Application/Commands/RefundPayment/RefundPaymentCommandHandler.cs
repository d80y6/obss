using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.RefundPayment;

public sealed class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RefundPaymentCommandHandler> _logger;

    public RefundPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ILogger<RefundPaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdWithDetailsAsync(request.PaymentId, cancellationToken);

        if (payment is null)
            return Result.Failure(Error.NotFound("Payment", request.PaymentId));

        payment.Refund(request.Amount, request.Reason);

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refund of {Amount} processed for payment {PaymentNumber}. Reason: {Reason}",
            request.Amount, payment.PaymentNumber, request.Reason);

        return Result.Success();
    }
}
