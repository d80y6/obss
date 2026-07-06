using FluentValidation;
using MediatR;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Commands;

public sealed record DeletePartyRoleCommand(Guid Id) : IRequest<Result<bool>>;

public sealed class DeletePartyRoleCommandValidator : AbstractValidator<DeletePartyRoleCommand>
{
    public DeletePartyRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class DeletePartyRoleCommandHandler : IRequestHandler<DeletePartyRoleCommand, Result<bool>>
{
    private readonly IRepository<PartyRole> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePartyRoleCommandHandler(IRepository<PartyRole> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeletePartyRoleCommand request, CancellationToken cancellationToken)
    {
        var partyRole = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (partyRole is null)
            return Result.Failure<bool>(Error.NotFound(nameof(PartyRole), request.Id));

        await _repository.DeleteAsync(partyRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
