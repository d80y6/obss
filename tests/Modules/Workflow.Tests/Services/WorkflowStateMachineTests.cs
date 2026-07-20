using FluentAssertions;
using Obss.Workflow.Domain.Services;
using Xunit;

namespace Obss.Workflow.Tests.Services;

public class WorkflowStateMachineTests
{
    [Fact]
    public void Constructor_ShouldStartInPendingState()
    {
        var machine = new WorkflowStateMachine();

        machine.CurrentState.Should().Be(ServiceFulfillmentState.Pending);
    }

    [Fact]
    public void Constructor_WithCustomState_ShouldStartInGivenState()
    {
        var machine = new WorkflowStateMachine(ServiceFulfillmentState.Activation);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.Activation);
    }

    [Fact]
    public void CanTransitionTo_FromPending_ShouldAllowQualification()
    {
        var machine = new WorkflowStateMachine();

        machine.CanTransitionTo(ServiceFulfillmentState.Qualification).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_FromPending_ShouldAllowFailed()
    {
        var machine = new WorkflowStateMachine();

        machine.CanTransitionTo(ServiceFulfillmentState.Failed).Should().BeTrue();
    }

    [Fact]
    public void CanTransitionTo_FromPending_ShouldNotAllowCompleted()
    {
        var machine = new WorkflowStateMachine();

        machine.CanTransitionTo(ServiceFulfillmentState.Completed).Should().BeFalse();
    }

    [Fact]
    public void CanTransitionTo_FromPending_ShouldNotAllowTerminated()
    {
        var machine = new WorkflowStateMachine();

        machine.CanTransitionTo(ServiceFulfillmentState.Terminated).Should().BeFalse();
    }

    [Fact]
    public void TransitionTo_ShouldMoveToTargetState()
    {
        var machine = new WorkflowStateMachine();

        machine.TransitionTo(ServiceFulfillmentState.Qualification);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.Qualification);
    }

    [Fact]
    public void TransitionTo_InvalidTransition_ShouldThrow()
    {
        var machine = new WorkflowStateMachine();

        FluentActions.Invoking(() => machine.TransitionTo(ServiceFulfillmentState.Completed))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetAllowedTransitions_FromPending_ShouldReturnCorrectSet()
    {
        var machine = new WorkflowStateMachine();

        var allowed = machine.GetAllowedTransitions();

        allowed.Should().Contain(ServiceFulfillmentState.Qualification);
        allowed.Should().Contain(ServiceFulfillmentState.Failed);
        allowed.Should().Contain(ServiceFulfillmentState.Suspended);
        allowed.Should().HaveCount(3);
    }

    [Fact]
    public void HappyPath_ShouldTransitionThroughAllStates()
    {
        var machine = new WorkflowStateMachine();

        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.CurrentState.Should().Be(ServiceFulfillmentState.Qualification);

        machine.TransitionTo(ServiceFulfillmentState.InventoryReservation);
        machine.CurrentState.Should().Be(ServiceFulfillmentState.InventoryReservation);

        machine.TransitionTo(ServiceFulfillmentState.Provisioning);
        machine.CurrentState.Should().Be(ServiceFulfillmentState.Provisioning);

        machine.TransitionTo(ServiceFulfillmentState.Activation);
        machine.CurrentState.Should().Be(ServiceFulfillmentState.Activation);

        machine.TransitionTo(ServiceFulfillmentState.Testing);
        machine.CurrentState.Should().Be(ServiceFulfillmentState.Testing);

        machine.TransitionTo(ServiceFulfillmentState.Completed);
        machine.CurrentState.Should().Be(ServiceFulfillmentState.Completed);
    }

    [Fact]
    public void FailedState_ShouldAllowRollback()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Failed);

        machine.CanTransitionTo(ServiceFulfillmentState.Rollback).Should().BeTrue();
        machine.TransitionTo(ServiceFulfillmentState.Rollback);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.Rollback);
    }

    [Fact]
    public void Rollback_ShouldTransitionToCompensationCompleted()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Failed);
        machine.TransitionTo(ServiceFulfillmentState.Rollback);

        machine.TransitionTo(ServiceFulfillmentState.CompensationCompleted);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.CompensationCompleted);
    }

    [Fact]
    public void SuspendedState_ShouldAllowResumeToPriorStates()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Suspended);

        machine.CanTransitionTo(ServiceFulfillmentState.Qualification).Should().BeTrue();
    }

    [Fact]
    public void CompletedState_ShouldAllowPendingChange()
    {
        var machine = new WorkflowStateMachine();
        RunHappyPath(machine);

        machine.CanTransitionTo(ServiceFulfillmentState.PendingChange).Should().BeTrue();
    }

    [Fact]
    public void CompletedState_ShouldAllowPendingTermination()
    {
        var machine = new WorkflowStateMachine();
        RunHappyPath(machine);

        machine.CanTransitionTo(ServiceFulfillmentState.PendingTermination).Should().BeTrue();
    }

    [Fact]
    public void TerminatedState_ShouldHaveNoTransitions()
    {
        var machine = new WorkflowStateMachine();
        RunHappyPath(machine);
        machine.TransitionTo(ServiceFulfillmentState.PendingTermination);
        machine.TransitionTo(ServiceFulfillmentState.Terminated);

        machine.GetAllowedTransitions().Should().BeEmpty();
    }

    [Fact]
    public void TransitionTo_FromSuspendedToFailed_ShouldSucceed()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Suspended);

        machine.TransitionTo(ServiceFulfillmentState.Failed);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.Failed);
    }

    [Fact]
    public void TransitionTo_FromFailedToPendingChange_ShouldSucceed()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Failed);

        machine.TransitionTo(ServiceFulfillmentState.PendingChange);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.PendingChange);
    }

    [Fact]
    public void TransitionTo_FromCompensationCompletedToPendingChange_ShouldSucceed()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Failed);
        machine.TransitionTo(ServiceFulfillmentState.Rollback);
        machine.TransitionTo(ServiceFulfillmentState.CompensationCompleted);

        machine.TransitionTo(ServiceFulfillmentState.PendingChange);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.PendingChange);
    }

    [Fact]
    public void TransitionTo_SuspendedToPendingTermination_ShouldSucceed()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Suspended);

        machine.TransitionTo(ServiceFulfillmentState.PendingTermination);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.PendingTermination);
    }

    [Fact]
    public void TransitionTo_PendingTerminationToTerminated_ShouldSucceed()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Suspended);
        machine.TransitionTo(ServiceFulfillmentState.PendingTermination);

        machine.TransitionTo(ServiceFulfillmentState.Terminated);

        machine.CurrentState.Should().Be(ServiceFulfillmentState.Terminated);
    }

    [Fact]
    public void GetAllowedTransitions_FromFailed_ShouldNotIncludeComplete()
    {
        var machine = new WorkflowStateMachine();
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.Failed);

        var allowed = machine.GetAllowedTransitions();

        allowed.Should().NotContain(ServiceFulfillmentState.Completed);
    }

    private static void RunHappyPath(IWorkflowStateMachine machine)
    {
        machine.TransitionTo(ServiceFulfillmentState.Qualification);
        machine.TransitionTo(ServiceFulfillmentState.InventoryReservation);
        machine.TransitionTo(ServiceFulfillmentState.Provisioning);
        machine.TransitionTo(ServiceFulfillmentState.Activation);
        machine.TransitionTo(ServiceFulfillmentState.Testing);
        machine.TransitionTo(ServiceFulfillmentState.Completed);
    }
}
