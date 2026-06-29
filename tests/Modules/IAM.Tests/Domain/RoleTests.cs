using Xunit;
using FluentAssertions;
using Obss.IAM.Domain.Entities;

namespace Obss.IAM.Tests.Domain;

public class RoleTests
{
    private static readonly Guid RoleId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", "Administrator role");

        role.Id.Should().Be(RoleId);
        role.TenantId.Should().Be("tenant-1");
        role.Name.Should().Be("Admin");
        role.Description.Should().Be("Administrator role");
        role.IsSystem.Should().BeFalse();
        role.Permissions.Should().BeEmpty();
        role.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_WithIsSystemTrue_ShouldSetSystemRole()
    {
        var role = new Role(RoleId, "tenant-1", "SuperAdmin", "System role", true);

        role.IsSystem.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullDescription_ShouldSetNull()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", null);

        role.Description.Should().BeNull();
    }

    [Fact]
    public void AddPermission_ShouldAddPermission()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", null);
        var permission = new Permission(Guid.NewGuid(), "iam.user.read", "Read Users", null, "IAM", "User", "Read");

        role.AddPermission(permission);

        role.Permissions.Should().ContainSingle();
        role.Permissions.Should().Contain(permission);
    }

    [Fact]
    public void AddPermission_WhenDuplicate_ShouldNotAdd()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", null);
        var permission = new Permission(Guid.NewGuid(), "iam.user.read", "Read Users", null, "IAM", "User", "Read");
        role.AddPermission(permission);

        role.AddPermission(permission);

        role.Permissions.Should().ContainSingle();
    }

    [Fact]
    public void RemovePermission_ShouldRemovePermission()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", null);
        var permission = new Permission(Guid.NewGuid(), "iam.user.read", "Read Users", null, "IAM", "User", "Read");
        role.AddPermission(permission);

        role.RemovePermission(permission);

        role.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void RemovePermission_WhenNotPresent_ShouldNotThrow()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", null);
        var permission = new Permission(Guid.NewGuid(), "iam.user.read", "Read Users", null, "IAM", "User", "Read");

        var act = () => role.RemovePermission(permission);

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndDescription()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", "Original description");

        role.UpdateDetails("SuperAdmin", "Updated description");

        role.Name.Should().Be("SuperAdmin");
        role.Description.Should().Be("Updated description");
    }

    [Fact]
    public void UpdateDetails_WithNullDescription_ShouldSetNull()
    {
        var role = new Role(RoleId, "tenant-1", "Admin", "Original description");

        role.UpdateDetails("Admin", null);

        role.Description.Should().BeNull();
    }
}
