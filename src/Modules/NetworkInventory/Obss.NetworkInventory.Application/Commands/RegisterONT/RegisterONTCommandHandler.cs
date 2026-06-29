using MediatR;
using Obss.NetworkInventory.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.RegisterONT;

public sealed class RegisterONTCommandHandler : IRequestHandler<RegisterONTCommand, Result>
{
    private readonly IRepository<OLT> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterONTCommandHandler(IRepository<OLT> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegisterONTCommand request, CancellationToken cancellationToken)
    {
        var olt = await _repository.GetByIdAsync(request.OLTId, cancellationToken);

        if (olt is null)
            return Result.Failure(Error.NotFound("OLT", request.OLTId));

        olt.RegisterONT(request.PortNumber);

        await _repository.UpdateAsync(olt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
