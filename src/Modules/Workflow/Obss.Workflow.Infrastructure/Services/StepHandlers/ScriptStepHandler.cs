using Microsoft.Extensions.Logging;
using Obss.Workflow.Application.Abstractions;

namespace Obss.Workflow.Infrastructure.Services.StepHandlers;

public sealed class ScriptStepHandler : IWorkflowStepHandler
{
    private readonly ILogger<ScriptStepHandler> _logger;

    public ScriptStepHandler(ILogger<ScriptStepHandler> logger)
    {
        _logger = logger;
    }

    public string HandlerType => "Script";

    public Task<StepExecutionResult> ExecuteAsync(
        string? configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing script step with configuration: {Configuration}", configuration ?? "(none)");

        return Task.FromResult(new StepExecutionResult(
            true,
            "{\"result\": \"script_executed\"}",
            null));
    }
}
