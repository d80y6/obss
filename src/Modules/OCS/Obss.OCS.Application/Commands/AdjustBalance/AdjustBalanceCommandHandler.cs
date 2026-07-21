using Mapster;
using MediatR;
using Obss.OCS.Application.Abstractions;
using Obss.OCS.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.OCS.Application.Commands.AdjustBalance;

internal sealed class AdjustBalanceCommandHandler : IRequestHandler<AdjustBalanceCommand, Result<BalanceDto>>
{
    private readonly IBalanceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustBalanceCommandHandler(IBalanceRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BalanceDto>> Handle(AdjustBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = await _repository.GetByIdAsync(request.BalanceId, cancellationToken);
        if (balance is null)
            return Result.Failure<BalanceDto>(Error.NotFound("Balance", request.BalanceId));

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

        return Result.Success(balance.Adapt<BalanceDto>());
    }
}
