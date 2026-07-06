using FluentValidation;
using Mapster;
using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Commands;

public sealed record SuspendPartyRoleCommand(Guid Id) : IRequest<Result<PartyRoleDto>>;

public sealed class SuspendPartyRoleCommandValidator : AbstractValidator<SuspendPartyRoleCommand>
{
    public SuspendPartyRoleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class SuspendPartyRoleCommandHandler : IRequestHandler<SuspendPartyRoleCommand, Result<PartyRoleDto>>
{
    private readonly IRepository<PartyRole> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendPartyRoleCommandHandler(IRepository<PartyRole> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PartyRoleDto>> Handle(SuspendPartyRoleCommand request, CancellationToken cancellationToken)
    {
        var partyRole = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (partyRole is null)
            return Result.Failure<PartyRoleDto>(Error.NotFound(nameof(PartyRole), request.Id));

        partyRole.Suspend();
        await _repository.UpdateAsync(partyRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(partyRole.Adapt<PartyRoleDto>());
    }
}
