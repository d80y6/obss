namespace Obss.Workflow.Domain.ValueObjects;

public enum StepType
{
    Manual,
    Automated,
    SubWorkflow,
    Decision,
    Notification,
    ApiCall,
    Script
}
