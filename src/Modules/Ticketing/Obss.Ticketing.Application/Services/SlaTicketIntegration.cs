using Microsoft.Extensions.Logging;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Ticketing.Application.Services;

public sealed class SlaTicketIntegration : ISlaTicketIntegration
{
    private readonly ISlaDefinitionRepository _slaDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SlaTicketIntegration> _logger;

    public SlaTicketIntegration(
        ISlaDefinitionRepository slaDefinitionRepository,
        IUnitOfWork unitOfWork,
        ILogger<SlaTicketIntegration> logger)
    {
        _slaDefinitionRepository = slaDefinitionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LinkSlaToTicketAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        var slaDefinitions = await _slaDefinitionRepository.GetAllAsync(cancellationToken);
        var matchingSla = slaDefinitions
            .Where(s => s.IsActive && s.Priority == ticket.Priority)
            .MaxBy(s => s.ResolutionTimeHours);

        if (matchingSla is null)
        {
            _logger.LogWarning(
                "No active SLA definition found for ticket {TicketNumber} with priority {Priority}",
                ticket.TicketNumber, ticket.Priority);

            return;
        }

        ticket.ApplySlaDefinition(matchingSla);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "SLA {SlaName} (ID: {SlaId}) linked to ticket {TicketNumber}. Response: {Response}h, Resolution: {Resolution}h",
            matchingSla.Name, matchingSla.Id, ticket.TicketNumber,
            matchingSla.ResponseTimeHours, matchingSla.ResolutionTimeHours);
    }

    public Task<SlaStatusResult> GetSlaStatusAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        if (!ticket.SlaDeadline.HasValue)
        {
            return Task.FromResult(new SlaStatusResult(false, null, null));
        }

        var remaining = ticket.SlaDeadline.Value - DateTime.UtcNow;
        var breached = ticket.IsSlaBreached || remaining <= TimeSpan.Zero;

        return Task.FromResult(new SlaStatusResult(
            breached,
            breached ? TimeSpan.Zero : remaining,
            ticket.SlaDeadline));
    }

    public async Task EscalateOnSlaBreachAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        var status = await GetSlaStatusAsync(ticket, cancellationToken);

        if (!status.SlaBreached)
        {
            return;
        }

        if (ticket.IsSlaBreached)
        {
            _logger.LogWarning(
                "SLA already breached for ticket {TicketNumber}. Deadline was: {SlaDeadline}",
                ticket.TicketNumber, ticket.SlaDeadline);

            return;
        }

        ticket.Escalate("SLA Monitor", $"SLA deadline missed. Deadline was: {ticket.SlaDeadline:O}");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Ticket {TicketNumber} escalated due to SLA breach. Deadline: {SlaDeadline}",
            ticket.TicketNumber, ticket.SlaDeadline);
    }
}
