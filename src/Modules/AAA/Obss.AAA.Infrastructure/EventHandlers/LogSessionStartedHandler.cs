using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Infrastructure.EventHandlers;

public sealed class LogSessionStartedHandler : INotificationHandler<RadiusSessionStartedDomainEvent>
{
    private readonly IAaaAuditLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public LogSessionStartedHandler(
        IAaaAuditLogRepository logRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task Handle(RadiusSessionStartedDomainEvent notification, CancellationToken cancellationToken)
    {
        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "AccountingStart",
            notification.Username,
            notification.NasId,
            detail: $"{{\"sessionId\":\"{notification.SessionId}\",\"radiusSessionId\":\"{notification.RadiusSessionId}\"}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
