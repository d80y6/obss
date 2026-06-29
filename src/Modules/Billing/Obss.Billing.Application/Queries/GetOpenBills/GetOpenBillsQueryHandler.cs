using Mapster;
using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetOpenBills;

public sealed class GetOpenBillsQueryHandler : IRequestHandler<GetOpenBillsQuery, Result<IReadOnlyList<BillDto>>>
{
    private readonly IBillRepository _billRepository;

    public GetOpenBillsQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<Result<IReadOnlyList<BillDto>>> Handle(GetOpenBillsQuery request, CancellationToken cancellationToken)
    {
        var bills = await _billRepository.GetOpenBillsAsync(cancellationToken);
        return Result.Success<IReadOnlyList<BillDto>>(bills.Adapt<List<BillDto>>());
    }
}
