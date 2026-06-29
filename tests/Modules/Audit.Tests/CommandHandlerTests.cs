using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.Audit.Application.Commands.CreateAlertRule;
using Obss.Audit.Application.Commands.CreateAuditEntry;
using Obss.Audit.Infrastructure.Persistence;
using Obss.Audit.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Audit.Tests;

public class CommandHandlerTests : AuditIntegrationTests
{
    [Fact]
    public async Task CreateAuditEntryCommand_ShouldCreateEntryInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new AuditEntryRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateAuditEntryCommandHandler(repository, unitOfWork, currentTenant);

        var command = new CreateAuditEntryCommand(
            "Customer",
            Guid.NewGuid().ToString(),
            "Created",
            "{\"Name\": \"New Corp\"}",
            Guid.NewGuid().ToString(),
            "testuser",
            "192.168.1.1",
            "Mozilla/5.0",
            Guid.NewGuid().ToString(),
            false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.EntityType.Should().Be("Customer");
        result.Value.Action.Should().Be("Created");
        result.Value.IsSensitive.Should().BeFalse();

        var saved = await repository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.EntityType.Should().Be("Customer");
        saved.Action.ToString().Should().Be("Created");
    }

    [Fact]
    public async Task CreateAuditEntryCommand_ShouldCreateSensitiveEntry()
    {
        using var context = CreateDbContext();
        var repository = new AuditEntryRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateAuditEntryCommandHandler(repository, unitOfWork, currentTenant);

        var command = new CreateAuditEntryCommand(
            "User",
            Guid.NewGuid().ToString(),
            "Viewed",
            null,
            Guid.NewGuid().ToString(),
            "admin",
            null,
            null,
            null,
            true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsSensitive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAuditEntryCommand_ShouldFailWithInvalidAction()
    {
        using var context = CreateDbContext();
        var repository = new AuditEntryRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateAuditEntryCommandHandler(repository, unitOfWork, currentTenant);

        var command = new CreateAuditEntryCommand(
            "Customer",
            Guid.NewGuid().ToString(),
            "InvalidAction",
            null,
            null,
            null,
            null,
            null,
            null,
            false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid audit action");
    }

    [Fact]
    public async Task CreateAlertRuleCommand_ShouldCreateRuleInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRuleRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateAlertRuleCommandHandler(repository, unitOfWork, currentTenant);

        var command = new CreateAlertRuleCommand(
            "Failed Login Alert",
            "Alert when too many failed logins",
            "FailedLogin",
            "High",
            10,
            5,
            true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Failed Login Alert");
        result.Value.AlertType.Should().Be("FailedLogin");
        result.Value.Severity.Should().Be("High");
        result.Value.Threshold.Should().Be(10);
        result.Value.WindowMinutes.Should().Be(5);
        result.Value.IsActive.Should().BeTrue();

        var saved = await repository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Failed Login Alert");
        saved.Threshold.Should().Be(10);
    }

    [Fact]
    public async Task CreateAlertRuleCommand_ShouldCreateInactiveRule()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRuleRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateAlertRuleCommandHandler(repository, unitOfWork, currentTenant);

        var command = new CreateAlertRuleCommand(
            "Mass Export Alert",
            null,
            "MassExport",
            "Medium",
            50,
            60,
            false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task CreateAlertRuleCommand_ShouldFailWithInvalidAlertType()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRuleRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateAlertRuleCommandHandler(repository, unitOfWork, currentTenant);

        var command = new CreateAlertRuleCommand(
            "Bad Rule",
            null,
            "UnknownType",
            "High",
            5,
            10);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid alert type");
    }

    [Fact]
    public async Task CreateAlertRuleCommand_ShouldFailWithInvalidSeverity()
    {
        using var context = CreateDbContext();
        var repository = new AuditAlertRuleRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.TenantId.Returns(Guid.NewGuid().ToString("N"));

        var handler = new CreateAlertRuleCommandHandler(repository, unitOfWork, currentTenant);

        var command = new CreateAlertRuleCommand(
            "Bad Rule",
            null,
            "FailedLogin",
            "Extreme",
            5,
            10);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid alert severity");
    }
}
