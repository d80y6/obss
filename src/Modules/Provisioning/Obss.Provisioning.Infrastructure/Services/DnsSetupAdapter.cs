using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Domain.Entities;
using Obss.Provisioning.Domain.ValueObjects;


namespace Obss.Provisioning.Infrastructure.Services;

public sealed class DnsSetupAdapter : IProvisioningAdapter
{
    private readonly ILogger<DnsSetupAdapter> _logger;

    public DnsSetupAdapter(ILogger<DnsSetupAdapter> logger)
    {
        _logger = logger;
    }

    public string AdapterName => "DnsAdapter";

    public Task<ProvisioningResult> ExecuteAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DnsSetupAdapter executing task {TaskId}", task.Id);

        if (task.TaskType != ProvisioningTaskType.DNSSetup)
        {
            return Task.FromResult(ProvisioningResult.Fail($"Unsupported task type: {task.TaskType}"));
        }

        var config = task.Configuration is not null
            ? JsonSerializer.Deserialize<JsonElement>(task.Configuration!.RootElement.GetRawText())
            : new JsonElement();

        var domain = config.TryGetProperty("domain", out var d) ? d.GetString() : "example.com";
        var recordType = config.TryGetProperty("recordType", out var rt) ? rt.GetString() : "A";

        _logger.LogInformation("DNS {RecordType} record for {Domain} created successfully (simulated)", recordType, domain);

        var resultData = JsonSerializer.Serialize(new
        {
            domain,
            recordType = recordType ?? "A",
            ttl = 3600,
            status = "simulated"
        });

        return Task.FromResult(ProvisioningResult.Ok(resultData));
    }

    public Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DnsSetupAdapter removing DNS records for task {TaskId} (simulated)", task.Id);

        var resultData = JsonSerializer.Serialize(new
        {
            taskId = task.Id.ToString(),
            status = "simulated"
        });

        return Task.FromResult(ProvisioningResult.Ok(resultData));
    }
}
