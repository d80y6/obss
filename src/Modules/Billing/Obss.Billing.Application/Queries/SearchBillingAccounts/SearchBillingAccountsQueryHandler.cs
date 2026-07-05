using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.SearchBillingAccounts;

public sealed class SearchBillingAccountsQueryHandler(
    IRepository<BillingAccount> repository)
    : IRequestHandler<SearchBillingAccountsQuery, Result<IReadOnlyList<BillingAccountDto>>>
{
    public async Task<Result<IReadOnlyList<BillingAccountDto>>> Handle(SearchBillingAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await repository.GetAllAsync(cancellationToken);
        var filtered = accounts.AsEnumerable();

        if (request.CustomerId.HasValue)
            filtered = filtered.Where(ba => ba.CustomerId == request.CustomerId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            filtered = filtered.Where(ba => ba.Status == request.Status);

        var dtos = filtered.Select(ba => ba.Adapt<BillingAccountDto>()).ToList();
        return Result.Success<IReadOnlyList<BillingAccountDto>>(dtos);
    }
}
