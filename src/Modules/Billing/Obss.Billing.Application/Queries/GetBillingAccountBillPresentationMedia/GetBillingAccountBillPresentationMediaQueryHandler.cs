using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountBillPresentationMedia;

public sealed class GetBillingAccountBillPresentationMediaQueryHandler(
    IRepository<BillingAccount> repository)
    : IRequestHandler<GetBillingAccountBillPresentationMediaQuery, Result<List<BillPresentationMediaDto>>>
{
    public async Task<Result<List<BillPresentationMediaDto>>> Handle(GetBillingAccountBillPresentationMediaQuery request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<List<BillPresentationMediaDto>>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var dtos = account.BillPresentationMedia.Adapt<List<BillPresentationMediaDto>>();
        return Result.Success(dtos);
    }
}
