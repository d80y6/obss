using Mapster;
using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Queries;

public sealed record GetPartyRoleByIdQuery(Guid Id) : IRequest<Result<PartyRoleDto>>;

public sealed class GetPartyRoleByIdQueryHandler : IRequestHandler<GetPartyRoleByIdQuery, Result<PartyRoleDto>>
{
    private readonly IRepository<PartyRole> _repository;

    public GetPartyRoleByIdQueryHandler(IRepository<PartyRole> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PartyRoleDto>> Handle(GetPartyRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var partyRole = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (partyRole is null)
            return Result.Failure<PartyRoleDto>(Error.NotFound(nameof(PartyRole), request.Id));

        return Result.Success(partyRole.Adapt<PartyRoleDto>());
    }
}
