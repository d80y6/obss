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

        _logger.LogInformation("Creating DNS {RecordType} record for {Domain}", recordType, domain);

        var zoneId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        return Task.FromResult(ProvisioningResult.Ok(
            JsonSerializer.Serialize(new
            {
                domain,
                recordType,
                zoneId,
                recordId,
                ttl = 300,
                dnsServer = "ns1.obss.example.com",
                status = "active"
            }),
            TimeSpan.FromMilliseconds(80)));
    }

    public Task<ProvisioningResult> CompensateAsync(ProvisioningTask task, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DnsSetupAdapter removing DNS records for task {TaskId}", task.Id);
        return Task.FromResult(ProvisioningResult.Ok(
            JsonSerializer.Serialize(new { recordsRemoved = true }),
            TimeSpan.FromMilliseconds(30)));
    }
}
