using Mapster;
using MediatR;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetDunningPolicyById;

public sealed class GetDunningPolicyByIdQueryHandler : IRequestHandler<GetDunningPolicyByIdQuery, Result<DunningPolicyDto>>
{
    private readonly IDunningPolicyRepository _repository;

    public GetDunningPolicyByIdQueryHandler(IDunningPolicyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<DunningPolicyDto>> Handle(GetDunningPolicyByIdQuery request, CancellationToken cancellationToken)
    {
        var policy = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (policy is null)
            return Result.Failure<DunningPolicyDto>(Error.NotFound("DunningPolicy", request.Id));

        return Result.Success(policy.Adapt<DunningPolicyDto>());
    }
}
