using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Commands.CreateTenant;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.IAM.Tests.Application;

public class CreateTenantCommandHandlerTests
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _tenantRepository = Substitute.For<IRepository<Tenant>>();
        _userRepository = Substitute.For<IRepository<User>>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new CreateTenantCommandHandler(_tenantRepository, _userRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCreateTenantAndAdminUser()
    {
        var command = new CreateTenantCommand(
            "New Tenant", "new-tenant", null, null,
            "admin", "admin@tenant.com", "Admin", "User");

        _tenantRepository.AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Tenant.Create("New Tenant", "new-tenant")));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Tenant");
        result.Value.Slug.Should().Be("new-tenant");

        await _tenantRepository.Received(1).AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>());
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldSetActiveStateOnTenant()
    {
        var command = new CreateTenantCommand(
            "Active Tenant", "active-tenant", null, null,
            "admin", "admin@tenant.com", "Admin", "User");

        Tenant? capturedTenant = null;
        _tenantRepository.AddAsync(Arg.Any<Tenant>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Tenant.Create("Active Tenant", "active-tenant")))
            .AndDoes(callInfo => { capturedTenant = callInfo.Arg<Tenant>(); });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithConnectionStringAndSettings_ShouldPassToTenant()
    {
        var command = new CreateTenantCommand(
            "Config Tenant", "config-tenant", "Host=localhost", "{\"key\":\"val\"}",
            "admin", "admin@tenant.com", "Admin", "User");

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.ConnectionString.Should().Be("Host=localhost");
        result.Value.Settings.Should().Be("{\"key\":\"val\"}");
    }
}
