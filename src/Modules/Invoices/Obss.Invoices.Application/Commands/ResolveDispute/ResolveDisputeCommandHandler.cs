using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.ResolveDispute;

public sealed class ResolveDisputeCommandHandler : IRequestHandler<ResolveDisputeCommand, Result>
{
    private readonly IInvoiceDisputeRepository _disputeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResolveDisputeCommandHandler> _logger;

    public ResolveDisputeCommandHandler(
        IInvoiceDisputeRepository disputeRepository,
        IUnitOfWork unitOfWork,
        ILogger<ResolveDisputeCommandHandler> logger)
    {
        _disputeRepository = disputeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ResolveDisputeCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _disputeRepository.GetByIdAsync(request.DisputeId, cancellationToken);

        if (dispute is null)
            return Result.Failure(Error.NotFound("InvoiceDispute", request.DisputeId));

        dispute.AcceptResolution(request.Resolution, request.ResolvedBy);

        await _disputeRepository.UpdateAsync(dispute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Dispute {DisputeId} resolved by {ResolvedBy}", request.DisputeId, request.ResolvedBy);

        return Result.Success();
    }
}
