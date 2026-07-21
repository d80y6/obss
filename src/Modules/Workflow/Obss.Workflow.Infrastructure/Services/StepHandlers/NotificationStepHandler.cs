using Microsoft.Extensions.Logging;
using Obss.Workflow.Application.Abstractions;

namespace Obss.Workflow.Infrastructure.Services.StepHandlers;

public sealed class NotificationStepHandler : IWorkflowStepHandler
{
    private readonly ILogger<NotificationStepHandler> _logger;

    public NotificationStepHandler(ILogger<NotificationStepHandler> logger)
    {
        _logger = logger;
    }

    public string HandlerType => "Notification";

    public Task<StepExecutionResult> ExecuteAsync(
        string? configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending notification with configuration: {Configuration}", configuration ?? "(none)");

        return Task.FromResult(new StepExecutionResult(
            true,
            "{\"result\": \"notification_sent\"}",
            null));
    }
}
