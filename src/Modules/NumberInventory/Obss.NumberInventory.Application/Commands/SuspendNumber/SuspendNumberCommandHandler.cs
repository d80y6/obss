using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.SuspendNumber;

public sealed class SuspendNumberCommandHandler : IRequestHandler<SuspendNumberCommand, Result>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SuspendNumberCommand request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.NumberId, cancellationToken);

        if (number is null)
            return Result.Failure(Error.NotFound("TelephoneNumber", request.NumberId));

        number.Suspend();

        await _numberRepository.UpdateAsync(number, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
