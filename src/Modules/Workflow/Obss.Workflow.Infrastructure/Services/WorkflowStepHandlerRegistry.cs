using Obss.Workflow.Application.Abstractions;

namespace Obss.Workflow.Infrastructure.Services;

public sealed class WorkflowStepHandlerRegistry : IWorkflowStepHandlerRegistry
{
    private readonly Dictionary<string, IWorkflowStepHandler> _handlers;

    public WorkflowStepHandlerRegistry(IEnumerable<IWorkflowStepHandler> handlers)
    {
        _handlers = [];
        foreach (var handler in handlers)
        {
            _handlers[handler.HandlerType] = handler;
        }
    }

    public void Register(IWorkflowStepHandler handler)
    {
        _handlers[handler.HandlerType] = handler;
    }

    public IWorkflowStepHandler? GetHandler(string handlerType)
    {
        return _handlers.TryGetValue(handlerType, out var handler) ? handler : null;
    }

    public IEnumerable<IWorkflowStepHandler> GetAllHandlers()
    {
        return _handlers.Values;
    }
}
