using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Application.Queries.GetBillById;
using Obss.Invoices.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Infrastructure.Persistence.Repositories;

public sealed class BillQuery : IBillQuery
{
    private readonly IMediator _mediator;

    public BillQuery(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<BillDto>> GetBillByIdAsync(Guid billId, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetBillByIdQuery(billId), cancellationToken);
        
        if (result.IsFailure)
        {
            return Result.Failure<BillDto>(result.Error);
        }

        return Result.Success(result.Value);
    }
}
