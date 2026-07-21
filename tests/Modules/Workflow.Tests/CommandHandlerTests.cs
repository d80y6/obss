using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Workflow.Application.Commands.CreateWorkflowDefinition;
using Obss.Workflow.Application.Commands.StartWorkflowInstance;
using Obss.Workflow.Infrastructure.Persistence.Repositories;
using Obss.Workflow.Application.Abstractions;
using Obss.Workflow.Infrastructure.Services;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Workflow.Tests;

public class CommandHandlerTests : WorkflowIntegrationTests
{
    [Fact]
    public async Task CreateWorkflowDefinitionCommand_ShouldCreateDefinitionInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new WorkflowDefinitionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateWorkflowDefinitionCommandHandler(repository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateWorkflowDefinitionCommand(
            tenantId,
            "Customer Provisioning",
            "Handles new customer onboarding",
            "Provisioning");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Customer Provisioning");
        result.Value.Description.Should().Be("Handles new customer onboarding");
        result.Value.Category.Should().Be("Provisioning");
        result.Value.IsActive.Should().BeTrue();
        result.Value.Version.Should().Be(1);
        result.Value.TenantId.Should().Be(tenantId);

        var saved = await repository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Customer Provisioning");
    }

    [Fact]
    public async Task CreateWorkflowDefinitionCommand_ShouldFailWithInvalidCategory()
    {
        using var context = CreateDbContext();
        var repository = new WorkflowDefinitionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateWorkflowDefinitionCommandHandler(repository, unitOfWork);

        var command = new CreateWorkflowDefinitionCommand(
            Guid.NewGuid().ToString("N"),
            "Bad Category",
            null,
            "NonExistentCategory");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task CreateWorkflowDefinitionCommand_ShouldSupportCustomCategory()
    {
        using var context = CreateDbContext();
        var repository = new WorkflowDefinitionRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateWorkflowDefinitionCommandHandler(repository, unitOfWork);

        var command = new CreateWorkflowDefinitionCommand(
            Guid.NewGuid().ToString("N"),
            "Custom Flow",
            "A custom workflow",
            "Custom");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be("Custom");
    }

    [Fact]
    public async Task StartWorkflowInstanceCommand_ShouldCreateAndStartInstance()
    {
        using var context = CreateDbContext();
        var definitionRepository = new WorkflowDefinitionRepository(context);
        var instanceRepository = new WorkflowInstanceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<WorkflowEngine>>();

        var handlerRegistry = Substitute.For<IWorkflowStepHandlerRegistry>();
        var workflowEngine = new WorkflowEngine(definitionRepository, instanceRepository, handlerRegistry, logger);

        var handler = new StartWorkflowInstanceCommandHandler(
            workflowEngine, definitionRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");

        var definition = Obss.Workflow.Domain.Entities.WorkflowDefinition.Create(
            tenantId,
            "Order Approval",
            "Approval workflow for orders",
            Obss.Workflow.Domain.ValueObjects.WorkflowCategory.Provisioning);

        definition.AddStep(1, "Validate Order", "Check order validity",
            Obss.Workflow.Domain.ValueObjects.StepType.Automated,
            "OrderValidator", null, 30, 2, 10, true);

        definition.AddStep(2, "Manager Approval", "Manager reviews order",
            Obss.Workflow.Domain.ValueObjects.StepType.Manual,
            null, null, 0, 0, 0, true);

        await definitionRepository.AddAsync(definition);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var triggerEntityId = Guid.NewGuid();
        var command = new StartWorkflowInstanceCommand(
            definition.Id,
            "Order",
            triggerEntityId,
            "test-user");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.WorkflowDefinitionId.Should().Be(definition.Id);
        result.Value.WorkflowDefinitionName.Should().Be("Order Approval");
        result.Value.TriggerEntityType.Should().Be("Order");
        result.Value.TriggerEntityId.Should().Be(triggerEntityId);
        result.Value.Status.Should().Be("Running");
        result.Value.StartedAt.Should().NotBeNull();
        result.Value.CreatedBy.Should().Be("test-user");

        var saved = await instanceRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(Obss.Workflow.Domain.ValueObjects.InstanceStatus.Running);
        saved.Tasks.Should().HaveCount(2);
    }

    [Fact]
    public async Task StartWorkflowInstanceCommand_ShouldFailForNonexistentDefinition()
    {
        using var context = CreateDbContext();
        var definitionRepository = new WorkflowDefinitionRepository(context);
        var instanceRepository = new WorkflowInstanceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<WorkflowEngine>>();

        var handlerRegistry = Substitute.For<IWorkflowStepHandlerRegistry>();
        var workflowEngine = new WorkflowEngine(definitionRepository, instanceRepository, handlerRegistry, logger);

        var handler = new StartWorkflowInstanceCommandHandler(
            workflowEngine, definitionRepository, unitOfWork);

        var command = new StartWorkflowInstanceCommand(
            Guid.NewGuid(),
            "Order",
            Guid.NewGuid(),
            "test-user");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task StartWorkflowInstanceCommand_ShouldFailForInactiveDefinition()
    {
        using var context = CreateDbContext();
        var definitionRepository = new WorkflowDefinitionRepository(context);
        var instanceRepository = new WorkflowInstanceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<WorkflowEngine>>();

        var handlerRegistry = Substitute.For<IWorkflowStepHandlerRegistry>();
        var workflowEngine = new WorkflowEngine(definitionRepository, instanceRepository, handlerRegistry, logger);

        var handler = new StartWorkflowInstanceCommandHandler(
            workflowEngine, definitionRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var definition = Obss.Workflow.Domain.Entities.WorkflowDefinition.Create(
            tenantId,
            "Inactive Flow",
            null,
            Obss.Workflow.Domain.ValueObjects.WorkflowCategory.Custom);
        definition.Deactivate();

        await definitionRepository.AddAsync(definition);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        var command = new StartWorkflowInstanceCommand(
            definition.Id,
            "Test",
            Guid.NewGuid(),
            "test-user");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }
}
