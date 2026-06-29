using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Obss.CRM.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.CRM.Application.BackgroundJobs;

public sealed class CustomerSegmentationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CustomerSegmentationJob> _logger;
    private static readonly TimeSpan EvaluationInterval = TimeSpan.FromHours(1);

    public CustomerSegmentationJob(IServiceScopeFactory scopeFactory, ILogger<CustomerSegmentationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Customer Segmentation Job started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await EvaluateSegmentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while evaluating customer segments.");
            }

            await Task.Delay(EvaluationInterval, stoppingToken);
        }
    }

    private async Task EvaluateSegmentsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var segmentRepository = scope.ServiceProvider.GetRequiredService<ICustomerSegmentRepository>();
        var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var activeSegments = await segmentRepository.GetAllActiveAsync(cancellationToken);
        var allCustomers = await customerRepository.GetAllAsync(cancellationToken);

        foreach (var segment in activeSegments)
        {
            var criteria = segment.Criteria;
            if (criteria is null || criteria.RuleGroups.Count == 0)
                continue;

            foreach (var customer in allCustomers)
            {
                var matches = EvaluateCustomerAgainstCriteria(customer, criteria);
                if (matches)
                {
                    segment.AddCustomer(customer.Id, Guid.Empty, true);
                }
            }

            await segmentRepository.UpdateAsync(segment, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool EvaluateCustomerAgainstCriteria(Domain.Entities.Customer customer, Domain.ValueObjects.SegmentCriteria criteria)
    {
        foreach (var group in criteria.RuleGroups)
        {
            var groupResult = group.Conjunction != "Or";

            foreach (var rule in group.Rules)
            {
                var ruleResult = EvaluateRule(customer, rule);
                groupResult = group.Conjunction == "Or"
                    ? groupResult || ruleResult
                    : groupResult && ruleResult;
            }

            if (groupResult)
                return true;
        }

        return false;
    }

    private static bool EvaluateRule(Domain.Entities.Customer customer, Domain.ValueObjects.Rule rule)
    {
        var value = rule.Field.ToLowerInvariant() switch
        {
            "customer_type" => customer.CustomerType.ToString(),
            "status" => customer.Status.ToString(),
            "is_active" => customer.IsActive.ToString(),
            "credit_limit" => customer.CreditLimit.ToString(),
            "currency" => customer.Currency,
            _ => null
        };

        if (value is null)
            return false;

        return rule.Operator.ToLowerInvariant() switch
        {
            "equals" => string.Equals(value, rule.Value, StringComparison.OrdinalIgnoreCase),
            "not_equals" => !string.Equals(value, rule.Value, StringComparison.OrdinalIgnoreCase),
            "contains" => value.Contains(rule.Value, StringComparison.OrdinalIgnoreCase),
            "greater_than" => decimal.TryParse(value, out var v) && decimal.TryParse(rule.Value, out var rv) && v > rv,
            "less_than" => decimal.TryParse(value, out var v) && decimal.TryParse(rule.Value, out var rv) && v < rv,
            _ => false
        };
    }
}
