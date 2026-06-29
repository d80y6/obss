using Mapster;
using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetReconciliationStatus;

public sealed class GetReconciliationStatusQueryHandler : IRequestHandler<GetReconciliationStatusQuery, Result<PaymentReconciliationDto>>
{
    private readonly IPaymentReconciliationRepository _reconciliationRepository;

    public GetReconciliationStatusQueryHandler(IPaymentReconciliationRepository reconciliationRepository)
    {
        _reconciliationRepository = reconciliationRepository;
    }

    public async Task<Result<PaymentReconciliationDto>> Handle(GetReconciliationStatusQuery request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetWithItemsAsync(request.ReconciliationId, cancellationToken);

        if (reconciliation is null)
            return Result.Failure<PaymentReconciliationDto>(Error.NotFound("PaymentReconciliation", request.ReconciliationId));

        return Result.Success(reconciliation.Adapt<PaymentReconciliationDto>());
    }
}
