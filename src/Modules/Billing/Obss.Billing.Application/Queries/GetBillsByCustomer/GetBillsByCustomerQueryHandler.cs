using Mapster;
using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillsByCustomer;

public sealed class GetBillsByCustomerQueryHandler : IRequestHandler<GetBillsByCustomerQuery, Result<IReadOnlyList<BillDto>>>
{
    private readonly IBillRepository _billRepository;

    public GetBillsByCustomerQueryHandler(IBillRepository billRepository)
    {
        _billRepository = billRepository;
    }

    public async Task<Result<IReadOnlyList<BillDto>>> Handle(GetBillsByCustomerQuery request, CancellationToken cancellationToken)
    {
        BillStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            if (!Enum.TryParse<BillStatus>(request.Status, true, out var parsedStatus))
            {
                return Result.Failure<IReadOnlyList<BillDto>>(
                    Error.Validation($"Invalid bill status: '{request.Status}'."));
            }
            status = parsedStatus;
        }

        var bills = await _billRepository.GetByCustomerAsync(
            request.CustomerId,
            status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success<IReadOnlyList<BillDto>>(bills.Adapt<List<BillDto>>());
    }
}
