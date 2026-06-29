using Mapster;
using MediatR;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetUnmatchedTransactions;

public sealed class GetUnmatchedTransactionsQueryHandler : IRequestHandler<GetUnmatchedTransactionsQuery, Result<List<ReconciliationItemDto>>>
{
    private readonly IPaymentReconciliationRepository _reconciliationRepository;
    private readonly ICurrentTenant _currentTenant;

    public GetUnmatchedTransactionsQueryHandler(
        IPaymentReconciliationRepository reconciliationRepository,
        ICurrentTenant currentTenant)
    {
        _reconciliationRepository = reconciliationRepository;
        _currentTenant = currentTenant;
    }

    public async Task<Result<List<ReconciliationItemDto>>> Handle(GetUnmatchedTransactionsQuery request, CancellationToken cancellationToken)
    {
        var unmatchedItems = await _reconciliationRepository.GetUnmatchedItemsAsync(_currentTenant.TenantId!, cancellationToken);

        return Result.Success(unmatchedItems.Adapt<List<ReconciliationItemDto>>());
    }
}
