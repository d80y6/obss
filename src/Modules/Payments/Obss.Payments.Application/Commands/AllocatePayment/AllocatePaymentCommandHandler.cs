using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.AllocatePayment;

public sealed class AllocatePaymentCommandHandler : IRequestHandler<AllocatePaymentCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AllocatePaymentCommandHandler> _logger;

    public AllocatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ILogger<AllocatePaymentCommandHandler> logger)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AllocatePaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdWithDetailsAsync(request.PaymentId, cancellationToken);

        if (payment is null)
            return Result.Failure(Error.NotFound("Payment", request.PaymentId));

        payment.AllocateToInvoice(request.InvoiceId, request.Amount);

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Allocated {Amount} from payment {PaymentNumber} to invoice {InvoiceId}.",
            request.Amount, payment.PaymentNumber, request.InvoiceId);

        return Result.Success();
    }
}
