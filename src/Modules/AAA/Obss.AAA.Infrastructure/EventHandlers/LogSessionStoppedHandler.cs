using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.AAA.Infrastructure.EventHandlers;

public sealed class LogSessionStoppedHandler : INotificationHandler<RadiusSessionStoppedDomainEvent>
{
    private readonly IAaaAuditLogRepository _logRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public LogSessionStoppedHandler(
        IAaaAuditLogRepository logRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _logRepository = logRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task Handle(RadiusSessionStoppedDomainEvent notification, CancellationToken cancellationToken)
    {
        var log = AaaAuditLog.Create(
            _currentTenant.TenantId ?? string.Empty,
            "AccountingStop",
            notification.Username,
            notification.NasId,
            detail: $"{{\"sessionId\":\"{notification.SessionId}\",\"acctSessionTime\":{notification.AcctSessionTime},\"inputOctets\":{notification.InputOctets},\"outputOctets\":{notification.OutputOctets}}}");

        await _logRepository.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
