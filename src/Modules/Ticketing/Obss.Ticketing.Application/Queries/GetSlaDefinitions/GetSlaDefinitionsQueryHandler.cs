using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Queries.GetSlaDefinitions;

public sealed class GetSlaDefinitionsQueryHandler : IRequestHandler<GetSlaDefinitionsQuery, Result<IReadOnlyList<SlaDefinitionDto>>>
{
    private readonly ISlaDefinitionRepository _slaDefinitionRepository;

    public GetSlaDefinitionsQueryHandler(ISlaDefinitionRepository slaDefinitionRepository)
    {
        _slaDefinitionRepository = slaDefinitionRepository;
    }

    public async Task<Result<IReadOnlyList<SlaDefinitionDto>>> Handle(GetSlaDefinitionsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.SlaDefinition> slaDefinitions;

        if (!string.IsNullOrWhiteSpace(request.TenantId))
        {
            slaDefinitions = await _slaDefinitionRepository.GetActiveByTenantAsync(request.TenantId, cancellationToken);
        }
        else
        {
            slaDefinitions = await _slaDefinitionRepository.GetAllAsync(cancellationToken);
        }

        var result = slaDefinitions.Adapt<List<SlaDefinitionDto>>();
        return Result.Success<IReadOnlyList<SlaDefinitionDto>>(result);
    }
}
