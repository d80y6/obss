using Mapster;
using MediatR;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Application.Contracts;
using Obss.EventManagement.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.EventManagement.Application.Queries;

public sealed record SearchEventsQuery(
    string? EventType,
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<EventDto>>>;

public sealed class SearchEventsQueryHandler : IRequestHandler<SearchEventsQuery, Result<PaginatedResult<EventDto>>>
{
    private readonly IWebhookEventRepository _repository;

    public SearchEventsQueryHandler(IWebhookEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<EventDto>>> Handle(SearchEventsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(request.Offset, request.Limit, cancellationToken);
        var dtos = items.Adapt<List<EventDto>>();
        return Result.Success(new PaginatedResult<EventDto>(dtos, totalCount));
    }
}
