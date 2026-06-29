using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.IAM.Application.Commands.AssignRole;
using Obss.IAM.Application.Commands.CreateTenant;
using Obss.IAM.Application.Commands.CreateUser;
using Obss.IAM.Application.Commands.DeactivateUser;
using Obss.IAM.Application.Commands.UpdateUser;
using Obss.IAM.Application.Queries.GetUserById;
using Obss.IAM.Application.Queries.GetUsers;
using Obss.IAM.Application.Queries.GetTenants;
using Obss.IAM.Domain.Entities;
using Obss.IAM.Infrastructure.Persistence;
using Obss.IAM.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests;

public class CommandHandlerTests : IamIntegrationTests
{
    [Fact]
    public async Task CreateUserCommand_ShouldCreateUserInDatabase()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateUserCommandHandler(userRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(
            tenantId,
            "johndoe",
            "john@example.com",
            "John",
            "Doe",
            null,
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Username.Should().Be("johndoe");
        result.Value.Email.Should().Be("john@example.com");
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");

        var saved = await userRepository.GetByEmailAsync("john@example.com");
        saved.Should().NotBeNull();
        saved!.Username.Should().Be("johndoe");
    }

    [Fact]
    public async Task CreateUserCommand_WithDuplicateEmail_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateUserCommandHandler(userRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(
            tenantId,
            "johndoe",
            "duplicate@example.com",
            "John",
            "Doe",
            null,
            null,
            null);

        await handler.Handle(command, CancellationToken.None);

        var duplicateCommand = new CreateUserCommand(
            tenantId,
            "janedoe",
            "duplicate@example.com",
            "Jane",
            "Doe",
            null,
            null,
            null);

        var result = await handler.Handle(duplicateCommand, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Description.Should().Contain("already registered");
    }

    [Fact]
    public async Task CreateUserCommand_WithDuplicateUsernameInSameTenant_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateUserCommandHandler(userRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateUserCommand(
            tenantId,
            "dupeuser",
            "first@example.com",
            "First",
            "User",
            null,
            null,
            null);

        await handler.Handle(command, CancellationToken.None);

        var duplicateCommand = new CreateUserCommand(
            tenantId,
            "dupeuser",
            "second@example.com",
            "Second",
            "User",
            null,
            null,
            null);

        var result = await handler.Handle(duplicateCommand, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.Conflict");
        result.Error.Description.Should().Contain("already taken");
    }

    [Fact]
    public async Task CreateTenantCommand_ShouldCreateTenantAndAdminUser()
    {
        using var context = CreateDbContext();
        var tenantRepository = new TenantRepository(context);
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateTenantCommandHandler(
            tenantRepository,
            userRepository,
            unitOfWork);

        var command = new CreateTenantCommand(
            "New Tenant",
            "new-tenant",
            null,
            null,
            "admin",
            "admin@tenant.com",
            "Admin",
            "User");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("New Tenant");
        result.Value.Slug.Should().Be("new-tenant");

        var savedTenant = await tenantRepository.GetBySlugAsync("new-tenant");
        savedTenant.Should().NotBeNull();
        savedTenant!.Name.Should().Be("New Tenant");

        var savedUser = await userRepository.GetByEmailAsync("admin@tenant.com");
        savedUser.Should().NotBeNull();
        savedUser!.Username.Should().Be("admin");
    }

    [Fact]
    public async Task CreateTenantCommand_ShouldSetActiveState()
    {
        using var context = CreateDbContext();
        var tenantRepository = new TenantRepository(context);
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateTenantCommandHandler(
            tenantRepository,
            userRepository,
            unitOfWork);

        var command = new CreateTenantCommand(
            "Active Tenant",
            "active-tenant",
            null,
            null,
            "admin",
            "admin2@tenant.com",
            "Admin",
            "User");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task DeactivateUserCommand_ShouldDeactivateUserInDatabase()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateUserCommandHandler(userRepository, unitOfWork);
        var tenantId = Guid.NewGuid().ToString("N");
        var createResult = await createHandler.Handle(
            new CreateUserCommand(tenantId, "deactivatem", "deactivate@example.com", "De", "Activate", null, null, null),
            CancellationToken.None);

        var deactivateHandler = new DeactivateUserCommandHandler(userRepository, unitOfWork);
        var result = await deactivateHandler.Handle(
            new DeactivateUserCommand(createResult.Value.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await userRepository.GetByIdAsync(createResult.Value.Id);
        saved.Should().NotBeNull();
        saved!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateUserCommand_WithNonExistingUser_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new DeactivateUserCommandHandler(userRepository, unitOfWork);

        var result = await handler.Handle(
            new DeactivateUserCommand(Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task UpdateUserCommand_ShouldUpdateUserProfileInDatabase()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateUserCommandHandler(userRepository, unitOfWork);
        var tenantId = Guid.NewGuid().ToString("N");
        var createResult = await createHandler.Handle(
            new CreateUserCommand(tenantId, "updateuser", "update@example.com", "Old", "Name", null, null, null),
            CancellationToken.None);

        var updateHandler = new UpdateUserCommandHandler(userRepository, unitOfWork);
        var result = await updateHandler.Handle(
            new UpdateUserCommand(createResult.Value.Id, "New", "Name", null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("New");
        result.Value.LastName.Should().Be("Name");

        var saved = await userRepository.GetByIdAsync(createResult.Value.Id);
        saved!.FirstName.Should().Be("New");
        saved.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateUserCommand_WithNonExistingUser_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new UpdateUserCommandHandler(userRepository, unitOfWork);

        var result = await handler.Handle(
            new UpdateUserCommand(Guid.NewGuid(), "New", "Name", null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task AssignRoleCommand_ShouldAssignRoleToUserInDatabase()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var roleRepository = new Obss.SharedKernel.Infrastructure.Persistence.EfRepository<Role>(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateUserCommandHandler(userRepository, unitOfWork);
        var tenantId = Guid.NewGuid().ToString("N");
        var createResult = await createHandler.Handle(
            new CreateUserCommand(tenantId, "roleuser", "roleuser@example.com", "Role", "User", null, null, null),
            CancellationToken.None);

        var roleId = Guid.NewGuid();
        var role = new Role(roleId, tenantId, "Admin", "Administrator role");
        context.Roles.Add(role);
        await context.SaveChangesAsync();

        var assignHandler = new AssignRoleCommandHandler(userRepository, roleRepository, unitOfWork);
        var result = await assignHandler.Handle(
            new AssignRoleCommand(createResult.Value.Id, roleId, Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var saved = await userRepository.GetByIdWithRolesAsync(createResult.Value.Id);
        saved!.UserRoles.Should().ContainSingle(ur => ur.RoleId == roleId);
    }

    [Fact]
    public async Task AssignRoleCommand_WithNonExistingUser_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var roleRepository = new Obss.SharedKernel.Infrastructure.Persistence.EfRepository<Role>(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new AssignRoleCommandHandler(userRepository, roleRepository, unitOfWork);

        var result = await handler.Handle(
            new AssignRoleCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetUserByIdQuery_WithExistingUser_ShouldReturnUser()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var createHandler = new CreateUserCommandHandler(userRepository, unitOfWork);
        var tenantId = Guid.NewGuid().ToString("N");
        var createResult = await createHandler.Handle(
            new CreateUserCommand(tenantId, "queryuser", "query@example.com", "Query", "User", null, null, null),
            CancellationToken.None);

        var queryHandler = new GetUserByIdQueryHandler(userRepository);
        var result = await queryHandler.Handle(
            new GetUserByIdQuery(createResult.Value.Id),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("queryuser");
    }

    [Fact]
    public async Task GetUserByIdQuery_WithNonExistingUser_ShouldReturnFailure()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);

        var handler = new GetUserByIdQueryHandler(userRepository);
        var result = await handler.Handle(
            new GetUserByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Error.NotFound");
    }

    [Fact]
    public async Task GetUsersQuery_ShouldReturnFilteredList()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var handler = new CreateUserCommandHandler(userRepository, unitOfWork);
        await handler.Handle(
            new CreateUserCommand(tenantId, "filter1", "filter1@example.com", "Filter", "One", null, null, null),
            CancellationToken.None);
        await handler.Handle(
            new CreateUserCommand(tenantId, "filter2", "filter2@example.com", "Filter", "Two", null, null, null),
            CancellationToken.None);

        var queryHandler = new GetUsersQueryHandler(userRepository);
        var result = await queryHandler.Handle(
            new GetUsersQuery(tenantId, null, null, 1, 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTenantsQuery_ShouldReturnAllTenants()
    {
        using var context = CreateDbContext();
        var tenantRepository = new TenantRepository(context);
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateTenantCommandHandler(
            tenantRepository, userRepository, unitOfWork);
        await handler.Handle(
            new CreateTenantCommand("Tenant A", "tenant-a", null, null, "admin", "admin@tenanta.com", "Admin", "A"),
            CancellationToken.None);

        var queryHandler = new GetTenantsQueryHandler(tenantRepository);
        var result = await queryHandler.Handle(
            new GetTenantsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Contain(t => t.Slug == "tenant-a");
    }

    [Fact]
    public async Task CreateUserCommand_WithPhoneNumber_ShouldPersistPhoneNumber()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateUserCommandHandler(userRepository, unitOfWork);
        var tenantId = Guid.NewGuid().ToString("N");
        var result = await handler.Handle(
            new CreateUserCommand(tenantId, "phoneuser", "phone2@example.com", "Phone", "User", "712345678", "+967", null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PhoneNumber.Should().Be("+967712345678");

        var saved = await userRepository.GetByEmailAsync("phone2@example.com");
        saved!.PhoneNumber.Should().NotBeNull();
        saved.PhoneNumber!.Number.Should().Be("712345678");
        saved.PhoneNumber.CountryCode.Should().Be("+967");
    }

    [Fact]
    public async Task CreateUserCommand_WithDifferentTenants_ShouldAllowSameUsername()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateUserCommandHandler(userRepository, unitOfWork);
        var tenant1 = Guid.NewGuid().ToString("N");
        var tenant2 = Guid.NewGuid().ToString("N");

        var result1 = await handler.Handle(
            new CreateUserCommand(tenant1, "sameuser", "same1@example.com", "Same", "User", null, null, null),
            CancellationToken.None);
        var result2 = await handler.Handle(
            new CreateUserCommand(tenant2, "sameuser", "same2@example.com", "Same", "User", null, null, null),
            CancellationToken.None);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
    }
}
