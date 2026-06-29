using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.Audit.Domain.Entities;
using Obss.Audit.Domain.ValueObjects;
using Obss.Audit.Infrastructure.Persistence;

namespace Obss.Audit.Infrastructure.BackgroundJobs;

public sealed class AuditAlertDetectionJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditAlertDetectionJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(1);

    public AuditAlertDetectionJob(IServiceScopeFactory scopeFactory, ILogger<AuditAlertDetectionJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit alert detection job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

                await DetectAlertsAsync(context, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during audit alert detection.");
            }
        }

        _logger.LogInformation("Audit alert detection job stopped.");
    }

    private async Task DetectAlertsAsync(AuditDbContext context, CancellationToken cancellationToken)
    {
        var rules = await context.Set<AuditAlertRule>()
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        if (rules.Count == 0)
            return;

        foreach (var rule in rules)
        {
            await EvaluateRuleAsync(context, rule, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EvaluateRuleAsync(AuditDbContext context, AuditAlertRule rule, CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.AddMinutes(-rule.WindowMinutes);

        var matchingAlertType = rule.AlertType switch
        {
            AlertType.FailedLogin => AuditAction.FailedLogin,
            AlertType.MassExport => AuditAction.Exported,
            AlertType.PermissionChange => (AuditAction?)null,
            AlertType.SensitiveDataAccess => AuditAction.Viewed,
            AlertType.DataDeletion => AuditAction.Deleted,
            AlertType.AnomalousActivity => (AuditAction?)null,
            AlertType.RetentionBreach => (AuditAction?)null,
            _ => null
        };

        var actionFilter = matchingAlertType?.ToString();
        if (actionFilter is null && rule.AlertType != AlertType.PermissionChange
                                && rule.AlertType != AlertType.AnomalousActivity
                                && rule.AlertType != AlertType.RetentionBreach)
        {
            return;
        }

        var entryCount = await CountMatchingEntries(context, rule, since, actionFilter, cancellationToken);

        if (entryCount < rule.Threshold)
            return;

        var existingAlertCount = await context.Set<AuditAlert>()
            .CountAsync(a =>
                a.TenantId == rule.TenantId &&
                a.AlertType == rule.AlertType &&
                a.DetectedAt >= since,
                cancellationToken);

        if (existingAlertCount > 0)
            return;

        var alert = AuditAlert.Create(
            rule.TenantId,
            rule.Severity,
            rule.AlertType,
            $"Threshold exceeded: {entryCount} occurrences of '{rule.AlertType}' in {rule.WindowMinutes} minutes (threshold: {rule.Threshold}).");

        await context.Set<AuditAlert>().AddAsync(alert, cancellationToken);

        _logger.LogWarning(
            "Alert triggered: {AlertType} (Severity: {Severity}) - {Count} occurrences in {WindowMinutes} min",
            rule.AlertType, rule.Severity, entryCount, rule.WindowMinutes);
    }

    private static async Task<int> CountMatchingEntries(
        AuditDbContext context,
        AuditAlertRule rule,
        DateTime since,
        string? actionFilter,
        CancellationToken cancellationToken)
    {
        var query = context.Set<AuditEntry>()
            .Where(e => e.TenantId == rule.TenantId && e.PerformedAt >= since);

        switch (rule.AlertType)
        {
            case AlertType.FailedLogin:
            case AlertType.MassExport:
            case AlertType.DataDeletion:
                query = query.Where(e => e.Action.ToString() == actionFilter);
                break;

            case AlertType.PermissionChange:
                query = query.Where(e =>
                    (e.Action.ToString() == AuditAction.Updated.ToString() ||
                     e.Action.ToString() == AuditAction.Created.ToString() ||
                     e.Action.ToString() == AuditAction.Deleted.ToString()) &&
                    e.EntityType == "Permission");
                break;

            case AlertType.SensitiveDataAccess:
                query = query.Where(e => e.IsSensitive && e.Action.ToString() == actionFilter);
                break;

            case AlertType.AnomalousActivity:
            case AlertType.RetentionBreach:
                query = query.Where(e => false);
                break;
        }

        return await query.CountAsync(cancellationToken);
    }
}
