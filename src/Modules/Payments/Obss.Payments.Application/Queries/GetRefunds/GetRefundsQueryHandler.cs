using Mapster;
using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetRefunds;

public sealed class GetRefundsQueryHandler : IRequestHandler<GetRefundsQuery, Result<IReadOnlyList<RefundDto>>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetRefundsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<IReadOnlyList<RefundDto>>> Handle(GetRefundsQuery request, CancellationToken cancellationToken)
    {
        var refunds = await _paymentRepository.GetRefundsFilteredAsync(
            request.PaymentId,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Offset,
            request.Limit,
            cancellationToken);

        var result = refunds.Adapt<List<RefundDto>>();
        return Result.Success<IReadOnlyList<RefundDto>>(result);
    }
}
