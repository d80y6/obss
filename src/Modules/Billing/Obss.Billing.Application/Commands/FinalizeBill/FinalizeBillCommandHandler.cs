using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.FinalizeBill;

public sealed class FinalizeBillCommandHandler : IRequestHandler<FinalizeBillCommand, Result>
{
    private readonly IBillRepository _billRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FinalizeBillCommandHandler> _logger;

    public FinalizeBillCommandHandler(
        IBillRepository billRepository,
        IUnitOfWork unitOfWork,
        ILogger<FinalizeBillCommandHandler> logger)
    {
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(FinalizeBillCommand request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdWithLinesAsync(request.BillId, cancellationToken);

        if (bill is null)
            return Result.Failure(Error.NotFound(nameof(Bill), request.BillId));

        bill.MarkAsFinalized();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Bill {BillId} finalized for customer {CustomerId} with grand total {GrandTotal}",
            bill.Id,
            bill.CustomerId,
            bill.GrandTotal);

        return Result.Success();
    }
}
