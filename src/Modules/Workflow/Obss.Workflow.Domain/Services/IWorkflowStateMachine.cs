namespace Obss.Workflow.Domain.Services;

public enum ServiceFulfillmentState
{
    Pending,
    Qualification,
    InventoryReservation,
    Provisioning,
    Activation,
    Testing,
    Completed,
    Failed,
    Rollback,
    CompensationCompleted,
    Suspended,
    PendingChange,
    PendingTermination,
    Terminated
}

public interface IWorkflowStateMachine
{
    ServiceFulfillmentState CurrentState { get; }
    bool CanTransitionTo(ServiceFulfillmentState target);
    ServiceFulfillmentState TransitionTo(ServiceFulfillmentState target);
    IReadOnlyList<ServiceFulfillmentState> GetAllowedTransitions();
}
