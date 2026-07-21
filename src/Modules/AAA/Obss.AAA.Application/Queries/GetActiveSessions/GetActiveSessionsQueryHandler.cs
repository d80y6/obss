using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;

namespace Obss.AAA.Application.Queries.GetActiveSessions;



internal sealed class GetActiveSessionsQueryHandler : IRequestHandler<GetActiveSessionsQuery, IReadOnlyList<RadiusSessionDto>>
{
    private readonly IRadiusSessionRepository _repository;

    public GetActiveSessionsQueryHandler(IRadiusSessionRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<RadiusSessionDto>> Handle(GetActiveSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repository.GetActiveSessionsAsync(cancellationToken);
        return sessions.Adapt<IReadOnlyList<RadiusSessionDto>>();
    }
}
