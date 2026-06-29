using Mapster;
using MediatR;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetInvoiceDisputes;

public sealed class GetInvoiceDisputesQueryHandler : IRequestHandler<GetInvoiceDisputesQuery, Result<IReadOnlyList<InvoiceDisputeDto>>>
{
    private readonly IInvoiceDisputeRepository _disputeRepository;

    public GetInvoiceDisputesQueryHandler(IInvoiceDisputeRepository disputeRepository)
    {
        _disputeRepository = disputeRepository;
    }

    public async Task<Result<IReadOnlyList<InvoiceDisputeDto>>> Handle(GetInvoiceDisputesQuery request, CancellationToken cancellationToken)
    {
        var disputes = await _disputeRepository.GetDisputesAsync(request.InvoiceId, request.Status, cancellationToken);
        return Result.Success(disputes.Adapt<IReadOnlyList<InvoiceDisputeDto>>());
    }
}
