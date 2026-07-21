using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;

namespace Obss.AAA.Application.Queries.GetSessionsByUser;



internal sealed class GetSessionsByUserQueryHandler : IRequestHandler<GetSessionsByUserQuery, IReadOnlyList<RadiusSessionDto>>
{
    private readonly IRadiusSessionRepository _repository;

    public GetSessionsByUserQueryHandler(IRadiusSessionRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<RadiusSessionDto>> Handle(GetSessionsByUserQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repository.GetByUsernameAsync(request.Username, cancellationToken);
        return sessions.Adapt<IReadOnlyList<RadiusSessionDto>>();
    }
}
