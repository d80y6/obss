using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;

namespace Obss.AAA.Application.Queries.GetSessionById;



internal sealed class GetSessionByIdQueryHandler : IRequestHandler<GetSessionByIdQuery, RadiusSessionDto?>
{
    private readonly IRadiusSessionRepository _repository;

    public GetSessionByIdQueryHandler(IRadiusSessionRepository repository) => _repository = repository;

    public async Task<RadiusSessionDto?> Handle(GetSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return session?.Adapt<RadiusSessionDto>();
    }
}
