using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Invoices.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Invoices.Application.Commands.RejectDispute;

public sealed class RejectDisputeCommandHandler : IRequestHandler<RejectDisputeCommand, Result>
{
    private readonly IInvoiceDisputeRepository _disputeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectDisputeCommandHandler> _logger;

    public RejectDisputeCommandHandler(
        IInvoiceDisputeRepository disputeRepository,
        IUnitOfWork unitOfWork,
        ILogger<RejectDisputeCommandHandler> logger)
    {
        _disputeRepository = disputeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RejectDisputeCommand request, CancellationToken cancellationToken)
    {
        var dispute = await _disputeRepository.GetByIdAsync(request.DisputeId, cancellationToken);

        if (dispute is null)
            return Result.Failure(Error.NotFound("InvoiceDispute", request.DisputeId));

        dispute.RejectResolution(request.Reason);

        await _disputeRepository.UpdateAsync(dispute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Dispute {DisputeId} rejected", request.DisputeId);

        return Result.Success();
    }
}
