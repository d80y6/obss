using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.NetworkInventory.Application.Commands.CreateNetworkElement;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Infrastructure.Persistence;
using Obss.NetworkInventory.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Tests;

public class CommandHandlerTests : NetworkIntegrationTests
{
    [Fact]
    public async Task CreateNetworkElementCommand_ShouldCreateNetworkElementInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new NetworkElementRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateNetworkElementCommandHandler(repository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateNetworkElementCommand(
            tenantId,
            "core-router-01",
            "cr01.example.com",
            "10.0.0.1",
            "Router",
            "Cisco",
            "ISR4451",
            "16.12.03",
            "SN-ABC-123",
            "DataCenter-A",
            "10.0.0.254",
            "public",
            true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("core-router-01");
        result.Value.Hostname.Should().Be("cr01.example.com");
        result.Value.IPAddress.Should().Be("10.0.0.1");
        result.Value.ElementType.Should().Be("Router");
        result.Value.Vendor.Should().Be("Cisco");
        result.Value.Model.Should().Be("ISR4451");
        result.Value.SoftwareVersion.Should().Be("16.12.03");
        result.Value.SerialNumber.Should().Be("SN-ABC-123");
        result.Value.Location.Should().Be("DataCenter-A");
        result.Value.ManagementIP.Should().Be("10.0.0.254");
        result.Value.IsManaged.Should().BeTrue();
        result.Value.Status.Should().Be("Active");

        var saved = await repository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("core-router-01");
        saved.Hostname.Should().Be("cr01.example.com");
        saved.Status.ToString().Should().Be("Active");
    }

    [Fact]
    public async Task CreateNetworkElementCommand_ShouldCreateSwitchElement()
    {
        using var context = CreateDbContext();
        var repository = new NetworkElementRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateNetworkElementCommandHandler(repository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateNetworkElementCommand(
            tenantId,
            "access-switch-01",
            "sw01.example.com",
            "10.0.1.1",
            "Switch",
            "Juniper",
            "EX4300",
            "18.2R3",
            null,
            "Floor-2",
            null,
            null,
            true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ElementType.Should().Be("Switch");
        result.Value.Vendor.Should().Be("Juniper");
        result.Value.IsManaged.Should().BeTrue();
    }

    [Fact]
    public async Task CreateNetworkElementCommand_ShouldDefaultToActiveStatus()
    {
        using var context = CreateDbContext();
        var repository = new NetworkElementRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateNetworkElementCommandHandler(repository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateNetworkElementCommand(
            tenantId,
            "fw-01",
            "fw01.example.com",
            "10.0.2.1",
            "Firewall",
            "PaloAlto",
            "PA-5250",
            null,
            null,
            null,
            null,
            null,
            false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.Status.Should().Be("Active");
    }
}
