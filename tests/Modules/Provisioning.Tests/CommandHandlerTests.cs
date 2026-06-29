using Xunit;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Obss.Provisioning.Application.Commands.CreateProvisioningJob;
using Obss.Provisioning.Application.Commands.CreateProvisioningTemplate;
using Obss.Provisioning.Application.Commands.FailProvisioningJob;
using Obss.Provisioning.Application.Commands.RetryProvisioningJob;
using Obss.Provisioning.Application.Commands.StartProvisioningJob;
using Obss.Provisioning.Domain.ValueObjects;
using Obss.Provisioning.Infrastructure.Persistence;
using Obss.Provisioning.Infrastructure.Persistence.Repositories;

namespace Obss.Provisioning.Tests;

public class CommandHandlerTests : ProvisioningIntegrationTests
{
    [Fact]
    public async Task CreateProvisioningJobCommand_ShouldCreateJobInDatabase()
    {
        using var context = CreateDbContext();
        var jobRepository = new ProvisioningJobRepository(context);
        var templateRepository = new ProvisioningTemplateRepository(context);
        var mediator = Substitute.For<IMediator>();
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateProvisioningJobCommandHandler(
            jobRepository, templateRepository, mediator, unitOfWork);

        var tenantId = Guid.NewGuid();
        var command = new CreateProvisioningJobCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            tenantId,
            "Internet",
            "Provision");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ServiceType.Should().Be("Internet");
        result.Value.Action.Should().Be("Provision");
        result.Value.Status.Should().Be("Queued");
        result.Value.OrderId.Should().Be(command.OrderId);
        result.Value.OrderItemId.Should().Be(command.OrderItemId);
        result.Value.CustomerId.Should().Be(command.CustomerId);
        result.Value.TenantId.Should().Be(tenantId);

        var saved = await jobRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(JobStatus.Queued);
        saved.Action.Should().Be(ProvisioningAction.Provision);
    }

    [Fact]
    public async Task CreateProvisioningJobCommand_WithInvalidAction_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var jobRepository = new ProvisioningJobRepository(context);
        var templateRepository = new ProvisioningTemplateRepository(context);
        var mediator = Substitute.For<IMediator>();
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateProvisioningJobCommandHandler(
            jobRepository, templateRepository, mediator, unitOfWork);

        var command = new CreateProvisioningJobCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Internet",
            "InvalidAction");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid action");
    }

    [Fact]
    public async Task CreateProvisioningJobCommand_WithActivateAction_ShouldCreateJob()
    {
        using var context = CreateDbContext();
        var jobRepository = new ProvisioningJobRepository(context);
        var templateRepository = new ProvisioningTemplateRepository(context);
        var mediator = Substitute.For<IMediator>();
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateProvisioningJobCommandHandler(
            jobRepository, templateRepository, mediator, unitOfWork);

        var command = new CreateProvisioningJobCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VPN",
            "Activate");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Action.Should().Be("Activate");
        result.Value.Status.Should().Be("Queued");
    }

    [Fact]
    public async Task CreateTemplateCommand_ShouldCreateTemplateInDatabase()
    {
        using var context = CreateDbContext();
        var templateRepository = new ProvisioningTemplateRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateProvisioningTemplateCommandHandler(templateRepository, unitOfWork);

        var tenantId = Guid.NewGuid();
        var command = new CreateProvisioningTemplateCommand(
            tenantId,
            "Internet Provisioning",
            "Template for internet service provisioning",
            "Internet",
            "Provision",
            Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Internet Provisioning");
        result.Value.Description.Should().Be("Template for internet service provisioning");
        result.Value.ServiceType.Should().Be("Internet");
        result.Value.Action.Should().Be("Provision");
        result.Value.IsActive.Should().BeTrue();

        var saved = await templateRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Internet Provisioning");
    }

    [Fact]
    public async Task StartProvisioningJobCommand_ShouldStartQueuedJob()
    {
        using var context = CreateDbContext();
        var jobRepository = new ProvisioningJobRepository(context);
        var templateRepository = new ProvisioningTemplateRepository(context);
        var mediator = Substitute.For<IMediator>();
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateProvisioningJobCommandHandler(
            jobRepository, templateRepository, mediator, unitOfWork);

        var createCommand = new CreateProvisioningJobCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), "Internet", "Provision");

        var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        var startHandler = new StartProvisioningJobCommandHandler(jobRepository, unitOfWork);

        var startResult = await startHandler.Handle(
            new StartProvisioningJobCommand(createResult.Value.Id), CancellationToken.None);

        startResult.IsSuccess.Should().BeTrue();
        startResult.Value.Status.Should().Be("InProgress");
        startResult.Value.StartedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task FailProvisioningJobCommand_ShouldFailJobAndSetError()
    {
        using var context = CreateDbContext();
        var jobRepository = new ProvisioningJobRepository(context);
        var templateRepository = new ProvisioningTemplateRepository(context);
        var mediator = Substitute.For<IMediator>();
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateProvisioningJobCommandHandler(
            jobRepository, templateRepository, mediator, unitOfWork);

        var createResult = await createHandler.Handle(
            new CreateProvisioningJobCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), "Internet", "Provision"),
            CancellationToken.None);

        var startHandler = new StartProvisioningJobCommandHandler(jobRepository, unitOfWork);
        await startHandler.Handle(
            new StartProvisioningJobCommand(createResult.Value.Id), CancellationToken.None);

        var failHandler = new FailProvisioningJobCommandHandler(jobRepository, unitOfWork);
        var failResult = await failHandler.Handle(
            new FailProvisioningJobCommand(createResult.Value.Id, "Connection timeout"),
            CancellationToken.None);

        failResult.IsSuccess.Should().BeTrue();

        var saved = await jobRepository.GetByIdAsync(createResult.Value.Id);
        saved!.Status.Should().Be(JobStatus.Failed);
        saved.ErrorMessage.Should().Be("Connection timeout");
        saved.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RetryProvisioningJobCommand_ShouldResetFailedJobToQueued()
    {
        using var context = CreateDbContext();
        var jobRepository = new ProvisioningJobRepository(context);
        var templateRepository = new ProvisioningTemplateRepository(context);
        var mediator = Substitute.For<IMediator>();
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateProvisioningJobCommandHandler(
            jobRepository, templateRepository, mediator, unitOfWork);

        var createResult = await createHandler.Handle(
            new CreateProvisioningJobCommand(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid(), "Internet", "Provision"),
            CancellationToken.None);

        var startHandler = new StartProvisioningJobCommandHandler(jobRepository, unitOfWork);
        await startHandler.Handle(
            new StartProvisioningJobCommand(createResult.Value.Id), CancellationToken.None);

        var failHandler = new FailProvisioningJobCommandHandler(jobRepository, unitOfWork);
        await failHandler.Handle(
            new FailProvisioningJobCommand(createResult.Value.Id, "Error"),
            CancellationToken.None);

        var retryHandler = new RetryProvisioningJobCommandHandler(jobRepository, unitOfWork);
        var retryResult = await retryHandler.Handle(
            new RetryProvisioningJobCommand(createResult.Value.Id),
            CancellationToken.None);

        retryResult.IsSuccess.Should().BeTrue();

        var saved = await jobRepository.GetByIdAsync(createResult.Value.Id);
        saved!.Status.Should().Be(JobStatus.Queued);
        saved.ErrorMessage.Should().BeNull();
    }
}
