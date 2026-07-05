using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.ServiceInventory.Application.Commands.CreateService;
using Obss.ServiceInventory.Application.Commands.ActivateService;
using Obss.ServiceInventory.Application.Commands.SuspendService;
using Obss.ServiceInventory.Application.Commands.DecommissionService;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.ServiceInventory.Infrastructure.Persistence;
using Obss.ServiceInventory.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.ServiceInventory.Tests;

public class CommandHandlerTests : ServiceIntegrationTests
{
    [Fact]
    public async Task CreateServiceCommand_ShouldCreateServiceInDatabase()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateServiceCommandHandler(serviceRepository, unitOfWork);

        var command = new CreateServiceCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "FTTH",
            "SVC-001",
            "Main Street",
            @"{""speed"": ""100Mbps""}");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ServiceType.Should().Be("FTTH");
        result.Value.ServiceIdentifier.Should().Be("SVC-001");
        result.Value.Location.Should().Be("Main Street");
        result.Value.Configuration.Should().Be(@"{""speed"": ""100Mbps""}");
        result.Value.Status.Should().Be("Pending");

        var saved = await serviceRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.ServiceIdentifier.Should().Be("SVC-001");
        saved.Location.Should().Be("Main Street");
    }

    [Fact]
    public async Task CreateServiceCommand_ShouldFailForInvalidServiceType()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateServiceCommandHandler(serviceRepository, unitOfWork);

        var command = new CreateServiceCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "InvalidType",
            "SVC-002",
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateServiceCommand_ShouldFailForDuplicateIdentifier()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateServiceCommandHandler(serviceRepository, unitOfWork);

        var command1 = new CreateServiceCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ADSL",
            "SVC-003",
            null,
            null);

        var result1 = await handler.Handle(command1, CancellationToken.None);
        result1.IsSuccess.Should().BeTrue();

        var command2 = new CreateServiceCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "ADSL",
            "SVC-003",
            null,
            null);

        var result2 = await handler.Handle(command2, CancellationToken.None);
        result2.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ActivateServiceCommand_ShouldActivatePendingService()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateServiceCommandHandler(serviceRepository, unitOfWork);

        var createCommand = new CreateServiceCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "VoIP",
            "SVC-004",
            null,
            null);

        var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        var activateHandler = new ActivateServiceCommandHandler(serviceRepository, unitOfWork);
        var activateCommand = new ActivateServiceCommand(createResult.Value.Id);

        var activateResult = await activateHandler.Handle(activateCommand, CancellationToken.None);

        activateResult.IsSuccess.Should().BeTrue();
        activateResult.Value.Status.Should().Be("Active");
        activateResult.Value.ActivationDate.Should().NotBeNull();

        var saved = await serviceRepository.GetByIdAsync(createResult.Value.Id);
        saved!.Status.Should().Be(ServiceStatus.Active);
        saved.ActivationDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateServiceCommand_ShouldFailForNonExistentService()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new ActivateServiceCommandHandler(serviceRepository, unitOfWork);

        var command = new ActivateServiceCommand(Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task SuspendServiceCommand_ShouldSuspendActiveService()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateServiceCommandHandler(serviceRepository, unitOfWork);
        var createCommand = new CreateServiceCommand(
            Guid.NewGuid(), Guid.NewGuid(), "FTTH", "SVC-005", null, null);
        var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        var activateHandler = new ActivateServiceCommandHandler(serviceRepository, unitOfWork);
        await activateHandler.Handle(new ActivateServiceCommand(createResult.Value.Id), CancellationToken.None);

        var suspendHandler = new SuspendServiceCommandHandler(serviceRepository, unitOfWork);
        var result = await suspendHandler.Handle(
            new SuspendServiceCommand(createResult.Value.Id, "Maintenance"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Suspended");
        result.Value.SuspendedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SuspendServiceCommand_ShouldFailForNonExistentService()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new SuspendServiceCommandHandler(serviceRepository, unitOfWork);
        var result = await handler.Handle(
            new SuspendServiceCommand(Guid.NewGuid(), "Reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DecommissionServiceCommand_ShouldDecommissionActiveService()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateServiceCommandHandler(serviceRepository, unitOfWork);
        var createCommand = new CreateServiceCommand(
            Guid.NewGuid(), Guid.NewGuid(), "ADSL", "SVC-006", null, null);
        var createResult = await createHandler.Handle(createCommand, CancellationToken.None);
        createResult.IsSuccess.Should().BeTrue();

        var activateHandler = new ActivateServiceCommandHandler(serviceRepository, unitOfWork);
        await activateHandler.Handle(new ActivateServiceCommand(createResult.Value.Id), CancellationToken.None);

        var decommissionHandler = new DecommissionServiceCommandHandler(serviceRepository, unitOfWork);
        var result = await decommissionHandler.Handle(
            new DecommissionServiceCommand(createResult.Value.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Decommissioned");
        result.Value.DecommissionedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DecommissionServiceCommand_ShouldFailForNonExistentService()
    {
        using var context = CreateDbContext();
        var serviceRepository = new ServiceRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new DecommissionServiceCommandHandler(serviceRepository, unitOfWork);
        var result = await handler.Handle(
            new DecommissionServiceCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
