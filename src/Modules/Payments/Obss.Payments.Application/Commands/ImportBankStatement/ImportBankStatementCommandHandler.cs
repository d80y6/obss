using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Payments.Application.Abstractions;
using Obss.Payments.Application.DTOs;
using Obss.Payments.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.ImportBankStatement;

public sealed class ImportBankStatementCommandHandler : IRequestHandler<ImportBankStatementCommand, Result<PaymentReconciliationDto>>
{
    private readonly IPaymentReconciliationRepository _reconciliationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<ImportBankStatementCommandHandler> _logger;

    public ImportBankStatementCommandHandler(
        IPaymentReconciliationRepository reconciliationRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant,
        ILogger<ImportBankStatementCommandHandler> logger)
    {
        _reconciliationRepository = reconciliationRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<Result<PaymentReconciliationDto>> Handle(ImportBankStatementCommand request, CancellationToken cancellationToken)
    {
        var reconciliation = PaymentReconciliation.Create(
            _currentTenant.TenantId!,
            request.ImportSource,
            request.ImportFileName,
            0,
            request.Currency,
            _currentTenant.TenantId!);

        foreach (var line in request.Transactions)
        {
            var item = new ReconciliationItem(
                Guid.NewGuid(),
                reconciliation.Id,
                line.ExternalReference,
                line.Amount,
                request.Currency,
                line.TransactionDate,
                line.Description);

            reconciliation.AddItem(item);
        }

        await _reconciliationRepository.AddAsync(reconciliation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Bank statement imported: {Count} transactions, total {TotalAmount} {Currency}",
            request.Transactions.Count, reconciliation.TotalImportAmount, request.Currency);

        return Result.Success(reconciliation.Adapt<PaymentReconciliationDto>());
    }
}
