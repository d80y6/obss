using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Domain.ValueObjects;

namespace Obss.Collections.Application.BackgroundJobs;

public sealed class DunningProcessingJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DunningProcessingJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public DunningProcessingJob(
        IServiceProvider serviceProvider,
        ILogger<DunningProcessingJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dunning processing job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDunning(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing dunning.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task ProcessDunning(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var overdueQuery = scope.ServiceProvider.GetRequiredService<IOverdueInvoiceQuery>();
        var caseRepository = scope.ServiceProvider.GetRequiredService<ICollectionCaseRepository>();
        var policyRepository = scope.ServiceProvider.GetRequiredService<IDunningPolicyRepository>();

        var overdueInvoices = await overdueQuery.GetOverdueInvoicesAsync(cancellationToken);
        _logger.LogInformation("Found {Count} overdue invoices.", overdueInvoices.Count);

        var policy = await policyRepository.GetActivePolicyAsync(cancellationToken);

        var groupedByCustomer = overdueInvoices
            .GroupBy(i => i.CustomerId)
            .ToList();

        foreach (var customerGroup in groupedByCustomer)
        {
            try
            {
                var customerId = customerGroup.Key;
                var customerName = customerGroup.First().CustomerName;
                var totalOverdue = customerGroup.Sum(i => i.AmountDue);
                var currency = customerGroup.First().Currency;
                var maxDaysOverdue = customerGroup.Max(i => i.DaysOverdue);

                var existingCase = await caseRepository.GetByCustomerWithActiveArrangementAsync(customerId, cancellationToken);

                if (existingCase is null)
                {
                    var @case = Domain.Entities.CollectionCase.Open(
                        string.Empty,
                        customerId,
                        customerName,
                        totalOverdue,
                        currency);

                    await caseRepository.AddAsync(@case, cancellationToken);
                    existingCase = @case;

                    _logger.LogInformation(
                        "Opened collection case {CaseId} for customer {CustomerId}.",
                        @case.Id,
                        customerId);
                }
                else
                {
                    existingCase.UpdateOverdueAmount(totalOverdue);
                }

                if (policy is not null)
                {
                    var expectedLevel = CalculateDunningLevel(maxDaysOverdue, policy);
                    while (existingCase.CurrentDunningLevel < expectedLevel &&
                           existingCase.CurrentDunningLevel < policy.MaxDunningLevel)
                    {
                        var nextLevel = existingCase.CurrentDunningLevel + 1;
                        var levelNames = new Dictionary<int, string>
                        {
                            { 1, "1st Notice" },
                            { 2, "2nd Notice" },
                            { 3, "Final Notice" },
                            { 4, "Collection Agency Referral" },
                            { 5, "Legal Action Notice" }
                        };

                        var noticeName = levelNames.GetValueOrDefault(nextLevel, $"Dunning Level {nextLevel}");
                        var fee = policy.GetFeeForLevel(nextLevel);

                        var action = Domain.Entities.CollectionAction.Create(
                            existingCase.Id,
                            CollectionActionType.DunningNotice,
                            nextLevel,
                            $"{noticeName} sent automatically. {(fee > 0 ? $"Dunning fee of {fee} applied." : "")}",
                            "System",
                            DateTime.UtcNow.AddDays(policy.DaysBetweenActions));

                        existingCase.AddAction(action);
                        existingCase.AdvanceDunningLevel();

                        if (fee > 0)
                        {
                            existingCase.UpdateOverdueAmount(existingCase.TotalOverdueAmount + fee);
                        }

                        _logger.LogInformation(
                            "Auto-sent dunning notice level {Level} for case {CaseId}.",
                            nextLevel,
                            existingCase.Id);
                    }
                }

                foreach (var pa in existingCase.PaymentArrangements)
                {
                    pa.MarkOverdueInstallments();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing dunning for customer {CustomerId}.", customerGroup.Key);
            }
        }
    }

    private static int CalculateDunningLevel(int daysOverdue, Domain.Entities.DunningPolicy policy)
    {
        if (policy.EscalationAfterDays <= 0)
            return 0;

        var level = daysOverdue / policy.EscalationAfterDays;
        return Math.Min(level, policy.MaxDunningLevel);
    }
}
