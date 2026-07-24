using Mapster;
using MediatR;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.DTOs;
using Obss.OCS.Domain.Entities;
using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.AdjustBalance;

internal sealed class AdjustBalanceCommandHandler : IRequestHandler<AdjustBalanceCommand, Result<BalanceDto>>
{
    private readonly IBalanceRepository _repository;
    private readonly IOcsTransactionRepository _transactionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
    private readonly ICurrentUser _currentUser;

    public AdjustBalanceCommandHandler(
        IBalanceRepository repository,
        IOcsTransactionRepository transactionRepo,
        IUnitOfWork unitOfWork,
        IAuditService auditService,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _transactionRepo = transactionRepo;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<BalanceDto>> Handle(AdjustBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = await _repository.GetByIdAsync(request.BalanceId, cancellationToken);
        if (balance is null)
            return Result.Failure<BalanceDto>(Error.NotFound("Balance", request.BalanceId));

        var beforeBalance = balance.AvailableAmount;

        switch (request.Direction.ToUpperInvariant())
        {
            case "CREDIT":
                balance.Credit(request.Amount);
                break;
            case "DEBIT":
                balance.Debit(request.Amount);
                break;
            default:
                return Result.Failure<BalanceDto>(Error.Validation($"Invalid direction '{request.Direction}'. Use CREDIT or DEBIT."));
        }

        await _repository.UpdateAsync(balance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var transaction = OcsTransaction.Create(
            balance.TenantId, balance.SubscriptionId, balance.Id, null,
            TransactionType.Adjustment, request.Amount, balance.Currency,
            $"Balance {request.Direction.ToUpperInvariant()} adjustment",
            beforeBalance: beforeBalance, afterBalance: balance.AvailableAmount);
        await _transactionRepo.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Balance", balance.Id.ToString(),
            string.Equals(request.Direction, "CREDIT", StringComparison.OrdinalIgnoreCase) ? "CreditAdjustment" : "DebitAdjustment",
            performedById: _currentUser.UserId, cancellationToken: cancellationToken);

        return Result.Success(balance.Adapt<BalanceDto>());
    }
}
