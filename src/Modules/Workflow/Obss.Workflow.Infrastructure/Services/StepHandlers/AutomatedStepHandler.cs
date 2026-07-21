using Microsoft.Extensions.Logging;
using Obss.Workflow.Application.Abstractions;

namespace Obss.Workflow.Infrastructure.Services.StepHandlers;

public sealed class AutomatedStepHandler : IWorkflowStepHandler
{
    private readonly ILogger<AutomatedStepHandler> _logger;

    public AutomatedStepHandler(ILogger<AutomatedStepHandler> logger)
    {
        _logger = logger;
    }

    public string HandlerType => "Automated";

    public Task<StepExecutionResult> ExecuteAsync(
        string? configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing automated step with configuration: {Configuration}", configuration ?? "(none)");

        // In production, this would dispatch to a real automation handler
        // based on the configuration (e.g., API call, script execution, etc.)
        return Task.FromResult(new StepExecutionResult(
            true,
            "{\"result\": \"automated_step_completed\", \"configuration\": " +
            (configuration is not null ? $"\"{configuration}\"" : "null") + "}",
            null));
    }
}
