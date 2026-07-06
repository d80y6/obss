using FluentValidation;
using Mapster;
using MediatR;
using Obss.IAM.Application.Abstractions;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Application.Commands;

public sealed record CreatePartyRoleCommand(
    Guid PartyId,
    Guid RoleId,
    string Name,
    string? Description,
    DateTime? ValidFrom,
    DateTime? ValidUntil) : IRequest<Result<PartyRoleDto>>;

public sealed class CreatePartyRoleCommandValidator : AbstractValidator<CreatePartyRoleCommand>
{
    public CreatePartyRoleCommandValidator()
    {
        RuleFor(x => x.PartyId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public sealed class CreatePartyRoleCommandHandler : IRequestHandler<CreatePartyRoleCommand, Result<PartyRoleDto>>
{
    private readonly IRepository<PartyRole> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePartyRoleCommandHandler(IRepository<PartyRole> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PartyRoleDto>> Handle(CreatePartyRoleCommand request, CancellationToken cancellationToken)
    {
        var partyRole = PartyRole.Create(
            request.PartyId,
            request.RoleId,
            request.Name,
            request.Description,
            request.ValidFrom,
            request.ValidUntil);

        await _repository.AddAsync(partyRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(partyRole.Adapt<PartyRoleDto>());
    }
}
