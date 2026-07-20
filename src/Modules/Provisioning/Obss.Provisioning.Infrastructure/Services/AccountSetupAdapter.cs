using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;

namespace Obss.Provisioning.Infrastructure.Services;

public sealed class AccountSetupAdapter : IProvisioningAdapter
{
    private readonly ILogger<AccountSetupAdapter> _logger;

    public AccountSetupAdapter(ILogger<AccountSetupAdapter> logger)
    {
        _logger = logger;
    }

    public string AdapterName => "AccountAdapter";

    public Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AccountSetupAdapter executing task {TaskId} of type {TaskType}",
            task.Id, task.TaskType);

        return task.TaskType switch
        {
            ProvisioningTaskType.AccountSetup => SetupBillingAccountAsync(task),
            ProvisioningTaskType.EmailNotification => SendWelcomeEmailAsync(task),
            _ => Task.FromResult(ProvisioningResult.Fail($"Unsupported task type for AccountAdapter: {task.TaskType}"))
        };
    }

    public Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("AccountSetupAdapter compensating task {TaskId}", task.Id);

        return task.TaskType switch
        {
            ProvisioningTaskType.AccountSetup => Task.FromResult(ProvisioningResult.Blocked(
                "Account closure requires real billing system - adapter cannot compensate",
                TimeSpan.FromMilliseconds(40))),
            _ => Task.FromResult(ProvisioningResult.Ok(
                JsonSerializer.Serialize(new { compensated = true })))
        };
    }

    private Task<ProvisioningResult> SetupBillingAccountAsync(ProvisioningTask task)
    {
        var config = task.Configuration is not null
            ? JsonSerializer.Deserialize<JsonElement>(task.Configuration!.RootElement.GetRawText())
            : new JsonElement();

        var customerName = config.TryGetProperty("customerName", out var cn) ? cn.GetString() : "Unknown";
        var plan = config.TryGetProperty("plan", out var p) ? p.GetString() : "standard";

        _logger.LogInformation("Billing account creation for {Customer} on {Plan} plan requires billing system", customerName, plan);

        return Task.FromResult(ProvisioningResult.Blocked(
            $"Billing account creation for {customerName} on {plan} plan requires real billing system integration",
            TimeSpan.FromMilliseconds(120)));
    }

    private Task<ProvisioningResult> SendWelcomeEmailAsync(ProvisioningTask task)
    {
        var config = task.Configuration is not null
            ? JsonSerializer.Deserialize<JsonElement>(task.Configuration!.RootElement.GetRawText())
            : new JsonElement();

        var email = config.TryGetProperty("email", out var e) ? e.GetString() : "customer@example.com";

        _logger.LogInformation("Sending welcome email to {Email}", email);

        return Task.FromResult(ProvisioningResult.Ok(
            JsonSerializer.Serialize(new
            {
                email,
                template = "welcome_v2",
                sentAt = DateTime.UtcNow,
                trackingId = Guid.NewGuid()
            }),
            TimeSpan.FromMilliseconds(50)));
    }
}
