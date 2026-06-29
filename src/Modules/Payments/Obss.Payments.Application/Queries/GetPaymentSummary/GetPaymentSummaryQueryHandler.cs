using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetPaymentSummary;

public sealed class GetPaymentSummaryQueryHandler : IRequestHandler<GetPaymentSummaryQuery, Result<PaymentSummaryDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentSummaryQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Result<PaymentSummaryDto>> Handle(GetPaymentSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await _paymentRepository.GetPaymentSummaryAsync(cancellationToken);

        return Result.Success(new PaymentSummaryDto(
            summary.TotalPayments,
            summary.PendingCount,
            summary.CompletedCount,
            summary.FailedCount,
            summary.RefundedCount,
            summary.PartiallyRefundedCount,
            summary.TotalAmount,
            summary.TotalCompletedAmount,
            summary.TotalRefundedAmount,
            summary.NetAmount));
    }
}
