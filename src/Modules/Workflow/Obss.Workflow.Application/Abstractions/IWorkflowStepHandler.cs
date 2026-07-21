namespace Obss.Workflow.Application.Abstractions;

public sealed record StepExecutionResult(bool Success, string? ResultData, string? Error);

public interface IWorkflowStepHandler
{
    string HandlerType { get; }

    Task<StepExecutionResult> ExecuteAsync(
        string? configuration,
        CancellationToken cancellationToken = default);
}
