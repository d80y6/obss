using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.IAM.Domain.Events;

namespace Obss.IAM.Domain.Entities;

public class User : AggregateRoot<Guid>
{
    private User() { }

    private User(
        Guid id,
        TenantId tenantId,
        string username,
        Email email,
        string firstName,
        string lastName,
        PhoneNumber? phoneNumber,
        string? externalId,
        string? title,
        string? middleName,
        DateTime? birthDate,
        string? nationalId,
        string? preferredLanguage,
        string? gender)
        : base(id)
    {
        TenantId = tenantId;
        Username = username;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        ExternalId = externalId;
        Title = title;
        MiddleName = middleName;
        BirthDate = birthDate;
        NationalId = nationalId;
        PreferredLanguage = preferredLanguage;
        Gender = gender;
        IsActive = true;
        EmailVerified = false;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserCreatedDomainEvent(id, tenantId, email, username));
    }

    public TenantId TenantId { get; private set; } = default!;
    public string Username { get; private set; } = string.Empty;
    public Email Email { get; private set; } = default!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public PhoneNumber? PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }
    public bool EmailVerified { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? ExternalId { get; private set; }
    public string? Title { get; private set; }
    public string? MiddleName { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? NationalId { get; private set; }
    public string? PreferredLanguage { get; private set; }
    public string? Gender { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<UserRole> _userRoles = [];
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public static User Create(
        TenantId tenantId,
        string username,
        Email email,
        string firstName,
        string lastName,
        PhoneNumber? phoneNumber = null,
        string? externalId = null,
        string? title = null,
        string? middleName = null,
        DateTime? birthDate = null,
        string? nationalId = null,
        string? preferredLanguage = null,
        string? gender = null)
    {
        return new User(
            Guid.NewGuid(),
            tenantId,
            username,
            email,
            firstName,
            lastName,
            phoneNumber,
            externalId,
            title,
            middleName,
            birthDate,
            nationalId,
            preferredLanguage,
            gender);
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserDeactivatedDomainEvent(Id, TenantId));
    }

    public void VerifyEmail()
    {
        if (EmailVerified)
            return;

        EmailVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName, PhoneNumber? phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePartyProfile(
        string? title = null,
        string? middleName = null,
        DateTime? birthDate = null,
        string? nationalId = null,
        string? preferredLanguage = null,
        string? gender = null)
    {
        if (title is not null) Title = title;
        if (middleName is not null) MiddleName = middleName;
        if (birthDate.HasValue) BirthDate = birthDate;
        if (nationalId is not null) NationalId = nationalId;
        if (preferredLanguage is not null) PreferredLanguage = preferredLanguage;
        if (gender is not null) Gender = gender;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignRole(Guid roleId, Guid assignedBy)
    {
        if (_userRoles.Any(ur => ur.RoleId == roleId))
            return;

        _userRoles.Add(new UserRole(Guid.NewGuid(), Id, roleId, assignedBy));
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(Guid roleId)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == roleId);
        if (userRole is null)
            return;

        _userRoles.Remove(userRole);
        UpdatedAt = DateTime.UtcNow;
    }
}
