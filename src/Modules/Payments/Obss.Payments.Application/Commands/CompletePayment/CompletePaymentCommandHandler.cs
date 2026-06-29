using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.CompletePayment;

public sealed class CompletePaymentCommandHandler : IRequestHandler<CompletePaymentCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletePaymentCommandHandler> _logger;

    public CompletePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompletePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(CompletePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);

        if (payment is null)
            return Result.Failure(Error.NotFound("Payment", request.PaymentId));

        payment.Complete();

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentNumber} completed.", payment.PaymentNumber);

        return Result.Success();
    }
}
