using Mapster;
using MediatR;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Queries.GetDisputeById;

public sealed class GetDisputeByIdQueryHandler : IRequestHandler<GetDisputeByIdQuery, Result<InvoiceDisputeDto>>
{
    private readonly IInvoiceDisputeRepository _disputeRepository;

    public GetDisputeByIdQueryHandler(IInvoiceDisputeRepository disputeRepository)
    {
        _disputeRepository = disputeRepository;
    }

    public async Task<Result<InvoiceDisputeDto>> Handle(GetDisputeByIdQuery request, CancellationToken cancellationToken)
    {
        var dispute = await _disputeRepository.GetByIdWithAttachmentsAsync(request.DisputeId, cancellationToken);

        if (dispute is null)
            return Result.Failure<InvoiceDisputeDto>(Error.NotFound("InvoiceDispute", request.DisputeId));

        return Result.Success(dispute.Adapt<InvoiceDisputeDto>());
    }
}
