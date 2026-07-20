using FluentValidation;
using Mapster;
using MediatR;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Application.DTOs;
using Obss.EventManagement.Domain.Entities;
using Obss.EventManagement.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.EventManagement.Application.Commands;

public sealed record CreateSubscriptionCommand(
    string Name,
    string CallbackUrl,
    string? SigningSecret,
    string? Query,
    string? Description,
    IReadOnlyList<EventFilterDto>? Filters) : IRequest<Result<EventSubscriptionDto>>;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CallbackUrl).NotEmpty().MaximumLength(2048);
    }
}

public sealed class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, Result<EventSubscriptionDto>>
{
    private readonly IEventSubscriptionRepository _repository;

    public CreateSubscriptionCommandHandler(IEventSubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<EventSubscriptionDto>> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = EventSubscription.Create(
            request.Name,
            request.CallbackUrl,
            request.SigningSecret,
            request.Query,
            request.Description);

        if (request.Filters is not null)
        {
            foreach (var filter in request.Filters)
            {
                subscription.AddFilter(new EventFilter(filter.EventType, filter.FilterCriteria));
            }
        }

        await _repository.AddAsync(subscription, cancellationToken);
        return Result.Success(subscription.Adapt<EventSubscriptionDto>());
    }
}
