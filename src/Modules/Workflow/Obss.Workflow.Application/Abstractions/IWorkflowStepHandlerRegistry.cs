namespace Obss.Workflow.Application.Abstractions;

public interface IWorkflowStepHandlerRegistry
{
    void Register(IWorkflowStepHandler handler);

    IWorkflowStepHandler? GetHandler(string handlerType);

    IEnumerable<IWorkflowStepHandler> GetAllHandlers();
}
