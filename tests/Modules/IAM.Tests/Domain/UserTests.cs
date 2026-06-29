using Xunit;
using FluentAssertions;
using Obss.IAM.Domain.Entities;
using Obss.IAM.Domain.Events;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Tests.Domain;

public class UserTests
{
    private static readonly TenantId TenantId = SharedKernel.Domain.ValueObjects.TenantId.Create(Guid.NewGuid().ToString("N"));
    private static readonly Email Email = SharedKernel.Domain.ValueObjects.Email.Create("test@example.com");

    [Fact]
    public void Create_ShouldSetProperties()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");

        user.Should().NotBeNull();
        user.Username.Should().Be("testuser");
        user.Email.Should().Be(Email);
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.TenantId.Should().Be(TenantId);
        user.IsActive.Should().BeTrue();
        user.EmailVerified.Should().BeFalse();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldRaiseUserCreatedDomainEvent()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");

        user.DomainEvents.Should().ContainSingle(e => e is UserCreatedDomainEvent);
        var domainEvent = user.DomainEvents.First() as UserCreatedDomainEvent;
        domainEvent!.UserId.Should().Be(user.Id);
        domainEvent.TenantId.Should().Be(TenantId);
        domainEvent.Email.Should().Be(Email);
        domainEvent.Username.Should().Be("testuser");
    }

    [Fact]
    public void Create_WithPhoneNumber_ShouldSetPhoneNumber()
    {
        var phone = PhoneNumber.Create("712345678");
        var user = User.Create(TenantId, "phoneuser", Email, "Phone", "User", phone);

        user.PhoneNumber.Should().Be(phone);
    }

    [Fact]
    public void Create_WithExternalId_ShouldSetExternalId()
    {
        var user = User.Create(TenantId, "extuser", Email, "Ext", "User", null, "ext-123");

        user.ExternalId.Should().Be("ext-123");
    }

    [Fact]
    public void Deactivate_ShouldSetInactive()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldRaiseUserDeactivatedDomainEvent()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        user.ClearDomainEvents();

        user.Deactivate();

        user.DomainEvents.Should().ContainSingle(e => e is UserDeactivatedDomainEvent);
        var domainEvent = user.DomainEvents.First() as UserDeactivatedDomainEvent;
        domainEvent!.UserId.Should().Be(user.Id);
        domainEvent.TenantId.Should().Be(TenantId);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotRaiseEvent()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        user.Deactivate();
        user.ClearDomainEvents();

        user.Deactivate();

        user.DomainEvents.Should().BeEmpty();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetActive()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldNotChange()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void VerifyEmail_ShouldSetEmailVerified()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");

        user.VerifyEmail();

        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void VerifyEmail_WhenAlreadyVerified_ShouldNotChange()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        user.VerifyEmail();

        user.VerifyEmail();

        user.EmailVerified.Should().BeTrue();
    }

    [Fact]
    public void UpdateProfile_ShouldUpdateFields()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        var newPhone = PhoneNumber.Create("712345678");

        user.UpdateProfile("NewFirst", "NewLast", newPhone);

        user.FirstName.Should().Be("NewFirst");
        user.LastName.Should().Be("NewLast");
        user.PhoneNumber.Should().Be(newPhone);
    }

    [Fact]
    public void UpdateProfile_WithNullPhone_ShouldClearPhone()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User", PhoneNumber.Create("712345678"));

        user.UpdateProfile("Test", "User", null);

        user.PhoneNumber.Should().BeNull();
    }

    [Fact]
    public void SetLastLogin_ShouldSetTimestamp()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");

        user.SetLastLogin();

        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AssignRole_ShouldAddUserRole()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        var roleId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        user.AssignRole(roleId, assignedBy);

        user.UserRoles.Should().ContainSingle();
        user.UserRoles.First().RoleId.Should().Be(roleId);
        user.UserRoles.First().AssignedBy.Should().Be(assignedBy);
        user.UserRoles.First().UserId.Should().Be(user.Id);
    }

    [Fact]
    public void AssignRole_WhenRoleAlreadyAssigned_ShouldNotDuplicate()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId, Guid.NewGuid());

        user.AssignRole(roleId, Guid.NewGuid());

        user.UserRoles.Should().ContainSingle();
    }

    [Fact]
    public void RemoveRole_ShouldRemoveUserRole()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");
        var roleId = Guid.NewGuid();
        user.AssignRole(roleId, Guid.NewGuid());

        user.RemoveRole(roleId);

        user.UserRoles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRole_WhenRoleNotAssigned_ShouldNotThrow()
    {
        var user = User.Create(TenantId, "testuser", Email, "Test", "User");

        var act = () => user.RemoveRole(Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void TwoUsersWithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var tenantId1 = TenantId;
        var tenantId2 = TenantId.Create(TenantId.Value);
        var email1 = Email;
        var email2 = SharedKernel.Domain.ValueObjects.Email.Create("test@example.com");
        var user1 = User.Create(tenantId1, "user1", email1, "First", "User");
        var user2 = User.Create(tenantId2, "user2", email2, "Second", "User");

        var field = typeof(SharedKernel.Domain.Common.Entity<Guid>)
            .GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)!;
        field.SetValue(user1, id);
        field.SetValue(user2, id);

        user1.Equals(user2).Should().BeTrue();
        (user1 == user2).Should().BeTrue();
        (user1 != user2).Should().BeFalse();
    }
}
