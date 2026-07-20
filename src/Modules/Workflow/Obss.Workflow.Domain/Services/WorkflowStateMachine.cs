namespace Obss.Workflow.Domain.Services;

public sealed class WorkflowStateMachine : IWorkflowStateMachine
{
    private static readonly Dictionary<ServiceFulfillmentState, HashSet<ServiceFulfillmentState>> Transitions = new()
    {
        [ServiceFulfillmentState.Pending] =
        [
            ServiceFulfillmentState.Qualification,
            ServiceFulfillmentState.Failed,
            ServiceFulfillmentState.Suspended
        ],
        [ServiceFulfillmentState.Qualification] =
        [
            ServiceFulfillmentState.InventoryReservation,
            ServiceFulfillmentState.Failed,
            ServiceFulfillmentState.Suspended
        ],
        [ServiceFulfillmentState.InventoryReservation] =
        [
            ServiceFulfillmentState.Provisioning,
            ServiceFulfillmentState.Failed,
            ServiceFulfillmentState.Suspended
        ],
        [ServiceFulfillmentState.Provisioning] =
        [
            ServiceFulfillmentState.Activation,
            ServiceFulfillmentState.Failed,
            ServiceFulfillmentState.Suspended
        ],
        [ServiceFulfillmentState.Activation] =
        [
            ServiceFulfillmentState.Testing,
            ServiceFulfillmentState.Failed,
            ServiceFulfillmentState.Suspended
        ],
        [ServiceFulfillmentState.Testing] =
        [
            ServiceFulfillmentState.Completed,
            ServiceFulfillmentState.Failed,
            ServiceFulfillmentState.Suspended
        ],
        [ServiceFulfillmentState.Failed] =
        [
            ServiceFulfillmentState.Rollback,
            ServiceFulfillmentState.PendingChange
        ],
        [ServiceFulfillmentState.Rollback] =
        [
            ServiceFulfillmentState.CompensationCompleted,
            ServiceFulfillmentState.Failed
        ],
        [ServiceFulfillmentState.CompensationCompleted] =
        [
            ServiceFulfillmentState.PendingChange
        ],
        [ServiceFulfillmentState.Suspended] =
        [
            ServiceFulfillmentState.Qualification,
            ServiceFulfillmentState.InventoryReservation,
            ServiceFulfillmentState.Provisioning,
            ServiceFulfillmentState.Activation,
            ServiceFulfillmentState.Testing,
            ServiceFulfillmentState.PendingTermination,
            ServiceFulfillmentState.Failed
        ],
        [ServiceFulfillmentState.PendingChange] =
        [
            ServiceFulfillmentState.Qualification,
            ServiceFulfillmentState.Failed
        ],
        [ServiceFulfillmentState.PendingTermination] =
        [
            ServiceFulfillmentState.Terminated,
            ServiceFulfillmentState.Failed
        ],
        [ServiceFulfillmentState.Completed] =
        [
            ServiceFulfillmentState.PendingChange,
            ServiceFulfillmentState.PendingTermination
        ],
        [ServiceFulfillmentState.Terminated] = []
    };

    public WorkflowStateMachine(ServiceFulfillmentState initialState = ServiceFulfillmentState.Pending)
    {
        CurrentState = initialState;
    }

    public ServiceFulfillmentState CurrentState { get; private set; }

    public bool CanTransitionTo(ServiceFulfillmentState target)
    {
        return Transitions.TryGetValue(CurrentState, out var allowed) && allowed.Contains(target);
    }

    public ServiceFulfillmentState TransitionTo(ServiceFulfillmentState target)
    {
        if (!CanTransitionTo(target))
        {
            throw new InvalidOperationException(
                $"Cannot transition from '{CurrentState}' to '{target}'.");
        }

        CurrentState = target;
        return CurrentState;
    }

    public IReadOnlyList<ServiceFulfillmentState> GetAllowedTransitions()
    {
        return Transitions.TryGetValue(CurrentState, out var allowed)
            ? allowed.ToList()
            : [];
    }
}
