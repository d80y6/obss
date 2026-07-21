using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.EventManagement.Application.Abstractions;
using Obss.EventManagement.Application.DTOs;
using Obss.EventManagement.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.EventManagement.Application.Commands.PublishEvent;

public sealed record PublishEventCommand(
    string EventType,
    string Payload,
    string? Source) : IRequest<Result<EventDto>>;

public sealed class PublishEventCommandValidator : AbstractValidator<PublishEventCommand>
{
    public PublishEventCommandValidator()
    {
        RuleFor(x => x.EventType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Payload).NotEmpty();
    }
}

public sealed class PublishEventCommandHandler : IRequestHandler<PublishEventCommand, Result<EventDto>>
{
    private readonly IWebhookEventRepository _repository;
    private readonly IEventSubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PublishEventCommandHandler> _logger;

    public PublishEventCommandHandler(
        IWebhookEventRepository repository,
        IEventSubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        ILogger<PublishEventCommandHandler> logger)
    {
        _repository = repository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<EventDto>> Handle(PublishEventCommand request, CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetActiveSubscriptionsAsync(cancellationToken);

        var matchingSubscriptions = subscriptions
            .Where(s => s.Filters.Count == 0 || s.Filters.Any(f => f.EventType == request.EventType))
            .ToList();

        if (matchingSubscriptions.Count == 0)
        {
            _logger.LogWarning(
                "No active subscriptions found for event type {EventType}. Event will not be delivered.",
                request.EventType);

            return Result.Success(new EventDto(
                Guid.Empty,
                request.EventType,
                request.Payload,
                DateTime.UtcNow));
        }

        foreach (var sub in matchingSubscriptions)
        {
            var webhookEvent = new WebhookEvent(
                Guid.NewGuid(),
                sub.Id,
                request.EventType,
                request.Payload,
                "pending");

            await _repository.AddAsync(webhookEvent, cancellationToken);

            _logger.LogDebug(
                "Created webhook event {EventId} for subscription {SubscriptionId} ({Name})",
                webhookEvent.Id, sub.Id, sub.Name);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Published event {EventType} to {Count} matching subscription(s)",
            request.EventType, matchingSubscriptions.Count);

        return Result.Success(new EventDto(
            Guid.Empty,
            request.EventType,
            request.Payload,
            DateTime.UtcNow));
    }
}
