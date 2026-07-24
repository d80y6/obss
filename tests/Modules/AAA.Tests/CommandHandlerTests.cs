using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.Commands.RegisterNas;
using Obss.SharedKernel.Application.Abstractions;
using Obss.AAA.Application.Commands.UpdateNasStatus;
using Obss.AAA.Domain.ValueObjects;
using Obss.AAA.Infrastructure.Persistence;
using Obss.AAA.Infrastructure.Persistence.Repositories;

namespace Obss.AAA.Tests;

public class CommandHandlerTests : AaaIntegrationTests
{
    [Fact]
    public async Task RegisterNasCommand_ShouldCreateNasInDatabase()
    {
        using var context = CreateDbContext();
        var nasRepository = new NasRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new RegisterNasCommandHandler(nasRepository, unitOfWork, CreateCurrentTenant());

        var command = new RegisterNasCommand(
            "BRAS-01",
            "10.0.0.1",
            "shared-secret-123",
            NasType.BRAS,
            "Yemen-PTC-Sanaa");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("BRAS-01");
        result.Value.NasIpAddress.Should().Be("10.0.0.1");
        result.Value.NasType.Should().Be("BRAS");
        result.Value.Location.Should().Be("Yemen-PTC-Sanaa");
        result.Value.Status.Should().Be("Active");
    }

    [Fact]
    public async Task RegisterNasCommand_ShouldPersistAllFields()
    {
        using var context = CreateDbContext();
        var nasRepository = new NasRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new RegisterNasCommandHandler(nasRepository, unitOfWork, CreateCurrentTenant());

        var command = new RegisterNasCommand(
            "WLC-Datacenter",
            "10.0.0.2",
            "wlc-secret",
            NasType.WLC,
            "Yemen-PTC-Aden");

        var result = await handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();

        var saved = await nasRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("WLC-Datacenter");
        saved.NasIpAddress.Should().Be("10.0.0.2");
        saved.NasSecret.Should().Be("wlc-secret");
        saved.NasType.Should().Be(NasType.WLC);
        saved.Location.Should().Be("Yemen-PTC-Aden");
        saved.Status.Should().Be("Active");
    }

    [Fact]
    public async Task UpdateNasStatusCommand_ShouldActivateNas()
    {
        using var context = CreateDbContext();
        var nasRepository = new NasRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var registerHandler = new RegisterNasCommandHandler(nasRepository, unitOfWork, CreateCurrentTenant());
        var createResult = await registerHandler.Handle(
            new RegisterNasCommand("Test-NAS", "10.0.0.10", "secret", NasType.BNG, null),
            CancellationToken.None);

        var updateHandler = new UpdateNasStatusCommandHandler(nasRepository, unitOfWork);
        var result = await updateHandler.Handle(
            new UpdateNasStatusCommand(createResult.Value.Id, "INACTIVE"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Inactive");

        var saved = await nasRepository.GetByIdAsync(createResult.Value.Id);
        saved!.Status.Should().Be("Inactive");
    }

    [Fact]
    public async Task UpdateNasStatusCommand_WithNonExistingNas_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var nasRepository = new NasRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new UpdateNasStatusCommandHandler(nasRepository, unitOfWork);

        var result = await handler.Handle(
            new UpdateNasStatusCommand(Guid.NewGuid(), "Active"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task UpdateNasStatusCommand_WithInvalidStatus_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var nasRepository = new NasRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var registerHandler = new RegisterNasCommandHandler(nasRepository, unitOfWork, CreateCurrentTenant());
        var createResult = await registerHandler.Handle(
            new RegisterNasCommand("Test-NAS", "10.0.0.20", "secret", NasType.VSAT, "Test"),
            CancellationToken.None);

        var handler = new UpdateNasStatusCommandHandler(nasRepository, unitOfWork);
        var result = await handler.Handle(
            new UpdateNasStatusCommand(createResult.Value.Id, "INVALID"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Validation");
    }

    [Fact]
    public async Task RegisterNasCommand_WithMultipleNasTypes_ShouldAllSucceed()
    {
        using var context = CreateDbContext();
        var nasRepository = new NasRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = CreateCurrentTenant();

        var handler = new RegisterNasCommandHandler(nasRepository, unitOfWork, currentTenant);

        var r1 = await handler.Handle(new RegisterNasCommand("BRAS-1", "10.0.1.1", "s1", NasType.BRAS, null), CancellationToken.None);
        var r2 = await handler.Handle(new RegisterNasCommand("BNG-1", "10.0.1.2", "s2", NasType.BNG, null), CancellationToken.None);
        var r3 = await handler.Handle(new RegisterNasCommand("WLC-1", "10.0.1.3", "s3", NasType.WLC, null), CancellationToken.None);
        var r4 = await handler.Handle(new RegisterNasCommand("VSAT-1", "10.0.1.4", "s4", NasType.VSAT, null), CancellationToken.None);
        var r5 = await handler.Handle(new RegisterNasCommand("UAG-1", "10.0.1.5", "s5", NasType.UAG, null), CancellationToken.None);

        r1.IsSuccess.Should().BeTrue();
        r2.IsSuccess.Should().BeTrue();
        r3.IsSuccess.Should().BeTrue();
        r4.IsSuccess.Should().BeTrue();
        r5.IsSuccess.Should().BeTrue();

        var saved1 = await nasRepository.GetByIdAsync(r1.Value.Id);
        saved1.Should().NotBeNull();
        saved1!.NasType.Should().Be(NasType.BRAS);
    }

    [Fact]
    public async Task GetNasByIpAddress_ShouldReturnCorrectNas()
    {
        using var context = CreateDbContext();
        var nasRepository = new NasRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var currentTenant = CreateCurrentTenant();

        var handler = new RegisterNasCommandHandler(nasRepository, unitOfWork, currentTenant);

        var resultA = await handler.Handle(
            new RegisterNasCommand("BRAS-A", "10.0.0.100", "sec1", NasType.BRAS, null), CancellationToken.None);
        await handler.Handle(
            new RegisterNasCommand("BRAS-B", "10.0.0.101", "sec2", NasType.BRAS, null), CancellationToken.None);

        var saved = await nasRepository.GetByIdAsync(resultA.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("BRAS-A");
        saved.NasIpAddress.Should().Be("10.0.0.100");
    }

    private static ICurrentTenant CreateCurrentTenant()
    {
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.TenantId.Returns(Guid.NewGuid().ToString("N"));
        return tenant;
    }
}
