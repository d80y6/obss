using Mapster;
using MediatR;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetDunningPolicies;

public sealed class GetDunningPoliciesQueryHandler : IRequestHandler<GetDunningPoliciesQuery, Result<IReadOnlyList<DunningPolicyDto>>>
{
    private readonly IDunningPolicyRepository _repository;

    public GetDunningPoliciesQueryHandler(IDunningPolicyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<DunningPolicyDto>>> Handle(GetDunningPoliciesQuery request, CancellationToken cancellationToken)
    {
        var policies = await _repository.FindAsync(p =>
            !request.ActiveOnly.HasValue || (request.ActiveOnly.Value && p.IsActive),
            cancellationToken);

        return Result.Success<IReadOnlyList<DunningPolicyDto>>(policies.Adapt<List<DunningPolicyDto>>());
    }
}
