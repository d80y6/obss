using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.AddAdjustment;

public sealed class AddAdjustmentCommandHandler : IRequestHandler<AddAdjustmentCommand, Result>
{
    private readonly IBillRepository _billRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddAdjustmentCommandHandler> _logger;

    public AddAdjustmentCommandHandler(
        IBillRepository billRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddAdjustmentCommandHandler> logger)
    {
        _billRepository = billRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(AddAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var bill = await _billRepository.GetByIdWithLinesAsync(request.BillId, cancellationToken);

        if (bill is null)
            return Result.Failure(Error.NotFound(nameof(Bill), request.BillId));

        var line = BillLine.CreateAdjustment(
            request.BillId,
            request.Description,
            request.Amount,
            request.Currency,
            DateTime.UtcNow);

        bill.AddLine(line);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Adjustment of {Amount} {Currency} added to bill {BillId}",
            request.Amount,
            request.Currency,
            request.BillId);

        return Result.Success();
    }
}
