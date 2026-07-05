using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountById;

public sealed class GetBillingAccountByIdQueryHandler(
    IRepository<BillingAccount> repository)
    : IRequestHandler<GetBillingAccountByIdQuery, Result<BillingAccountDto>>
{
    public async Task<Result<BillingAccountDto>> Handle(GetBillingAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.Failure<BillingAccountDto>(Error.NotFound(nameof(BillingAccount), request.Id));

        return Result.Success(account.Adapt<BillingAccountDto>());
    }
}
