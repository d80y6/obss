using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Obss.IAM.Domain.Entities;
using Obss.IAM.Infrastructure.Persistence;
using Obss.IAM.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.IAM.Tests;

public class RepositoryTests : IamIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveUser()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var email = Email.Create("testuser@example.com");
        var user = User.Create(tenantId, "testuser", email, "Test", "User");

        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(user.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Username.Should().Be("testuser");
        retrieved.Email.Value.Should().Be("testuser@example.com");
        retrieved.FirstName.Should().Be("Test");
        retrieved.LastName.Should().Be("User");
        retrieved.TenantId.Value.Should().Be(tenantId.Value);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanAddAndRetrieveTenant()
    {
        using var context = CreateDbContext();
        var repository = new TenantRepository(context);

        var tenant = Tenant.Create("Test Tenant", "test-tenant");

        await repository.AddAsync(tenant);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(tenant.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Tenant");
        retrieved.Slug.Should().Be("test-tenant");
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanQueryUsersByTenant()
    {
        var tenantId1 = TenantId.Create(Guid.NewGuid().ToString("N"));

        using (var context1 = CreateDbContext())
        {
            var repository1 = new UserRepository(context1);
            var user1 = User.Create(tenantId1, "user1", Email.Create("user1@example.com"), "User", "One");
            await repository1.AddAsync(user1);
            await context1.SaveChangesAsync();
        }

        using (var context2 = CreateDbContext())
        {
            var repository2 = new UserRepository(context2);
            var user2 = User.Create(tenantId1, "user2", Email.Create("user2@example.com"), "User", "Two");
            await repository2.AddAsync(user2);
            await context2.SaveChangesAsync();
        }

        using (var context3 = CreateDbContext())
        {
            var repository3 = new UserRepository(context3);
            var usersInTenant1 = await repository3.GetFilteredAsync(tenantId1.Value, null, null, 1, 10);

            usersInTenant1.Should().HaveCount(2);
            usersInTenant1.Should().Contain(u => u.Username == "user1");
            usersInTenant1.Should().Contain(u => u.Username == "user2");
        }
    }

    [Fact]
    public async Task CanRetrieveUserByEmail()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var email = Email.Create("unique@example.com");
        var user = User.Create(tenantId, "uniqueuser", email, "Unique", "User");

        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByEmailAsync("unique@example.com");

        retrieved.Should().NotBeNull();
        retrieved!.Username.Should().Be("uniqueuser");
    }

    [Fact]
    public async Task CanRetrieveTenantBySlug()
    {
        using var context = CreateDbContext();
        var tenantRepository = new TenantRepository(context);

        var tenant = Tenant.Create("Slug Tenant", "my-slug");
        await tenantRepository.AddAsync(tenant);
        await context.SaveChangesAsync();

        var retrieved = await tenantRepository.GetBySlugAsync("my-slug");

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Slug Tenant");
    }

    [Fact]
    public async Task CanRetrieveUserByUsernameAndTenant()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var user = User.Create(tenantId, "uniqueuser", Email.Create("unique2@example.com"), "Unique", "User");

        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByUsernameAsync("uniqueuser", tenantId.Value);

        retrieved.Should().NotBeNull();
        retrieved!.Email.Value.Should().Be("unique2@example.com");
    }

    [Fact]
    public async Task GetUserWithRoles_ShouldIncludeRoles()
    {
        using var context = CreateDbContext();
        var userRepository = new UserRepository(context);

        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));
        var user = User.Create(tenantId, "roleuser", Email.Create("roleuser@example.com"), "Role", "User");

        var roleId = Guid.NewGuid();
        var role = new Role(roleId, tenantId.Value, "Admin", "Administrator role");
        context.Roles.Add(role);
        user.AssignRole(roleId, Guid.NewGuid());

        await userRepository.AddAsync(user);
        await context.SaveChangesAsync();

        var retrieved = await userRepository.GetByIdWithRolesAsync(user.Id);

        retrieved.Should().NotBeNull();
        retrieved!.UserRoles.Should().ContainSingle();
        retrieved.UserRoles.First().Role.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task CanFilterUsersByIsActive()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));

        var activeUser = User.Create(tenantId, "activeuser", Email.Create("active@example.com"), "Active", "User");
        await repository.AddAsync(activeUser);
        await context.SaveChangesAsync();

        activeUser.Deactivate();
        await context.SaveChangesAsync();

        var inactiveUsers = await repository.GetFilteredAsync(tenantId.Value, false, null, 1, 10);
        var activeUsers = await repository.GetFilteredAsync(tenantId.Value, true, null, 1, 10);

        inactiveUsers.Should().ContainSingle(u => u.Username == "activeuser");
        activeUsers.Should().BeEmpty();
    }

    [Fact]
    public async Task CanFilterUsersBySearchTerm()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));

        var user1 = User.Create(tenantId, "john", Email.Create("john@example.com"), "John", "Doe");
        var user2 = User.Create(tenantId, "jane", Email.Create("jane@example.com"), "Jane", "Doe");
        var user3 = User.Create(tenantId, "bob", Email.Create("bob@example.com"), "Bob", "Smith");

        await repository.AddAsync(user1);
        await repository.AddAsync(user2);
        await repository.AddAsync(user3);
        await context.SaveChangesAsync();

        var results = await repository.GetFilteredAsync(tenantId.Value, null, "john", 1, 10);

        results.Should().HaveCount(1);
        results.Should().Contain(u => u.Username == "john" || u.FirstName == "John");
    }

    [Fact]
    public async Task GetFilteredAsync_WithPagination_ShouldReturnCorrectPage()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);
        var tenantId = TenantId.Create(Guid.NewGuid().ToString("N"));

        for (int i = 1; i <= 5; i++)
        {
            var user = User.Create(tenantId, $"user{i}", Email.Create($"user{i}@example.com"), "User", $"{i}");
            await repository.AddAsync(user);
        }
        await context.SaveChangesAsync();

        var page1 = await repository.GetFilteredAsync(tenantId.Value, null, null, 1, 2);
        var page2 = await repository.GetFilteredAsync(tenantId.Value, null, null, 2, 2);
        var page3 = await repository.GetFilteredAsync(tenantId.Value, null, null, 3, 2);

        page1.Should().HaveCount(2);
        page2.Should().HaveCount(2);
        page3.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistingEmail_ShouldReturnNull()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);

        var retrieved = await repository.GetByEmailAsync("nonexistent@example.com");

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistingUsername_ShouldReturnNull()
    {
        using var context = CreateDbContext();
        var repository = new UserRepository(context);

        var retrieved = await repository.GetByUsernameAsync("nonexistent", Guid.NewGuid().ToString("N"));

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_WithNonExistingSlug_ShouldReturnNull()
    {
        using var context = CreateDbContext();
        var repository = new TenantRepository(context);

        var retrieved = await repository.GetBySlugAsync("nonexistent-slug");

        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task CanAddAndRetrieveRole()
    {
        using var context = CreateDbContext();
        var repository = new Obss.SharedKernel.Infrastructure.Persistence.EfRepository<Role>(context);

        var roleId = Guid.NewGuid();
        var role = new Role(roleId, Guid.NewGuid().ToString("N"), "Viewer", "Can view");
        await repository.AddAsync(role);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(roleId);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Viewer");
        retrieved.Description.Should().Be("Can view");
    }

    [Fact]
    public async Task GetAllTenants_ShouldReturnAll()
    {
        using var context = CreateDbContext();
        var repository = new TenantRepository(context);

        var tenant1 = Tenant.Create("Tenant 1", "tenant-1");
        var tenant2 = Tenant.Create("Tenant 2", "tenant-2");
        await repository.AddAsync(tenant1);
        await repository.AddAsync(tenant2);
        await context.SaveChangesAsync();

        var all = await repository.GetAllAsync();

        all.Should().HaveCount(2);
        all.Should().Contain(t => t.Slug == "tenant-1");
        all.Should().Contain(t => t.Slug == "tenant-2");
    }
}
