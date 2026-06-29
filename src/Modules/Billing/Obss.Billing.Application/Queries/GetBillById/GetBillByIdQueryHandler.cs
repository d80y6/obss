using Mapster;
using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillById;

public sealed class GetBillByIdQueryHandler : IRequestHandler<GetBillByIdQuery, Result<BillDto>>
{
    private readonly IBillRepository _billRepository;

    public GetBillByIdQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<Result<BillDto>> Handle(GetBillByIdQuery request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdWithLinesAsync(request.BillId, cancellationToken);

        if (bill is null)
            return Result.Failure<BillDto>(Error.NotFound(nameof(Bill), request.BillId));

        return Result.Success(bill.Adapt<BillDto>());
    }
}
