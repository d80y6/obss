using Mapster;
using MediatR;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Application.Contracts;
using Obss.EventManagement.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.EventManagement.Application.Queries;

public sealed record GetSubscriptionsQuery(
    int Offset = 0,
    int Limit = 20) : IRequest<Result<PaginatedResult<EventSubscriptionDto>>>;

public sealed class GetSubscriptionsQueryHandler : IRequestHandler<GetSubscriptionsQuery, Result<PaginatedResult<EventSubscriptionDto>>>
{
    private readonly IEventSubscriptionRepository _repository;

    public GetSubscriptionsQueryHandler(IEventSubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PaginatedResult<EventSubscriptionDto>>> Handle(GetSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetPaginatedAsync(request.Offset, request.Limit, cancellationToken);
        var dtos = items.Adapt<List<EventSubscriptionDto>>();
        return Result.Success(new PaginatedResult<EventSubscriptionDto>(dtos, totalCount));
    }
}
