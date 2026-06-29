using Mapster;
using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPaymentById;

public sealed class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, Result<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PaymentDto>> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdWithDetailsAsync(request.PaymentId, cancellationToken);

        if (payment is null)
            return Result.Failure<PaymentDto>(Error.NotFound(nameof(Payment), request.PaymentId));

        return Result.Success(payment.Adapt<PaymentDto>());
    }
}
