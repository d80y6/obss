using Mapster;
using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPayments;

public sealed class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, Result<IReadOnlyList<PaymentDto>>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<IReadOnlyList<PaymentDto>>> Handle(GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await _paymentRepository.GetFilteredAsync(
            request.CustomerId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = payments.Adapt<List<PaymentDto>>();
        return Result.Success<IReadOnlyList<PaymentDto>>(result);
    }
}
