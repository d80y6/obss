using Mapster;
using MediatR;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.DTOs;
using Obss.OCS.Domain.Entities;
using Obss.OCS.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.CreateBalance;

internal sealed class CreateBalanceCommandHandler : IRequestHandler<CreateBalanceCommand, Result<BalanceDto>>
{
    private readonly IBalanceRepository _repository;
    private readonly IOcsTransactionRepository _transactionRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public CreateBalanceCommandHandler(IBalanceRepository repository, IOcsTransactionRepository transactionRepo, IUnitOfWork unitOfWork, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _transactionRepo = transactionRepo;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<BalanceDto>> Handle(CreateBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = Balance.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.SubscriptionId,
            request.Currency);

        await _repository.AddAsync(balance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var transaction = OcsTransaction.Create(
            _currentTenant.TenantId ?? string.Empty, request.SubscriptionId, balance.Id, null,
            TransactionType.Recharge, 0, request.Currency,
            "Balance created");
        await _transactionRepo.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(balance.Adapt<BalanceDto>());
    }
}
