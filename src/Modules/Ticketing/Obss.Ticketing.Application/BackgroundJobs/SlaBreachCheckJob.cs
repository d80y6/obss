using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Ticketing.Application.Abstractions;

namespace Obss.Ticketing.Application.BackgroundJobs;

public sealed class SlaBreachCheckJob : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<SlaBreachCheckJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public SlaBreachCheckJob(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<SlaBreachCheckJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA Breach Check Job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForBreachesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking SLA breaches.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckForBreachesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var ticketRepository = scope.ServiceProvider.GetRequiredService<ITicketRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var breachedTickets = await ticketRepository.GetTicketsApproachingSlaBreachAsync(cancellationToken);

        foreach (var ticket in breachedTickets)
        {
            ticket.CheckSlaBreach();
            await ticketRepository.UpdateAsync(ticket, cancellationToken);
            _logger.LogWarning(
                "SLA breach detected for ticket {TicketNumber} (ID: {TicketId}). Deadline was: {SlaDeadline}",
                ticket.TicketNumber, ticket.Id, ticket.SlaDeadline);
        }

        if (breachedTickets.Count > 0)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
