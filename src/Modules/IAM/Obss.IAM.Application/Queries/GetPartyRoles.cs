using Mapster;
using MediatR;
using Obss.IAM.Application.Contracts;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.IAM.Application.Queries;

public sealed record GetPartyRolesQuery(
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<PartyRoleDto>>>;

public sealed class GetPartyRolesQueryHandler : IRequestHandler<GetPartyRolesQuery, Result<PaginatedResult<PartyRoleDto>>>
{
    private readonly IRepository<PartyRole> _repository;

    public GetPartyRolesQueryHandler(IRepository<PartyRole> repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<PartyRoleDto>>> Handle(GetPartyRolesQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(request.Offset, request.Limit, cancellationToken);
        var dtos = items.Adapt<List<PartyRoleDto>>();
        return Result.Success(new PaginatedResult<PartyRoleDto>(dtos, totalCount));
    }
}
