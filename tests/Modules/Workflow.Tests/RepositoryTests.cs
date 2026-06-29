using Xunit;
using FluentAssertions;
using Obss.Workflow.Domain.Entities;
using Obss.Workflow.Domain.ValueObjects;
using Obss.Workflow.Infrastructure.Persistence.Repositories;

namespace Obss.Workflow.Tests;

public class RepositoryTests : WorkflowIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveWorkflowDefinition()
    {
        using var context = CreateDbContext();
        var repository = new WorkflowDefinitionRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var definition = WorkflowDefinition.Create(
            tenantId,
            "Service Activation",
            "Activates customer services",
            WorkflowCategory.Activation);

        definition.AddStep(1, "Validate Request", "Check request validity",
            StepType.Automated, "RequestValidator", """{"rules": ["check_credit"]}""",
            30, 3, 5, true);

        definition.AddStep(2, "Approve", "Manager approval",
            StepType.Manual, null, null,
            0, 0, 0, true);

        await repository.AddAsync(definition);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(definition.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Service Activation");
        retrieved.Description.Should().Be("Activates customer services");
        retrieved.Category.Should().Be(WorkflowCategory.Activation);
        retrieved.IsActive.Should().BeTrue();
        retrieved.Version.Should().Be(1);
        retrieved.TenantId.Should().Be(tenantId);
        retrieved.Steps.Should().HaveCount(2);
    }

    [Fact]
    public async Task CanFilterWorkflowDefinitionsByTenant()
    {
        var tenantId1 = Guid.NewGuid().ToString("N");
        var tenantId2 = Guid.NewGuid().ToString("N");

        using (var context = CreateDbContext())
        {
            var repo = new WorkflowDefinitionRepository(context);

            var def1 = WorkflowDefinition.Create(tenantId1, "Tenant1 Flow", null, WorkflowCategory.Provisioning);
            var def2 = WorkflowDefinition.Create(tenantId1, "Tenant1 Other", null, WorkflowCategory.Activation);
            var def3 = WorkflowDefinition.Create(tenantId2, "Tenant2 Flow", null, WorkflowCategory.Custom);

            await repo.AddAsync(def1);
            await repo.AddAsync(def2);
            await repo.AddAsync(def3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new WorkflowDefinitionRepository(context);
            var tenant1Defs = await repo.GetFilteredAsync(tenantId1, null, null, null, 1, 10);

            tenant1Defs.Should().HaveCount(2);
            tenant1Defs.Should().Contain(d => d.Name == "Tenant1 Flow");
            tenant1Defs.Should().Contain(d => d.Name == "Tenant1 Other");
        }
    }

    [Fact]
    public async Task CanFilterWorkflowDefinitionsByCategory()
    {
        using var context = CreateDbContext();
        var repo = new WorkflowDefinitionRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var provisioning = WorkflowDefinition.Create(tenantId, "Prov Flow", null, WorkflowCategory.Provisioning);
        var activation = WorkflowDefinition.Create(tenantId, "Act Flow", null, WorkflowCategory.Activation);
        var suspension = WorkflowDefinition.Create(tenantId, "Sus Flow", null, WorkflowCategory.Suspension);

        await repo.AddAsync(provisioning);
        await repo.AddAsync(activation);
        await repo.AddAsync(suspension);
        await context.SaveChangesAsync();

        var activationDefs = await repo.GetFilteredAsync(null, "Activation", null, null, 1, 10);

        activationDefs.Should().Contain(d => d.Name == "Act Flow");
        activationDefs.Should().NotContain(d => d.Name == "Prov Flow");
        activationDefs.Should().NotContain(d => d.Name == "Sus Flow");
    }

    [Fact]
    public async Task CanSearchWorkflowDefinitionsByName()
    {
        using var context = CreateDbContext();
        var repo = new WorkflowDefinitionRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var customerOnboarding = WorkflowDefinition.Create(tenantId, "Customer Onboarding", "Onboarding process for new customers", WorkflowCategory.Provisioning);
        var orderApproval = WorkflowDefinition.Create(tenantId, "Order Approval", "Approval process for orders", WorkflowCategory.Provisioning);
        var serviceActivation = WorkflowDefinition.Create(tenantId, "Service Activation", null, WorkflowCategory.Activation);

        await repo.AddAsync(customerOnboarding);
        await repo.AddAsync(orderApproval);
        await repo.AddAsync(serviceActivation);
        await context.SaveChangesAsync();

        var searchResults = await repo.GetFilteredAsync(null, null, null, "Onboarding", 1, 10);

        searchResults.Should().Contain(d => d.Name == "Customer Onboarding");
        searchResults.Should().NotContain(d => d.Name == "Order Approval");
        searchResults.Should().NotContain(d => d.Name == "Service Activation");
    }

    [Fact]
    public async Task CanFilterActiveAndInactiveDefinitions()
    {
        using var context = CreateDbContext();
        var repo = new WorkflowDefinitionRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeDef = WorkflowDefinition.Create(tenantId, "Active Flow", null, WorkflowCategory.Provisioning);
        var inactiveDef = WorkflowDefinition.Create(tenantId, "Inactive Flow", null, WorkflowCategory.Custom);
        inactiveDef.Deactivate();

        await repo.AddAsync(activeDef);
        await repo.AddAsync(inactiveDef);
        await context.SaveChangesAsync();

        var activeResults = await repo.GetFilteredAsync(null, null, true, null, 1, 10);
        activeResults.Should().Contain(d => d.Name == "Active Flow");
        activeResults.Should().NotContain(d => d.Name == "Inactive Flow");

        var inactiveResults = await repo.GetFilteredAsync(null, null, false, null, 1, 10);
        inactiveResults.Should().Contain(d => d.Name == "Inactive Flow");
        inactiveResults.Should().NotContain(d => d.Name == "Active Flow");
    }

    [Fact]
    public async Task CanCreateNewVersionOfWorkflowDefinition()
    {
        using var context = CreateDbContext();
        var repository = new WorkflowDefinitionRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var definition = WorkflowDefinition.Create(
            tenantId,
            "Versioned Flow",
            "Original version",
            WorkflowCategory.Provisioning);

        definition.AddStep(1, "Step One", null,
            StepType.Automated, "HandlerA", null, 10, 0, 0, true);

        await repository.AddAsync(definition);
        await context.SaveChangesAsync();

        var newVersion = definition.CreateNewVersion();
        newVersion.AddStep(2, "Step Two New", null,
            StepType.Manual, null, null, 0, 0, 0, true);

        await repository.AddAsync(newVersion);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(newVersion.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Version.Should().Be(2);
        retrieved.Name.Should().Be("Versioned Flow");
        retrieved.Steps.Should().HaveCount(2);

        var original = await repository.GetByIdAsync(definition.Id);
        original.Should().NotBeNull();
        original!.Version.Should().Be(1);
        original.Steps.Should().HaveCount(1);
    }

    [Fact]
    public async Task CanAddAndRetrieveWorkflowInstance()
    {
        using var context = CreateDbContext();
        var definitionRepo = new WorkflowDefinitionRepository(context);
        var instanceRepo = new WorkflowInstanceRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var definition = WorkflowDefinition.Create(tenantId, "Test Flow", null, WorkflowCategory.Activation);
        definition.AddStep(1, "Step 1", null, StepType.Automated, "Handler", null, 10, 0, 0, true);

        await definitionRepo.AddAsync(definition);
        await context.SaveChangesAsync();

        var triggerEntityId = Guid.NewGuid();
        var instance = WorkflowInstance.Create(
            definition.Id,
            definition.Name,
            "Order",
            triggerEntityId,
            "admin");

        instance.Start();

        instance.AddTask(new WorkflowTaskInstance(
            Guid.NewGuid(), instance.Id, definition.Steps.First().Id,
            "Step 1", "system"));

        await instanceRepo.AddAsync(instance);
        await context.SaveChangesAsync();

        var retrieved = await instanceRepo.GetByIdAsync(instance.Id);

        retrieved.Should().NotBeNull();
        retrieved!.WorkflowDefinitionId.Should().Be(definition.Id);
        retrieved.WorkflowDefinitionName.Should().Be("Test Flow");
        retrieved.TriggerEntityType.Should().Be("Order");
        retrieved.TriggerEntityId.Should().Be(triggerEntityId);
        retrieved.Status.Should().Be(InstanceStatus.Running);
        retrieved.StartedAt.Should().NotBeNull();
        retrieved.CreatedBy.Should().Be("admin");
        retrieved.Tasks.Should().HaveCount(1);
    }

    [Fact]
    public async Task CanFilterWorkflowInstancesByStatus()
    {
        using var context = CreateDbContext();
        var defRepo = new WorkflowDefinitionRepository(context);
        var instanceRepo = new WorkflowInstanceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var definition = WorkflowDefinition.Create(tenantId, "Test Flow", null, WorkflowCategory.Activation);
        await defRepo.AddAsync(definition);
        await context.SaveChangesAsync();

        var runningInstance = WorkflowInstance.Create(definition.Id, definition.Name, "Test", Guid.NewGuid(), "user1");
        runningInstance.Start();

        var completedInstance = WorkflowInstance.Create(definition.Id, definition.Name, "Test", Guid.NewGuid(), "user2");
        completedInstance.Start();
        completedInstance.Complete();

        var failedInstance = WorkflowInstance.Create(definition.Id, definition.Name, "Test", Guid.NewGuid(), "user3");
        failedInstance.Start();
        failedInstance.Fail("Something went wrong");

        await instanceRepo.AddAsync(runningInstance);
        await instanceRepo.AddAsync(completedInstance);
        await instanceRepo.AddAsync(failedInstance);
        await context.SaveChangesAsync();

        var runningResults = await instanceRepo.GetFilteredAsync("Running", null, null, 1, 10);
        runningResults.Should().Contain(i => i.Id == runningInstance.Id);
        runningResults.Should().NotContain(i => i.Id == completedInstance.Id);
        runningResults.Should().NotContain(i => i.Id == failedInstance.Id);

        var completedResults = await instanceRepo.GetFilteredAsync("Completed", null, null, 1, 10);
        completedResults.Should().Contain(i => i.Id == completedInstance.Id);
    }

    [Fact]
    public async Task CanFilterWorkflowInstancesByEntity()
    {
        using var context = CreateDbContext();
        var defRepo = new WorkflowDefinitionRepository(context);
        var instanceRepo = new WorkflowInstanceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var definition = WorkflowDefinition.Create(tenantId, "Test", null, WorkflowCategory.Custom);
        await defRepo.AddAsync(definition);
        await context.SaveChangesAsync();

        var entityId = Guid.NewGuid();

        var instance1 = WorkflowInstance.Create(definition.Id, definition.Name, "Order", entityId, "user");
        instance1.Start();

        var instance2 = WorkflowInstance.Create(definition.Id, definition.Name, "Order", Guid.NewGuid(), "user");
        instance2.Start();

        var instance3 = WorkflowInstance.Create(definition.Id, definition.Name, "Invoice", entityId, "user");
        instance3.Start();

        await instanceRepo.AddAsync(instance1);
        await instanceRepo.AddAsync(instance2);
        await instanceRepo.AddAsync(instance3);
        await context.SaveChangesAsync();

        var orderResults = await instanceRepo.GetFilteredAsync(null, "Order", null, 1, 10);
        orderResults.Should().HaveCount(2);
        orderResults.Should().Contain(i => i.Id == instance1.Id);
        orderResults.Should().Contain(i => i.Id == instance2.Id);

        var entityResults = await instanceRepo.GetFilteredAsync(null, null, entityId, 1, 10);
        entityResults.Should().HaveCount(2);
        entityResults.Should().Contain(i => i.Id == instance1.Id);
        entityResults.Should().Contain(i => i.Id == instance3.Id);
    }

    [Fact]
    public async Task CanGetPendingTasks()
    {
        using var context = CreateDbContext();
        var defRepo = new WorkflowDefinitionRepository(context);
        var instanceRepo = new WorkflowInstanceRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var definition = WorkflowDefinition.Create(tenantId, "Test", null, WorkflowCategory.Provisioning);
        definition.AddStep(1, "Pending Step", null, StepType.Manual, null, null, 0, 0, 0, true);
        definition.AddStep(2, "Auto Step", null, StepType.Automated, "Handler", null, 10, 0, 0, true);

        await defRepo.AddAsync(definition);
        await context.SaveChangesAsync();

        var instance = WorkflowInstance.Create(definition.Id, definition.Name, "Test", Guid.NewGuid(), "admin");
        instance.Start();

        var pendingTask = new WorkflowTaskInstance(
            Guid.NewGuid(), instance.Id, definition.Steps.ElementAt(0).Id,
            "Pending Step", null);
        pendingTask.Status.Should().Be(WorkflowTaskStatus.Pending);

        var completedTask = new WorkflowTaskInstance(
            Guid.NewGuid(), instance.Id, definition.Steps.ElementAt(1).Id,
            "Auto Step", "system");
        completedTask.Start();
        completedTask.Complete("done");

        instance.AddTask(pendingTask);
        instance.AddTask(completedTask);

        await instanceRepo.AddAsync(instance);
        await context.SaveChangesAsync();

        var pendingTasks = await instanceRepo.GetPendingTasksAsync();

        pendingTasks.Should().Contain(t => t.StepName == "Pending Step");
        pendingTasks.Should().NotContain(t => t.StepName == "Auto Step");
    }

    [Fact]
    public async Task CanCompleteWorkflowInstanceLifecycle()
    {
        using var context = CreateDbContext();
        var instanceRepo = new WorkflowInstanceRepository(context);
        var defRepo = new WorkflowDefinitionRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var definition = WorkflowDefinition.Create(tenantId, "Lifecycle Test", null, WorkflowCategory.Maintenance);
        await defRepo.AddAsync(definition);
        await context.SaveChangesAsync();

        var instance = WorkflowInstance.Create(definition.Id, definition.Name, "Test", Guid.NewGuid(), "user");

        instance.Status.Should().Be(InstanceStatus.Pending);

        instance.Start();
        instance.Status.Should().Be(InstanceStatus.Running);
        instance.StartedAt.Should().NotBeNull();

        instance.Pause();
        instance.Status.Should().Be(InstanceStatus.Paused);

        instance.Resume();
        instance.Status.Should().Be(InstanceStatus.Running);

        instance.Complete();
        instance.Status.Should().Be(InstanceStatus.Completed);
        instance.CompletedAt.Should().NotBeNull();

        await instanceRepo.AddAsync(instance);
        await context.SaveChangesAsync();

        var retrieved = await instanceRepo.GetByIdAsync(instance.Id);
        retrieved!.Status.Should().Be(InstanceStatus.Completed);
        retrieved.StartedAt.Should().NotBeNull();
        retrieved.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CanFailWorkflowInstance()
    {
        using var context = CreateDbContext();
        var instanceRepo = new WorkflowInstanceRepository(context);
        var defRepo = new WorkflowDefinitionRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var definition = WorkflowDefinition.Create(tenantId, "Fail Test", null, WorkflowCategory.Provisioning);
        await defRepo.AddAsync(definition);
        await context.SaveChangesAsync();

        var instance = WorkflowInstance.Create(definition.Id, definition.Name, "Test", Guid.NewGuid(), "user");
        instance.Start();
        instance.Fail("Timeout exceeded");

        instance.Status.Should().Be(InstanceStatus.Failed);
        instance.CompletedAt.Should().NotBeNull();

        await instanceRepo.AddAsync(instance);
        await context.SaveChangesAsync();

        var retrieved = await instanceRepo.GetByIdAsync(instance.Id);
        retrieved!.Status.Should().Be(InstanceStatus.Failed);
    }

    [Fact]
    public async Task CanCancelWorkflowInstance()
    {
        using var context = CreateDbContext();
        var instanceRepo = new WorkflowInstanceRepository(context);
        var defRepo = new WorkflowDefinitionRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var definition = WorkflowDefinition.Create(tenantId, "Cancel Test", null, WorkflowCategory.Custom);
        await defRepo.AddAsync(definition);
        await context.SaveChangesAsync();

        var instance = WorkflowInstance.Create(definition.Id, definition.Name, "Test", Guid.NewGuid(), "user");
        instance.Start();
        instance.Cancel("Customer requested cancellation");

        instance.Status.Should().Be(InstanceStatus.Cancelled);
        instance.CompletedAt.Should().NotBeNull();

        await instanceRepo.AddAsync(instance);
        await context.SaveChangesAsync();

        var retrieved = await instanceRepo.GetByIdAsync(instance.Id);
        retrieved!.Status.Should().Be(InstanceStatus.Cancelled);
    }
}
