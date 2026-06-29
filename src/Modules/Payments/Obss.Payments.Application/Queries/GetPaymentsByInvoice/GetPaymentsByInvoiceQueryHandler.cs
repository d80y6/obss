using Mapster;
using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPaymentsByInvoice;

public sealed class GetPaymentsByInvoiceQueryHandler : IRequestHandler<GetPaymentsByInvoiceQuery, Result<IReadOnlyList<PaymentDto>>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsByInvoiceQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<IReadOnlyList<PaymentDto>>> Handle(GetPaymentsByInvoiceQuery request, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetByInvoiceAsync(request.InvoiceId, cancellationToken);

        var result = payments.Adapt<List<PaymentDto>>();
        return Result.Success<IReadOnlyList<PaymentDto>>(result);
    }
}
