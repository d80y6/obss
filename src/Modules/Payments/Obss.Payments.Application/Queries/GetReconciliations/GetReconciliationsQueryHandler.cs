using Mapster;
using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetReconciliations;

public sealed class GetReconciliationsQueryHandler : IRequestHandler<GetReconciliationsQuery, Result<IReadOnlyList<PaymentReconciliationDto>>>
{
    private readonly IPaymentReconciliationRepository _reconciliationRepository;

    public GetReconciliationsQueryHandler(IPaymentReconciliationRepository reconciliationRepository)
    {
        _reconciliationRepository = reconciliationRepository;
    }

    public async Task<Result<IReadOnlyList<PaymentReconciliationDto>>> Handle(GetReconciliationsQuery request, CancellationToken cancellationToken)
    {
        var reconciliations = await _reconciliationRepository.GetFilteredAsync(
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = reconciliations.Adapt<List<PaymentReconciliationDto>>();
        return Result.Success<IReadOnlyList<PaymentReconciliationDto>>(result);
    }
}
